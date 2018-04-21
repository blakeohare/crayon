using Common;
using Pastel.Nodes;
using Platform;
using System;
using System.Collections.Generic;
using System.Text;

namespace CApp
{
    public class PlatformImpl : AbstractPlatform
    {
        public override string Name { get { return "c-app"; } }
        public override string InheritsFrom { get { return "lang-c"; } }
        public override string NL { get { return "\n"; } }

        public PlatformImpl() : base(new ContextFreeCAppPlatform())
        {
            this.Translator = new CAppTranslator(this);
        }

        public LangC.CTranslator CTranslator { get { return (LangC.CTranslator)this.Translator; } }

        public override void ExportProject(
            Dictionary<string, FileOutput> output,
            IList<VariableDeclaration> globals,
            IList<StructDefinition> structDefinitions,
            IList<FunctionDefinition> functionDefinitions,
            IList<LibraryForExport> libraries,
            ResourceDatabase resourceDatabase,
            Options options,
            ILibraryNativeInvocationTranslatorProvider libraryNativeInvocationTranslatorProviderForPlatform)
        {
            Dictionary<string, string> replacements = this.GenerateReplacementDictionary(options, resourceDatabase);
            StringBuilder cCode = new StringBuilder();

            cCode.Append("#include <stdio.h>\n");
            cCode.Append("#include <stdlib.h>\n");
            cCode.Append("#include <string.h>\n");
            cCode.Append(this.NL);

            cCode.Append(this.LoadTextResource("Resources/List.txt", replacements));
            cCode.Append(this.LoadTextResource("Resources/String.txt", replacements));
            cCode.Append(this.LoadTextResource("Resources/Dictionary.txt", replacements));
            cCode.Append(this.LoadTextResource("Resources/TranslationHelper.txt", replacements));

            // This needs to be done in LangC
            foreach (StructDefinition structDef in structDefinitions)
            {
                string name = structDef.NameToken.Value;
                cCode.Append("typedef struct " + name + " " + name + ";\n");
            }
            cCode.Append(this.NL);

            foreach (StructDefinition structDef in structDefinitions)
            {
                cCode.Append(this.GenerateCodeForStruct(structDef));
            }

            foreach (FunctionDefinition fd in functionDefinitions)
            {
                string functionDeclaration = this.GenerateCodeForFunctionDeclaration(this.Translator, fd);
                cCode.Append(functionDeclaration);
                cCode.Append(this.NL);
            }

            this.CTranslator.StringTableBuilder = new LangC.StringTableBuilder("VM");

            StringBuilder functionCodeBuilder = new StringBuilder();
            foreach (FunctionDefinition fd in functionDefinitions)
            {
                string functionCode = this.GenerateCodeForFunction(this.Translator, fd);
                functionCodeBuilder.Append(functionCode);
                functionCodeBuilder.Append(this.NL);
                functionCodeBuilder.Append(this.NL);
            }

            LangC.PlatformImpl.BuildStringTable(cCode, this.CTranslator.StringTableBuilder, this.NL);

            cCode.Append(functionCodeBuilder);

            cCode.Append(this.LoadTextResource("Resources/main.txt", replacements));

            output["main.c"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = cCode.ToString(),
            };
        }

        public override void ExportStandaloneVm(Dictionary<string, FileOutput> output, IList<VariableDeclaration> globals, IList<StructDefinition> structDefinitions, IList<FunctionDefinition> functionDefinitions, IList<LibraryForExport> everyLibrary, ILibraryNativeInvocationTranslatorProvider libraryNativeInvocationTranslatorProviderForPlatform)
        {
            throw new NotImplementedException();
        }

        public override string GenerateCodeForFunction(AbstractTranslator translator, FunctionDefinition funcDef)
        {
            return this.ParentPlatform.GenerateCodeForFunction(translator, funcDef);
        }

        public override string GenerateCodeForGlobalsDefinitions(AbstractTranslator translator, IList<VariableDeclaration> globals)
        {
            return this.ParentPlatform.GenerateCodeForGlobalsDefinitions(translator, globals);
        }

        public override string GenerateCodeForStruct(StructDefinition structDef)
        {
            return this.ParentPlatform.GenerateCodeForStruct(structDef);
        }

        public override Dictionary<string, string> GenerateReplacementDictionary(Options options, ResourceDatabase resDb)
        {
            return new Dictionary<string, string>();
        }

        public override IDictionary<string, object> GetConstantFlags()
        {
            return new Dictionary<string, object>();
        }
    }
}
