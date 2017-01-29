using System;
using System.Collections.Generic;

namespace Pastel.Nodes
{
    class Variable : Expression
    {
        public Variable(Token token) : base(token)
        { }

        public override Expression NameResolution(Dictionary<string, FunctionDefinition> functionLookup, Dictionary<string, StructDefinition> structLookup)
        {
            throw new NotImplementedException();
        }

        public override void ResolveTypes()
        {
            throw new NotImplementedException();
        }
    }
}

