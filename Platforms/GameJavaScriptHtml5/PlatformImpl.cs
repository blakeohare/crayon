using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Pastel.Nodes;
using Platform;

namespace GameJavaScriptHtml5
{
    public class PlatformImpl : AbstractPlatform
    {
        public override string Name { get { return "game-javascript-html5-cbx"; } }
        public override string InheritsFrom { get { return "lang-javascript"; } }
        public override string NL { get { return "\n"; } }
        
        public PlatformImpl()
        {
            this.Translator = new JavaScriptGameHtml5Translator(this);
        }

        public override IDictionary<string, object> GetConstantFlags()
        {
            return new Dictionary<string, object>();
        }

        public override Dictionary<string, FileOutput> Export(
            Dictionary<string, object[]> executablesPerCompilationUnit,
            object[] structDefinitions)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<string, FileOutput> ExportProject(
            IList<VariableDeclaration> globals,
            IList<StructDefinition> structDefinitions,
            IList<FunctionDefinition> functionDefinitions,
            IList<LibraryForExport> libraries,
            ResourceDatabase resourceDatabase,
            Options options,
            ILibraryNativeInvocationTranslatorProvider libraryNativeInvocationTranslatorProviderForPlatform)
        {
            Dictionary<string, FileOutput> output = new Dictionary<string, FileOutput>();
            Dictionary<string, string> replacements = this.GenerateReplacementDictionary(options, resourceDatabase);

            List<string> coreVmFunctions = new List<string>();
            foreach (FunctionDefinition funcDef in functionDefinitions)
            {
                coreVmFunctions.Add(this.GenerateCodeForFunction(this.Translator, funcDef));
            }

            string functionCode = string.Join("\r\n\r\n", coreVmFunctions);

            output["vm.js"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = functionCode,
            };

            List<LibraryForExport> librariesWithCode = new List<LibraryForExport>();
            foreach (LibraryForExport library in libraries)
            {
                List<string> libraryLines = new List<string>();

                this.Translator.CurrentLibraryFunctionTranslator = 
                    libraryNativeInvocationTranslatorProviderForPlatform.GetTranslator(library.Name);
                foreach (FunctionDefinition fnDef in library.Functions)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("var v_");
                    sb.Append(fnDef.NameToken.Value);
                    sb.Append(" = function(");
                    for (int i = 0; i < fnDef.ArgNames.Length; ++i)
                    {
                        if (i > 0) sb.Append(", ");
                        sb.Append("v_");
                        sb.Append(fnDef.ArgNames[i].Value);
                    }
                    sb.Append(") {\n");
                    this.Translator.TabDepth = 1;
                    this.Translator.TranslateExecutables(sb, fnDef.Code);
                    this.Translator.TabDepth = 0;
                    sb.Append("};\n");
                    libraryLines.Add(sb.ToString());
                }
                
                if (libraryLines.Count > 0)
                {
                    output["libs/lib_" + library.Name.ToLower() + ".js"] = new FileOutput()
                    {
                        Type = FileOutputType.Text,
                        TextContent = string.Join("\n", libraryLines),
                    };
                    librariesWithCode.Add(library);
                }
            }

            Dictionary<string, string> htmlReplacements = new Dictionary<string, string>(replacements);
            replacements["JS_LIB_INCLUSIONS"] = this.GenerateJsLibInclusionHtml(librariesWithCode);
            this.CopyResourceAsText(output, "index.html", "Resources/GameHostHtml.txt", replacements);

            this.CopyResourceAsText(output, "common.js", "Resources/Common.txt", replacements);

            // TODO: this needs to be included as part of libraries
            output["lib_supplemental.js"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = string.Join("\n\n", new string[] {
                    this.LoadTextResource("Resources/ImageResource.txt", replacements),
                    this.LoadTextResource("Resources/Game.txt", replacements),
                    this.LoadTextResource("Resources/Drawing.txt", replacements),
                    this.LoadTextResource("Resources/Gamepad.txt", replacements),
                    this.LoadTextResource("Resources/Input.txt", replacements),
                    this.LoadTextResource("Resources/Sound.txt", replacements),
                }),
            };
            
            // TODO: minify JavaScript across all of output dictionary

            return output;
        }

        public override string GenerateCodeForStruct(StructDefinition structDef)
        {
            throw new NotImplementedException();
        }

        public override string GenerateCodeForFunction(AbstractTranslator translator, FunctionDefinition funcDef)
        {
            return this.ParentPlatform.GenerateCodeForFunction(translator, funcDef);
        }

        public override string GenerateCodeForGlobalsDefinitions(AbstractTranslator translator, IList<VariableDeclaration> globals)
        {
            throw new NotImplementedException();
        }

        private string GenerateJsLibInclusionHtml(List<LibraryForExport> librariesWithCode)
        {
            string[] librariesIncluded = librariesWithCode.Select(lib => lib.Name.ToLower()).ToArray();
            if (librariesIncluded.Length > 0)
            {
                return
                    this.IndentCodeWithTabs(
                        "<script type=\"text/javascript\" src=\"libs/lib_" +
                        string.Join(
                            ".js\"></script>\n<script type=\"text/javascript\" src=\"libs/lib_",
                            librariesIncluded) +
                        ".js\"></script>",
                    2);
            }
            return "";
        }

        public override Dictionary<string, string> GenerateReplacementDictionary(Options options, ResourceDatabase resDb)
        {

            return Util.FlattenDictionary(
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
                });
        }
    }
}
