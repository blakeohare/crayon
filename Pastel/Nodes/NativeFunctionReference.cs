using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pastel.Nodes
{
    class NativeFunctionReference : Expression
    {
        public NativeFunction NativeFunctionId { get; set; }

        public NativeFunctionReference(Token firstToken, NativeFunction nativeFunctionId) : base(firstToken)
        {
            this.NativeFunctionId = nativeFunctionId;
        }

        public override Expression ResolveNamesAndCullUnusedCode(PastelCompiler compiler)
        {
            // Introduced in ResolveTypes phase
            throw new NotImplementedException();
        }

        internal override void ResolveType(VariableScope varScope, PastelCompiler compiler)
        {
            // Introduced in this phase
            throw new NotImplementedException();
        }
    }
}
