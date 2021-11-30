using Common;
using CommonUtil;
using Platform;
using System.Collections.Generic;
using Wax;

namespace JavaScriptApp
{
    public class PlatformImpl : AbstractPlatform
    {
        public override string Name { get { return "javascript-app"; } }
        public override string NL { get { return "\n"; } }

        public PlatformImpl()
            : base("JAVASCRIPT")
        { }

        public override IDictionary<string, object> GetConstantFlags()
        {
            return new Dictionary<string, object>();
        }

        private static readonly string[] noriFiles = new string[] {
            "messagehub.js",
            "nori.js",
            "nori_audio.js",
            "nori_canvas.js",
            "nori_context.js",
            "nori_events.js",
            "nori_gamepad.js",
            "nori_layout.js",
            "nori_util.js",
            "shim.js",
        };

        public override void ExportProject(
            Dictionary<string, FileOutput> output,
            BuildData buildData,
            ExportProperties exportProperties)
        {
            List<string> jsExtraHead = new List<string>() { exportProperties.JsHeadExtras ?? "" };

            if (exportProperties.JsFullPage)
            {
                jsExtraHead.Add(
                    "<script type=\"text/javascript\">"
                    + "C$common$globalOptions['fullscreen'] = true;"
                    + "</script>");
            }

            if (buildData.UsesU3)
            {
                Dictionary<string, FileOutput> u3Files = new Dictionary<string, FileOutput>();
                foreach (string file in noriFiles)
                {
                    this.CopyResourceAsText(u3Files, file, "ResourcesU3/" + file, new Dictionary<string, string>());
                }
                List<string> newFile = new List<string>();
                foreach (string file in noriFiles)
                {
                    newFile.Add("// From " + file);
                    newFile.Add(u3Files[file].TextContent.Trim());
                }
                output["u3.js"] = new FileOutput()
                {
                    Type = FileOutputType.Text,
                    TextContent = string.Join("\n", newFile),
                };
                jsExtraHead.Add("<script src=\"u3.js\"></script>");
            }

            exportProperties.JsHeadExtras = string.Join("\n", jsExtraHead);

            ResourceDatabase resDb = buildData.CbxBundle.ResourceDB;
            Dictionary<string, string> replacements = this.GenerateReplacementDictionary(exportProperties, buildData);

            TemplateReader templateReader = new TemplateReader(new PkgAwareFileUtil(), this);
            TemplateSet vmTemplates = templateReader.GetVmTemplates();

            output["vm.js"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = vmTemplates.GetText("vm.js"),
            };

            Dictionary<string, string> htmlReplacements = new Dictionary<string, string>(replacements);

            this.CopyResourceAsText(output, "index.html", "Resources/HostHtml.txt", replacements);
            this.CopyResourceAsText(output, "test_server.py", "Resources/TestServerPy.txt", replacements);

            this.CopyResourceAsText(output, "common.js", "Resources/Common.txt", replacements);

            System.Text.StringBuilder resourcesJs = new System.Text.StringBuilder();

            foreach (FileOutput textResource in resDb.TextResources)
            {
                resourcesJs.Append("C$common$addTextRes(");
                resourcesJs.Append(ConvertStringValueToCode(textResource.CanonicalFileName));
                resourcesJs.Append(", ");
                resourcesJs.Append(ConvertStringValueToCode(textResource.TextContent));
                resourcesJs.Append(");\n");
            }

            foreach (FileOutput fontResource in resDb.FontResources)
            {
                resourcesJs.Append("C$common$addBinaryRes(");
                resourcesJs.Append(ConvertStringValueToCode(fontResource.CanonicalFileName));
                resourcesJs.Append(", '");
                resourcesJs.Append(Base64.ToBase64(fontResource.GetFinalBinaryContent()));
                resourcesJs.Append("');\n");
            }

            resourcesJs.Append("C$common$resourceManifest = ");
            resourcesJs.Append(ConvertStringValueToCode(resDb.ResourceManifestFile.TextContent));
            resourcesJs.Append(";\n");

            string filePrefix = exportProperties.JsFilePrefix;
            if (filePrefix != null)
            {
                resourcesJs.Append("C$common$jsFilePrefix = ");
                resourcesJs.Append(ConvertStringValueToCode(filePrefix));
                resourcesJs.Append(";\n");
            }

            string imageManifest = resDb.ImageResourceManifestFile.TextContent;
            imageManifest = ConvertStringValueToCode(imageManifest);
            resourcesJs.Append("C$common$imageManifest = " + imageManifest + ";\n");

            output["resources.js"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = resourcesJs.ToString(),
            };

            output["bytecode.js"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = "C$bytecode = " + ConvertStringValueToCode(buildData.CbxBundle.ByteCode) + ";",
            };

            foreach (string imageChunk in resDb.ImageResourceFiles.Keys)
            {
                output["resources/images/" + imageChunk] = resDb.ImageResourceFiles[imageChunk];
            }

            foreach (FileOutput audioResourceFile in resDb.AudioResources)
            {
                output["resources/audio/" + audioResourceFile.CanonicalFileName] = audioResourceFile;
            }

            if (exportProperties.HasIcon)
            {
                this.GenerateIconFile(output, "favicon.ico", exportProperties);
            }

            // TODO: minify JavaScript across all of output dictionary
        }

        public override Dictionary<string, string> GenerateReplacementDictionary(
            ExportProperties exportProperties,
            BuildData buildData)
        {
            return CommonUtil.Collections.DictionaryUtil.MergeDictionaries(
                this.ParentPlatform.GenerateReplacementDictionary(exportProperties, buildData),
                new Dictionary<string, string>()
                {
                    { "PROJECT_TITLE", exportProperties.ProjectTitle },
                    {
                        "FAVICON",
                        exportProperties.HasIcon
                            ? "<link rel=\"shortcut icon\" href=\"" + exportProperties.JsFilePrefix + "favicon.ico\">"
                            : ""
                    },
                    {
                        "CSS_EXTRA",
                        exportProperties.JsFullPage
                            ? ("#crayon_host { background-color:#000; text-align:left; width: 100%; height: 100%; }\n"
                                + "body { overflow:hidden; }")
                            : ""
                    },
                    { "JS_EXTRA_HEAD", exportProperties.JsHeadExtras },
                });
        }

        private static string ConvertStringValueToCode(string rawValue)
        {
            List<string> output = new List<string>() { "\"" };
            foreach (char c in rawValue)
            {
                switch (c)
                {
                    case '"': output.Add("\\\""); break;
                    case '\n': output.Add("\\n"); break;
                    case '\r': output.Add("\\r"); break;
                    case '\0': output.Add("\\0"); break;
                    case '\t': output.Add("\\t"); break;
                    case '\\': output.Add("\\\\"); break;
                    default: output.Add("" + c); break;
                }
            }
            output.Add("\"");

            return string.Join("", output);
        }
    }
}
