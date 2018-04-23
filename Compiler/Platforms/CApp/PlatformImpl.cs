﻿using Common;
using Pastel.Nodes;
using Pastel.Transpilers;
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

        public PlatformImpl()
        {
            this.Translator = new CTranslator();
        }

        public CTranslator CTranslator { get { return (CTranslator)this.Translator; } }

        public override void ExportProject(
            Dictionary<string, FileOutput> output,
            Pastel.PastelCompiler compiler,
            Pastel.PastelContext pastelContext,
            IList<LibraryForExport> libraries,
            ResourceDatabase resourceDatabase,
            Options options,
            ILibraryNativeInvocationTranslatorProvider libraryNativeInvocationTranslatorProviderForPlatform)
        {
            TranspilerContext ctx = new TranspilerContext();
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
            foreach (StructDefinition structDef in compiler.GetStructDefinitions())
            {
                string name = structDef.NameToken.Value;
                cCode.Append("typedef struct " + name + " " + name + ";\n");
            }
            cCode.Append(this.NL);

            foreach (StructDefinition structDef in compiler.GetStructDefinitions())
            {

                if (structDef.NameToken.Value == "Value")
                {
                    // I need to do fancy stuff with unions, so special case this one.
                    string valueStruct = this.LoadTextResource("Resources/ValueStruct.txt", new Dictionary<string, string>());
                    cCode.Append(valueStruct);
                }
                else
                {
                    this.Translator.GenerateCodeForStruct(ctx, this.Translator, structDef);
                    cCode.Append(ctx.FlushAndClearBuffer());
                }
            }

            foreach (FunctionDefinition fd in compiler.GetFunctionDefinitions())
            {
                this.Translator.GenerateCodeForFunctionDeclaration(ctx, this.Translator, fd);
                string functionDeclaration = ctx.FlushAndClearBuffer();
                cCode.Append(functionDeclaration);
                cCode.Append(this.NL);
            }

            this.CTranslator.StringTableBuilder = new StringTableBuilder("VM");

            StringBuilder functionCodeBuilder = new StringBuilder();
            foreach (FunctionDefinition fd in compiler.GetFunctionDefinitions())
            {
                this.Translator.GenerateCodeForFunction(ctx, this.Translator, fd);
                string functionCode = ctx.FlushAndClearBuffer();
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

        public override void ExportStandaloneVm(
            Dictionary<string, FileOutput> output,
            Pastel.PastelCompiler compiler,
            Pastel.PastelContext pastelContext,
            IList<LibraryForExport> everyLibrary,
            ILibraryNativeInvocationTranslatorProvider libraryNativeInvocationTranslatorProviderForPlatform)
        {
            throw new NotImplementedException();
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
