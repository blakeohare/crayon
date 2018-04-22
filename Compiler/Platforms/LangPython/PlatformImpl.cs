using Common;
using Pastel.Nodes;
using Pastel.Transpilers;
using Platform;
using System;
using System.Collections.Generic;

namespace LangPython
{
    public class PlatformImpl : AbstractPlatform
    {
        public override string Name { get { return "lang-python"; } }
        public override string InheritsFrom { get { return null; } }
        public override string NL { get { return "\n"; } }

        public PlatformImpl() : base()
        {
            this.ContextFreePlatformImpl = new ContextFreeLangPythonPlatform();
        }

        public override IDictionary<string, object> GetConstantFlags()
        {
            return new Dictionary<string, object>()
                {
                    { "PLATFORM_SUPPORTS_LIST_CLEAR", false },
                    { "STRONGLY_TYPED", false },
                    { "IS_ARRAY_SAME_AS_LIST", true },
                    { "IS_PYTHON", true },
                    { "IS_CHAR_A_NUMBER", false },
                    { "INT_IS_FLOOR", false },
                    { "IS_THREAD_BLOCKING_ALLOWED", true },
                };
        }

        public override void ExportStandaloneVm(
            Dictionary<string, FileOutput> output,
            Pastel.PastelCompiler compiler,
            IList<LibraryForExport> everyLibrary,
            ILibraryNativeInvocationTranslatorProvider libraryNativeInvocationTranslatorProviderForPlatform)
        {
            throw new NotImplementedException();
        }

        public override void ExportProject(
            Dictionary<string, FileOutput> output,
            Pastel.PastelCompiler compiler,
            IList<LibraryForExport> libraries,
            ResourceDatabase resourceDatabase,
            Options options,
            ILibraryNativeInvocationTranslatorProvider libraryNativeInvocationTranslatorProviderForPlatform)
        {
            throw new InvalidOperationException("This platform does not support direct export.");
        }

        public override void GenerateCodeForFunction(TranspilerContext sb, AbstractTranslator translator, FunctionDefinition funcDef)
        {
            PythonTranslator pyTranslator = (PythonTranslator)translator;
            sb.CurrentFunctionDefinition = funcDef;

            sb.Append(translator.CurrentTab);
            sb.Append("def v_");
            sb.Append(funcDef.NameToken.Value);
            sb.Append('(');
            int argCount = funcDef.ArgNames.Length;
            for (int i = 0; i < argCount; ++i)
            {
                if (i > 0) sb.Append(", ");
                sb.Append("v_");
                sb.Append(funcDef.ArgNames[i].Value);
            }
            sb.Append("):");
            sb.Append(this.NL);
            translator.TabDepth++;
            translator.TranslateExecutables(sb, funcDef.Code);
            translator.TabDepth--;
            sb.Append(this.NL);

            foreach (PythonFakeSwitchStatement switchStatement in pyTranslator.SwitchStatements)
            {
                sb.Append(translator.CurrentTab);
                sb.Append(switchStatement.GenerateGlobalDictionaryLookup());
                sb.Append(this.NL);
            }
            pyTranslator.SwitchStatements.Clear();
            sb.CurrentFunctionDefinition = null;
        }

        public override void GenerateCodeForStruct(TranspilerContext sb, AbstractTranslator translator, StructDefinition structDef)
        {
            throw new InvalidOperationException("This function should not be called. Python uses lists as structs.");
        }

        public override Dictionary<string, string> GenerateReplacementDictionary(Options options, ResourceDatabase resDb)
        {
            return AbstractPlatform.GenerateGeneralReplacementsDictionary(options);
        }

        public override void GenerateCodeForGlobalsDefinitions(TranspilerContext sb, AbstractTranslator translator, IList<VariableDeclaration> globals)
        {
            foreach (VariableDeclaration global in globals)
            {
                translator.TranslateVariableDeclaration(sb, global);
            }
        }
    }
}
