using System;
using System.Collections.Generic;

namespace Pastel.Nodes
{
    class BreakStatement : Executable
    {
        public BreakStatement(Token breakToken) : base(breakToken)
        { }

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
