using System;
using System.Collections.Generic;

namespace Pastel.Nodes
{
    class UnaryOp : Expression
    {
        public Expression Expression { get; set; }
        public Token OpToken { get; set; }

        public UnaryOp(Token op, Expression root) : base(op)
        {
            this.Expression = root;
            this.OpToken = op;
        }

        public override Expression ResolveNamesAndCullUnusedCode(PastelCompiler compiler)
        {
            if (this.Expression is InlineConstant)
            {
                InlineConstant ic = (InlineConstant)this.Expression;
                if (this.FirstToken.Value == "!" && ic.Value is bool)
                {
                    return new InlineConstant(PType.BOOL, this.FirstToken, !((bool)ic.Value));
                }
                if (this.FirstToken.Value == "-")
                {
                    if (ic.Value is int)
                    {
                        return new InlineConstant(PType.INT, this.FirstToken, -(int)ic.Value);
                    }
                    if (ic.Value is double)
                    {
                        return new InlineConstant(PType.DOUBLE, this.FirstToken, -(double)ic.Value);
                    }
                }
                throw new ParserException(this.OpToken, "The op '" + this.OpToken.Value + "' is not valid on this type of expression.");
            }
            return this;
        }
    }
}
