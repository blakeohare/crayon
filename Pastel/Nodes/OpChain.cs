using System;
using System.Collections.Generic;
using System.Linq;

namespace Pastel.Nodes
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

        public override Expression ResolveNamesAndCullUnusedCode(PastelCompiler compiler)
        {
            throw new NotImplementedException();
        }

        internal override InlineConstant DoConstantResolution(HashSet<string> cycleDetection, PastelCompiler compiler)
        {
            for (int i = 0; i < this.Expressions.Length; ++i)
            {
                this.Expressions[i] = this.Expressions[i].DoConstantResolution(cycleDetection, compiler);
            }

            InlineConstant current = (InlineConstant)this.Expressions[0];
            for (int i = 1; i < this.Expressions.Length; ++i)
            {
                InlineConstant next = (InlineConstant)this.Expressions[i];
                string lookup = current.Type.RootValue + this.Ops[i - 1].Value + next.Type.RootValue;
                switch (lookup)
                {
                    case "int+int":
                        current = new InlineConstant(PType.INT, current.FirstToken, (int)current.Value + (int)next.Value);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            return current;
        }
    }
}
