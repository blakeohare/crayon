using System;
using System.Collections.Generic;
using System.Linq;

namespace Pastel.Nodes
{
    class ConstructorReference : Expression
    {
        public PType TypeToConstruct { get; set; }

        public ConstructorReference(Token newToken, PType type) : base(newToken)
        {
            this.TypeToConstruct = type;
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
