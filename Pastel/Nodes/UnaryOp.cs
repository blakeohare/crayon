using System;
using System.Collections.Generic;

namespace Pastel.Nodes
{
    class UnaryOp : Expression
    {
        public Expression Expression { get; set; }

        public UnaryOp(Token op, Expression root) : base(op)
        {
            this.Expression = root;
        }

        public override Expression ResolveNamesAndCullUnusedCode(PastelCompiler compiler)
        {
            throw new NotImplementedException();
        }
    }
}
