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

        public override Expression ResolveNamesAndCullUnusedCode(PastelCompiler compiler)
        {
            return this;
        }

        internal override Expression ResolveType(VariableScope varScope, PastelCompiler compiler)
        {
            // no function pointer type
            return this;
        }
    }
}
