using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pastel.Nodes
{
    class FunctionReference : Expression
    {
        public FunctionDefinition Function { get; set; }

        public FunctionReference(Token firstToken, FunctionDefinition functionDefinition) : base(firstToken)
        {
            this.Function = functionDefinition;
        }

        public override Expression ResolveNamesAndCullUnusedCode(PastelCompiler compiler)
        {
            return this;
        }

        internal override Expression ResolveType(VariableScope varScope, PastelCompiler compiler)
        {
            // no function pointeer type.
            return this;
        }
    }
}
