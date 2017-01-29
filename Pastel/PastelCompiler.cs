using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pastel.Nodes;

namespace Pastel
{
    class PastelCompiler
    {
        private Pastel.Parser interpreterParser;
        private Dictionary<string, bool> boolConstants;

        public PastelCompiler(Dictionary<string, bool> boolConstants)
        {
            this.boolConstants = boolConstants;
            this.interpreterParser = new Parser(boolConstants);
        }

        public Executable[] Compile(string code)
        {
            throw new NotImplementedException();
        }
    }
}
