﻿using Common;
using Pastel;
using Pastel.Nodes;
using Platform;
using System;
using System.Collections.Generic;
using System.Text;

namespace LangJavaScript
{
    public class PlatformImpl : AbstractPlatform
    {
        public override string Name { get { return "lang-javascript"; } }
        public override string InheritsFrom { get { return null; } }
        public override string NL { get { return "\n"; } }

        public PlatformImpl()
        {
            this.ContextFreePlatformImpl = new ContextFreeLangJavaScriptPlatform();
        }

        public override void ExportStandaloneVm(
            Dictionary<string, FileOutput> output,
            IList<VariableDeclaration> globals,
            IList<StructDefinition> structDefinitions,
            IList<FunctionDefinition> functionDefinitions,
            IList<LibraryForExport> everyLibrary,
            ILibraryNativeInvocationTranslatorProvider libraryNativeInvocationTranslatorProviderForPlatform)
        {
            throw new NotImplementedException();
        }

        public override void ExportProject(Dictionary<string, FileOutput> output, IList<VariableDeclaration> globals, IList<StructDefinition> structDefinitions, IList<FunctionDefinition> functionDefinitions, IList<LibraryForExport> libraries, ResourceDatabase resourceDatabase, Options options, ILibraryNativeInvocationTranslatorProvider libraryNativeInvocationTranslatorProviderForPlatform)
        {
            throw new NotImplementedException();
        }

        public override string GenerateCodeForFunction(AbstractTranslator translator, FunctionDefinition funcDef)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("var v_");
            sb.Append(funcDef.NameToken.Value);
            sb.Append(" = function(");
            Token[] args = funcDef.ArgNames;
            for (int i = 0; i < args.Length; ++i)
            {
                if (i > 0) sb.Append(", ");
                sb.Append("v_");
                sb.Append(args[i].Value);
            }
            sb.Append(") {");
            sb.Append(this.NL);

            translator.TabDepth = 1;
            translator.TranslateExecutables(sb, funcDef.Code);
            translator.TabDepth = 0;

            sb.Append("};");
            sb.Append(this.NL);
            sb.Append(this.NL);

            return sb.ToString();
        }

        public override string GenerateCodeForGlobalsDefinitions(AbstractTranslator translator, IList<VariableDeclaration> globals)
        {
            StringBuilder sb = new StringBuilder();
            foreach (VariableDeclaration global in globals)
            {
                translator.TranslateVariableDeclaration(sb, global);
            }
            return sb.ToString();
        }

        public override string GenerateCodeForStruct(AbstractTranslator translator, StructDefinition structDef)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<string, string> GenerateReplacementDictionary(Options options, ResourceDatabase resDb)
        {
            return AbstractPlatform.GenerateGeneralReplacementsDictionary(options);
        }

        public override IDictionary<string, object> GetConstantFlags()
        {
            return new Dictionary<string, object>()
                {
                    { "IS_ASYNC", true },
                    { "PLATFORM_SUPPORTS_LIST_CLEAR", true },
                    { "STRONGLY_TYPED", false },
                    { "IS_ARRAY_SAME_AS_LIST", true},
                    { "IS_JAVASCRIPT", true },
                    { "IS_PYTHON", false },
                    { "IS_CHAR_A_NUMBER", false },
                    { "INT_IS_FLOOR", true },
                    { "IS_THREAD_BLOCKING_ALLOWED", false },
                    { "HAS_INCREMENT", true },
                };
        }
    }
}
