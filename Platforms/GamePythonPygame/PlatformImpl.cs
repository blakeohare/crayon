using System;
using System.Collections.Generic;
using Common;
using Pastel.Nodes;
using Platform;

namespace GamePythonPygame
{
    public class PlatformImpl : AbstractPlatform
    {
        public override string Name { get { return "game-python-pygame-cbx"; } }
        public override string InheritsFrom { get { return "lang-python"; } }

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
            Dictionary<ExportOptionKey, object> options)
        {
            throw new NotImplementedException();
        }
    }
}
