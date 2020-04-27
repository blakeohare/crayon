using Common;
using Platform;
using System.Collections.Generic;
using System.Linq;

namespace PhpServer
{
    public class PlatformImpl : AbstractPlatform
    {
        public PlatformImpl() : base("PHP") { }

        public override string Name { get { return "php-server"; } }
        public override string InheritsFrom { get { return "lang-php"; } }
        public override string NL { get { return "\n"; } }

        private string ConvertStringToVariableSetterFile(string value, string variableName)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("<?php\n");
            sb.Append("\t$");
            sb.Append(variableName);
            sb.Append(" = \"");
            string byteCode = value;
            int length = byteCode.Length;
            char c;
            for (int i = 0; i < length; ++i)
            {
                c = byteCode[i];
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '$': sb.Append("\\$"); break;
                    default: sb.Append(c); break;
                }
            }
            sb.Append("\";\n");
            sb.Append("?>");
            return sb.ToString();
        }

        public override void ExportProject(
            Dictionary<string, FileOutput> output,
            string byteCode,
            IList<LibraryForExport> libraries,
            Build.ResourceDatabase resourceDatabase,
            Options options)
        {
            Dictionary<string, string> replacements = this.GenerateReplacementDictionary(options, resourceDatabase);
            TemplateReader templates = new TemplateReader(new PkgAwareFileUtil(), this);
            TemplateSet vmTemplates = templates.GetVmTemplates();
            string functions = vmTemplates.GetText("functions.php");
            string structs = vmTemplates.GetText("structs.php");
            string byteCodeFile = ConvertStringToVariableSetterFile(byteCode, "_CRAYON_BYTE_CODE");
            string resourceManifest = ConvertStringToVariableSetterFile(resourceDatabase.ResourceManifestFile.TextContent, "_CRAYON_RESOURCE_MANIFEST");
            output["crayon_gen/bytecode.php"] = FileOutput.OfString(byteCodeFile);
            output["crayon_gen/resource_manifest.php"] = FileOutput.OfString(resourceManifest);
            output["crayon_gen/functions.php"] = FileOutput.OfString(functions);
            output["crayon_gen/structs.php"] = FileOutput.OfString(structs);
            output["index.php"] = FileOutput.OfString(this.LoadTextResource("Resources/index.php", replacements));
            output[".htaccess"] = FileOutput.OfString(this.LoadTextResource("Resources/htaccess.txt", replacements));

            List<string> libsIncluder = new List<string>() { "<?php" };

            foreach (LibraryForExport library in libraries.Where(lib => lib.HasNativeCode))
            {
                foreach (string key in library.ExportEntities.Keys)
                {
                    foreach (ExportEntity entity in library.ExportEntities[key])
                    {
                        switch (key)
                        {
                            case "COPY_CODE":
                                string target = entity.Values["target"];
                                string fileContent = entity.FileOutput.TextContent;
                                int lastLine = fileContent.LastIndexOf('\n');
                                fileContent = fileContent.Substring(0, lastLine); // trim off '?>'
                                fileContent = fileContent.TrimEnd() + string.Join("\n", new string[] {
                                    "",
                                    "",
                                    "\t$_CRAYON_LIBS['" + library.Name + "'] = crayon_generateFunctionLookup('CrayonLibWrapper_" + library.Name + "');",
                                    "?>",
                                });

                                output["crayon_gen/" + target] = FileOutput.OfString(fileContent);
                                libsIncluder.Add("\trequire 'crayon_gen/" + target + "';");
                                break;

                            default:
                                throw new System.NotImplementedException();
                        }
                    }
                }
            }

            libsIncluder.Add("?>");
            output["crayon_gen/libs.php"] = FileOutput.OfString(string.Join("\n", libsIncluder));
        }

        public override void ExportStandaloneVm(
            Dictionary<string, FileOutput> output,
            IList<LibraryForExport> everyLibrary)
        {
            throw new System.NotImplementedException();
        }

        public override Dictionary<string, string> GenerateReplacementDictionary(
            Options options,
            Build.ResourceDatabase resDb)
        {
            return this.ParentPlatform.GenerateReplacementDictionary(options, resDb);
        }

        public override IDictionary<string, object> GetConstantFlags()
        {
            return new Dictionary<string, object>();
        }
    }
}
