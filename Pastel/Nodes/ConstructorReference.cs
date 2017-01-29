using System;
using System.Collections.Generic;
using System.Linq;

namespace Pastel.Nodes
{
    class ConstructorReference : Expression
    {
        public Token[] DotChain { get; set; }

        public ConstructorReference(Token newToken, IList<Token> dotChain) : base(newToken)
        {
            this.DotChain = dotChain.ToArray();
        }

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
