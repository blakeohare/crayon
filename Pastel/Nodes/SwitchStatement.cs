using System;
using System.Collections.Generic;

namespace Pastel.Nodes
{
    class SwitchStatement : Executable
    {
        public SwitchStatement(Token switchToken) : base(switchToken)
        {
            throw new Exception();
        }

        public override IList<Executable> NameResolution(Dictionary<string, FunctionDefinition> functionLookup, Dictionary<string, StructDefinition> structLookup)
        {
            throw new NotImplementedException();
        }

        public override void ResolveTypes()
        {
            throw new NotImplementedException();
        }
    }
}
