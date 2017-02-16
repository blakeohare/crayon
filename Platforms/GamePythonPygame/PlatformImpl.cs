using System;
using System.Collections.Generic;
using Common;
using Pastel.Nodes;
using Platform;
using System.Text;

namespace GamePythonPygame
{
    public class PlatformImpl : AbstractPlatform
    {
        public override string Name { get { return "game-python-pygame-cbx"; } }
        public override string InheritsFrom { get { return "lang-python"; } }
        public override string NL { get { return "\n"; } }

        public override Dictionary<string, FileOutput> Export(
            Dictionary<string, object[]> executablesPerCompilationUnit,
            object[] structDefinitions)
        {
            throw new NotImplementedException();
        }

        public override IDictionary<string, object> GetConstantFlags()
        {
            return new Dictionary<string, object>();
        }

        public override Dictionary<string, FileOutput> ExportProject(
            IList<VariableDeclaration> globals,
            IList<StructDefinition> structDefinitions,
            IList<FunctionDefinition> functionDefinitions,
            Options options)
        {
            throw new NotImplementedException();
        }

        public override string GenerateCodeForFunction(FunctionDefinition funcDef)
        {
            return this.ParentPlatform.GenerateCodeForFunction(funcDef);
        }

        public override string GenerateCodeForStruct(StructDefinition structDef)
        {
            return this.ParentPlatform.GenerateCodeForStruct(structDef);
        }

        public override void TranslateExecutables(StringBuilder output, Executable[] executables)
        {
            this.ParentPlatform.TranslateExecutables(output, executables);
        }

        public override Dictionary<string, string> GenerateReplacementDictionary(Options options)
        {
            return this.ParentPlatform.GenerateReplacementDictionary(options);
        }
    }
}
