using System;
using System.Collections.Generic;

namespace Pastel.Nodes
{
    class CastExpression : Expression
    {
        public PType Type { get; set; }
        public Expression Expression { get; set; }

        public CastExpression(Token openParenToken, PType type, Expression expression) : base(openParenToken)
        {
            this.Type = type;
            this.Expression = expression;
        }

        public override Expression ResolveNamesAndCullUnusedCode(PastelCompiler compiler)
        {
            throw new NotImplementedException();
        }
    }
}
