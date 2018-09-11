using Common;
using Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JavaScriptApp
{
    public class PlatformImpl : AbstractPlatform
    {
        public override string Name { get { return "javascript-app"; } }
        public override string InheritsFrom { get { return "lang-javascript"; } }
        public override string NL { get { return "\n"; } }

        public PlatformImpl()
            : base("JAVASCRIPT")
        { }

        public override IDictionary<string, object> GetConstantFlags()
        {
            return new Dictionary<string, object>();
        }

        public override void ExportStandaloneVm(
            Dictionary<string, FileOutput> output,
            TemplateStorage templates,
            IList<LibraryForExport> everyLibrary)
        {
            throw new NotImplementedException();
        }

        public override void ExportProject(
            Dictionary<string, FileOutput> output,
            TemplateStorage templates,
            IList<LibraryForExport> libraries,
            ResourceDatabase resourceDatabase,
            Options options)
        {
            this.ExportProjectImpl(
                output,
                templates,
                libraries,
                resourceDatabase,
                options);
        }

        public void ExportProjectImpl(
            Dictionary<string, FileOutput> output,
            TemplateStorage templates,
            IList<LibraryForExport> libraries,
            ResourceDatabase resourceDatabase,
            Options options)
        {
            List<string> jsExtraHead = new List<string>() { options.GetStringOrEmpty(ExportOptionKey.JS_HEAD_EXTRAS) };
            bool fullPage = options.GetBool(ExportOptionKey.JS_FULL_PAGE);

            // There's a double-check here so that you can || them together and then have multiple options added here.
            if (fullPage)
            {
                jsExtraHead.Add(
                    "<script type=\"text/javascript\">"
                    + (fullPage ? "C$common$globalOptions['fullscreen'] = true;" : "")
                    + "</script>");
            }
            options.SetOption(ExportOptionKey.JS_HEAD_EXTRAS, string.Join("\n", jsExtraHead));

            Dictionary<string, string> replacements = this.GenerateReplacementDictionary(options, resourceDatabase);

            output["vm.js"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = templates.GetCode("vm:functions"),
            };

            List<LibraryForExport> librariesWithCode = new List<LibraryForExport>();
            foreach (LibraryForExport library in libraries)
            {
                if (library.HasPastelCode)
                {
                    string libraryName = library.Name;

                    List<string> libraryLines = new List<string>();
                    libraryLines.Add(templates.GetCode("library:" + libraryName + ":manifestfunc"));
                    libraryLines.Add("");
                    libraryLines.Add(templates.GetCode("library:" + libraryName + ":functions"));
                    libraryLines.Add("");
                    libraryLines.Add("C$common$scrapeLibFuncNames('" + libraryName.ToLower() + "');");
                    libraryLines.Add("");

                    // add helper functions after the scrape.

                    foreach (ExportEntity embedCode in library.ExportEntities["EMBED_CODE"])
                    {
                        libraryLines.Add(embedCode.StringValue);
                    }

                    output["libs/lib_" + libraryName.ToLower() + ".js"] = new FileOutput()
                    {
                        Type = FileOutputType.Text,
                        TextContent = string.Join("\n", libraryLines),
                    };
                    librariesWithCode.Add(library);
                }
            }

            Dictionary<string, string> htmlReplacements = new Dictionary<string, string>(replacements);
            replacements["JS_LIB_INCLUSIONS"] = GenerateJsLibInclusionHtml(output.Keys);

            this.CopyResourceAsText(output, "index.html", "Resources/HostHtml.txt", replacements);

            this.CopyResourceAsText(output, "common.js", "Resources/Common.txt", replacements);

            StringBuilder resourcesJs = new StringBuilder();

            foreach (FileOutput textResource in resourceDatabase.TextResources)
            {
                resourcesJs.Append("C$common$addTextRes(");
                resourcesJs.Append(Util.ConvertStringValueToCode(textResource.CanonicalFileName));
                resourcesJs.Append(", ");
                resourcesJs.Append(Util.ConvertStringValueToCode(textResource.TextContent));
                resourcesJs.Append(");\n");
            }

            foreach (FileOutput fontResource in resourceDatabase.FontResources)
            {
                resourcesJs.Append("C$common$addBinaryRes(");
                resourcesJs.Append(Util.ConvertStringValueToCode(fontResource.CanonicalFileName));
                resourcesJs.Append(", '");
                resourcesJs.Append(Util.ConvertByteArrayToBase64(fontResource.GetFinalBinaryContent()));
                resourcesJs.Append("');\n");
            }

            FileOutput imageSheetManifest = resourceDatabase.ImageSheetManifestFile;
            resourcesJs.Append("C$common$addTextRes('image_sheets.txt', ");
            resourcesJs.Append(imageSheetManifest == null ? "''" : Util.ConvertStringValueToCode(imageSheetManifest.TextContent));
            resourcesJs.Append(");\n");

            resourcesJs.Append("C$common$resourceManifest = ");
            resourcesJs.Append(Util.ConvertStringValueToCode(resourceDatabase.ResourceManifestFile.TextContent));
            resourcesJs.Append(";\n");

            string filePrefix = options.GetStringOrNull(ExportOptionKey.JS_FILE_PREFIX);
            if (filePrefix != null)
            {
                resourcesJs.Append("C$common$jsFilePrefix = ");
                resourcesJs.Append(Util.ConvertStringValueToCode(filePrefix));
                resourcesJs.Append(";\n");
            }

            output["resources.js"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = resourcesJs.ToString(),
            };

            output["bytecode.js"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = "C$bytecode = " + Util.ConvertStringValueToCode(resourceDatabase.ByteCodeFile.TextContent) + ";",
            };

            foreach (string imageResourceFile in resourceDatabase.ImageSheetFiles.Keys)
            {
                FileOutput file = resourceDatabase.ImageSheetFiles[imageResourceFile];
                output["resources/images/" + imageResourceFile] = file;
            }

            foreach (FileOutput audioResourceFile in resourceDatabase.AudioResources)
            {
                output["resources/audio/" + audioResourceFile.CanonicalFileName] = audioResourceFile;
            }

            // TODO: minify JavaScript across all of output dictionary
        }

        public static string GenerateJsLibInclusionHtml(ICollection<string> filesIncluded)
        {
            string[] libraryPathsIncluded = filesIncluded.Where(s => s.StartsWith("libs/lib_")).OrderBy(s => s).ToArray();
            if (libraryPathsIncluded.Length > 0)
            {
                return
                    IndentCodeWithTabs(
                        "<script type=\"text/javascript\" src=\"" +
                        string.Join(
                            "\"></script>\n<script type=\"text/javascript\" src=\"",
                            libraryPathsIncluded) +
                        "\"></script>",
                    2);
            }
            return "";
        }

        public override Dictionary<string, string> GenerateReplacementDictionary(
            Options options,
            ResourceDatabase resDb)
        {
            return Util.MergeDictionaries(
                this.ParentPlatform.GenerateReplacementDictionary(options, resDb),
                new Dictionary<string, string>()
                {
                    { "DEFAULT_TITLE", options.GetString(ExportOptionKey.DEFAULT_TITLE, "Untitled") },
                    {
                        "FAVICON",
                        options.GetBool(ExportOptionKey.HAS_ICON)
                            ? "<link rel=\"shortcut icon\" href=\"" + options.GetStringOrEmpty(ExportOptionKey.JS_FILE_PREFIX) + "favicon.ico\">"
                            : ""
                    },
                    {
                        "CSS_EXTRA",
                        options.GetBool(ExportOptionKey.JS_FULL_PAGE)
                            ? ("#crayon_host { background-color:#000; text-align:left; width: 100%; height: 100%; }\n"
                                + "body { overflow:hidden; }")
                            : ""
                    },
                    { "JS_EXTRA_HEAD", options.GetStringOrEmpty(ExportOptionKey.JS_HEAD_EXTRAS) },
                });
        }
    }
}
