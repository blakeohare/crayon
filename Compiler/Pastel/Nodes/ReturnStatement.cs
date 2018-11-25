namespace Pastel.Nodes
{
    internal class ReturnStatement : Executable
    {
        public Expression Expression { get; set; }

        public ReturnStatement(Token returnToken, Expression expression) : base(returnToken)
        {
            this.Expression = expression;
        }

        public override Executable ResolveNamesAndCullUnusedCode(PastelCompiler compiler)
        {
            if (this.Expression != null)
            {
                this.Expression = this.Expression.ResolveNamesAndCullUnusedCode(compiler);
            }
            return this;
        }

        internal override void ResolveTypes(VariableScope varScope, PastelCompiler compiler)
        {
            if (this.Expression != null)
            {
                this.Expression = this.Expression.ResolveType(varScope, compiler);
                if (!PType.CheckReturnType(compiler, varScope.RootFunctionDefinition.ReturnType, this.Expression.ResolvedType))
                {
                    throw new ParserException(this.Expression.FirstToken, "This expression is not the expected return type of this function.");
                }
            }
            else
            {
                if (!this.Expression.ResolvedType.IsIdentical(compiler, PType.VOID))
                {
                    throw new ParserException(this.FirstToken, "Must return a value in this function.");
                }
            }
        }

        internal override Executable ResolveWithTypeContext(PastelCompiler compiler)
        {
            if (this.Expression != null)
            {
                this.Expression = this.Expression.ResolveWithTypeContext(compiler);
            }
            return this;
        }
    }
}
