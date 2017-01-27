using System.Collections.Generic;
using System.Linq;

namespace Crayon.Pastel.Nodes
{
    class OpChain : Expression
    {
        public Expression[] Expressions { get; set; }
        public Token[] Ops { get; set; }

        public OpChain(
            IList<Expression> expressions,
            IList<Token> ops) : base(expressions[0].FirstToken)
        {
            this.Expressions = expressions.ToArray();
            this.Ops = ops.ToArray();
        }
    }
}
