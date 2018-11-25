namespace Pastel.Nodes
{
    internal class UnaryOp : Expression
    {
        public Expression Expression { get; set; }
        public Token OpToken { get; set; }

        public UnaryOp(Token op, Expression root) : base(op, root.Owner)
        {
            this.Expression = root;
            this.OpToken = op;
        }

        public override Expression ResolveNamesAndCullUnusedCode(PastelCompiler compiler)
        {
            this.Expression = this.Expression.ResolveNamesAndCullUnusedCode(compiler);

            if (this.Expression is InlineConstant)
            {
                InlineConstant ic = (InlineConstant)this.Expression;
                if (this.FirstToken.Value == "!" && ic.Value is bool)
                {
                    return new InlineConstant(PType.BOOL, this.FirstToken, !((bool)ic.Value), this.Owner);
                }
                if (this.FirstToken.Value == "-")
                {
                    if (ic.Value is int)
                    {
                        return new InlineConstant(PType.INT, this.FirstToken, -(int)ic.Value, this.Owner);
                    }
                    if (ic.Value is double)
                    {
                        return new InlineConstant(PType.DOUBLE, this.FirstToken, -(double)ic.Value, this.Owner);
                    }
                }
                throw new ParserException(this.OpToken, "The op '" + this.OpToken.Value + "' is not valid on this type of expression.");
            }
            return this;
        }

        internal override Expression ResolveType(VariableScope varScope, PastelCompiler compiler)
        {
            this.Expression = this.Expression.ResolveType(varScope, compiler);
            this.ResolvedType = this.Expression.ResolvedType;

            if (this.OpToken.Value == "-")
            {
                if (!(this.ResolvedType.IsIdentical(compiler, PType.INT) || this.ResolvedType.IsIdentical(compiler, PType.DOUBLE)))
                {
                    throw new ParserException(this.OpToken, "Cannot apply '-' to type: " + this.ResolvedType.ToString());
                }
            }
            else // '!'
            {
                if (!this.ResolvedType.IsIdentical(compiler, PType.BOOL))
                {
                    throw new ParserException(this.OpToken, "Cannot apply '!' to type: " + this.ResolvedType.ToString());
                }
            }
            return this;
        }

        internal override Expression ResolveWithTypeContext(PastelCompiler compiler)
        {
            this.Expression = this.Expression.ResolveWithTypeContext(compiler);
            return this;
        }
    }
}
