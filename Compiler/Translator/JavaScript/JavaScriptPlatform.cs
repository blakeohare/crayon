using System.Collections.Generic;
using System.Linq;

namespace Crayon.Translator.JavaScript
{
    class JavaScriptPlatform : AbstractPlatform
    {
        private string jsFolderPrefix;

        public JavaScriptPlatform(bool isMin, string jsFolderPrefix)
            : base(PlatformId.JAVASCRIPT_CANVAS, LanguageId.JAVASCRIPT, isMin, new JavaScriptTranslator(), new JavaScriptSystemFunctionTranslator(), false)
        {
            this.jsFolderPrefix = jsFolderPrefix;
        }

        public override bool IsAsync { get { return true; } }
        public override bool SupportsListClear { get { return false; } }
        public override bool IsStronglyTyped { get { return false; } }
        public override bool ImagesLoadInstantly { get { return false; } }
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
            string fileCopySourceRoot,
            ResourceDatabase resourceDatabase)
        {
            Dictionary<string, FileOutput> output = new Dictionary<string, FileOutput>();

            Dictionary<string, string> replacements = new Dictionary<string, string>()
            {
                { "PROJECT_ID", projectId },

                // TODO: create a field in the build file
                { "BUILD_FILE_DEFINED_TITLE", "Untitled" },
            };

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
                "file_io.js",
                "fake_disk.js",
                "http.js",
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

            string codeJsText = Constants.DoReplacements(string.Join("", codeJs), replacements);

            if (this.IsMin)
            {
                codeJsText = JavaScriptMinifier.Minify(codeJsText);
            }

            output["code.js"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = codeJsText
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

            textResources["imageresmanifest.txt"] = resourceDatabase.ImageSheetManifestFile.TextContent;

            foreach (FileOutput audioFile in resourceDatabase.AudioResources)
            {
                output["resources/audio/" + audioFile.CanonicalFileName] = audioFile;
            }

            output["resources.js"] = new FileOutput()
            {
                TextContent = BuildTextResourcesCodeFile(textResources),
                Type = FileOutputType.Text
            };

            output["index.html"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = Constants.DoReplacements(this.GenerateHtmlFile(), replacements),
            };

            if (this.IsMin)
            {
                // TODO: redo this so that minification is smarter.
                // When you do, you'll have to solve the problem of functions being invoked from handlers in HTML (templates and generated).
                foreach (string filename in output.Keys.Where(name => name.EndsWith(".js")))
                {
                    FileOutput file = output[filename];
                    file.TextContent = this.MinifyCode(file.TextContent);
                }
            }

            return output;
        }

        private string BuildTextResourcesCodeFile(Dictionary<string, string> files)
        {
            List<string> output = new List<string>();
            output.Add("C$common$jsFilePrefix = '" + this.jsFolderPrefix + "';\n");
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
            if (!this.IsMin)
            {
                return data;
            }

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
