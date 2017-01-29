using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pastel.Nodes;

namespace Pastel
{
    public class PastelCompiler
    {
        private PastelParser interpreterParser;
        private Dictionary<string, bool> boolConstants;

        private Dictionary<string, StructDefinition> structDefinitions = new Dictionary<string, StructDefinition>();
        private Dictionary<string, Assignment> globals = new Dictionary<string, Assignment>();
        private Dictionary<string, FunctionDefinition[]> compilationBlocks = new Dictionary<string, FunctionDefinition[]>();

        public PastelCompiler(Dictionary<string, bool> boolConstants)
        {
            this.boolConstants = boolConstants;
            this.interpreterParser = new PastelParser(boolConstants);
        }

        public Executable[] CompileBlock(string name, string code)
        {
            throw new NotImplementedException();
        }
    }
}
