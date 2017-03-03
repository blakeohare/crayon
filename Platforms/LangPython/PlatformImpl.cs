using System;
using System.Collections.Generic;
using System.Text;
using Common;
using Pastel.Nodes;
using Platform;

namespace LangPython
{
    public class PlatformImpl : AbstractPlatform
    {
        public override string Name { get { return "lang-python"; } }
        public override string InheritsFrom { get { return null; } }
        public override string NL { get { return "\n"; } }

        public override Dictionary<string, FileOutput> Export(
            Dictionary<string, object[]> executablesPerCompilationUnit,
            object[] structDefinitions)
        {
            throw new NotImplementedException();
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

        public override string TranslateType(PType type)
        {
            throw new InvalidOperationException("Python does not support types.");
        }

        public override Dictionary<string, FileOutput> ExportProject(
            IList<VariableDeclaration> globals,
            IList<StructDefinition> structDefinitions,
            IList<FunctionDefinition> functionDefinitions,
            ResourceDatabase resourceDatabase,
            Options options)
        {
            throw new InvalidOperationException("This platform does not support direct export.");
        }

        public override string GenerateCodeForFunction(AbstractTranslator translator, FunctionDefinition funcDef)
        {
            throw new NotImplementedException();
        }

        public override string GenerateCodeForStruct(StructDefinition structDef)
        {
            throw new InvalidOperationException("This function should not be called. Python uses lists as structs.");
        }

        public override Dictionary<string, string> GenerateReplacementDictionary(Options options)
        {
            return AbstractPlatform.GenerateGeneralReplacementsDictionary(options);
        }

        public override string GenerateCodeForGlobalsDefinitions(AbstractTranslator translator, IList<VariableDeclaration> globals)
        {
            throw new NotImplementedException();
        }
    }
}
