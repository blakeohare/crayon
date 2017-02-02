using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pastel.Nodes
{
    class NativeFunctionReference : Expression
    {
        public NativeFunction NativeFunctionId { get; set; }
        public Expression Context { get; set; }

        public NativeFunctionReference(Token firstToken, NativeFunction nativeFunctionId) : this(firstToken, nativeFunctionId, null) { }
        public NativeFunctionReference(Token firstToken, NativeFunction nativeFunctionId, Expression context) : base(firstToken)
        {
            this.NativeFunctionId = nativeFunctionId;
            this.Context = context;
        }

        public override Expression ResolveNamesAndCullUnusedCode(PastelCompiler compiler)
        {
            // Introduced in ResolveTypes phase
            throw new NotImplementedException();
        }

        internal override Expression ResolveType(VariableScope varScope, PastelCompiler compiler)
        {
            // Introduced in this phase
            throw new NotImplementedException();
        }
    }
}
