using System.Collections.Generic;
using System.Linq;
using Common;

namespace Crayon.Translator.JavaScript
{
    class JavaScriptPlatform : AbstractPlatform
    {
        public JavaScriptPlatform()
            : base(PlatformId.JAVASCRIPT_CANVAS, LanguageId.JAVASCRIPT, new JavaScriptTranslator(), new JavaScriptSystemFunctionTranslator())
        { }

        public override bool IsAsync { get { return true; } }
        public override bool SupportsListClear { get { return false; } }
        public override bool IsStronglyTyped { get { return false; } }
        public override bool IsArraySameAsList { get { return true; } }
        public override bool IsCharANumber { get { return false; } }
        public override bool IntIsFloor { get { return true; } }
        public override bool IsThreadBlockingAllowed { get { return false; } }
        public override string PlatformShortId { get { return "game-javascript"; } }

        public override Dictionary<string, FileOutput> Package(
            BuildContext buildContext,
            string projectId,
            Dictionary<string, ParseTree.Executable[]> finalCode,
            ICollection<ParseTree.StructDefinition> structDefinitions,
            ResourceDatabase resourceDatabase,
            SystemLibraryManager libraryManager)
        {
            string jsFilePrefix = "";
            if (buildContext.JsFilePrefix != null)
            {
                jsFilePrefix = buildContext.JsFilePrefix;
                if (!jsFilePrefix.StartsWith("/")) jsFilePrefix = "/" + jsFilePrefix;
                if (!jsFilePrefix.EndsWith("/")) jsFilePrefix += "/";
            }

            Dictionary<string, FileOutput> output = new Dictionary<string, FileOutput>();

            bool hasIcon = buildContext.IconFilePath != null;

            Dictionary<string, string> replacements = new Dictionary<string, string>()
            {
                { "PROJECT_ID", projectId },
                { "DEFAULT_TITLE", buildContext.DefaultTitle ?? "Untitled" },
                { "FAVICON", hasIcon ? "<link rel=\"shortcut icon\" href=\"" + jsFilePrefix + "favicon.ico\">" : "" },
            };

            if (hasIcon)
            {
                output["favicon.ico"] = new FileOutput()
                {
                    Type = FileOutputType.Binary,
                    BinaryContent = Util.GetIconFileBytesFromImageFile(buildContext.IconFilePath),
                };
            }

            output["bytecode.js"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent =
                    "C$bytecode = " + Util.ConvertStringValueToCode(resourceDatabase.ByteCodeFile.TextContent, true) + ";\n" +
                    "C$resourceManifest = " + Util.ConvertStringValueToCode(resourceDatabase.ResourceManifestFile.TextContent, true) + ";",
            };

            List<string> codeJs = new List<string>();

            foreach (string jsFile in new string[] {
                "common.js",
            })
            {
                codeJs.Add(Util.ReadResourceFileInternally("javascript-common/" + jsFile));
                codeJs.Add(this.Translator.NL);
            }

            foreach (string jsFile in new string[] {
                "game.js",
                "input.js",
                "imageresources.js",
                "drawing.js",
                "sound.js",
                "gamepad.js",
            })
            {
                codeJs.Add(Util.ReadResourceFileInternally("game-javascript/" + jsFile));
                codeJs.Add(this.Translator.NL);
            }

            foreach (string component in finalCode.Keys)
            {
                this.Translator.Translate(codeJs, finalCode[component]);
                codeJs.Add(this.Translator.NL);
            }

            string codeJsText = Constants.DoReplacements(false, string.Join("", codeJs), replacements);

            output["code.js"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = codeJsText
            };


            output["libraries.js"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = libraryManager.EmbeddedContent,
            };

            Dictionary<string, string> textResources = new Dictionary<string, string>();
            foreach (FileOutput textFile in resourceDatabase.TextResources)
            {
                textResources["text/" + textFile.CanonicalFileName] = textFile.TextContent;
            }

            foreach (string filename in resourceDatabase.ImageSheetFiles.Keys)
            {
                FileOutput imageSheetFile = resourceDatabase.ImageSheetFiles[filename];
                output["resources/images/" + filename] = imageSheetFile;
            }

            foreach (FileOutput imageResource in resourceDatabase.ImageResources)
            {
                output["resources/images/" + imageResource.CanonicalFileName] = imageResource;
            }

            if (resourceDatabase.ImageSheetManifestFile != null)
            {
                textResources["imageresmanifest.txt"] = resourceDatabase.ImageSheetManifestFile.TextContent;
            }

            foreach (FileOutput audioFile in resourceDatabase.AudioResources)
            {
                output["resources/audio/" + audioFile.CanonicalFileName] = audioFile;
            }

            output["resources.js"] = new FileOutput()
            {
                TextContent = BuildTextResourcesCodeFile(textResources, jsFilePrefix),
                Type = FileOutputType.Text,
            };

            output["index.html"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = Constants.DoReplacements(false, this.GenerateHtmlFile(), replacements),
            };

            return output;
        }

        private string BuildTextResourcesCodeFile(Dictionary<string, string> files, string jsFilePrefix)
        {
            List<string> output = new List<string>();
            output.Add("C$common$jsFilePrefix = '" + jsFilePrefix + "';\n");
            string[] keys = files.Keys.OrderBy<string, string>(s => s.ToLowerInvariant()).ToArray();
            for (int i = 0; i < keys.Length; ++i)
            {
                string filename = keys[i];
                output.Add("C$common$addTextRes(");
                output.Add(Util.ConvertStringValueToCode(filename, true));
                output.Add(", ");
                output.Add(Util.ConvertStringValueToCode(files[filename], true));
                output.Add(");\n");
            }
            return string.Join("", output);
        }

        // TODO: minify variable names by actually parsing the code and resolving stuff.
        private string MinifyCode(string data)
        {
            string[] lines = data.Split('\n');
            List<string> output = new List<string>();
            int counter = -1;
            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();
                int commentLoc = line.IndexOf("//");
                if (commentLoc != -1)
                {
                    line = line.Substring(0, commentLoc);
                }

                line = line
                    .Replace(" = ", "=")
                    .Replace(", ", ",")
                    .Replace(" + ", "+")
                    .Replace(" == ", "==")
                    .Replace(" += ", "+=")
                    .Replace(" -= ", "-=")
                    .Replace(" - ", "-")
                    .Replace(" / ", "/")
                    .Replace("function (", "function(")
                    .Replace(") {", "){");
                if (counter-- > 0)
                {
                    line += "\r\n";
                }
                output.Add(line);

            }

            return string.Join("", output);
        }

        public string GenerateHtmlFile()
        {
            return Util.ReadResourceFileInternally("game-javascript/game_host_html.txt");
        }
    }
}
