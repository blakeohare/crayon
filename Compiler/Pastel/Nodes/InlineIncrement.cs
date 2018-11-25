namespace Pastel.Nodes
{
    internal class InlineIncrement : Expression
    {
        public Token IncrementToken { get; set; }
        public Expression Expression { get; set; }
        public bool IsPrefix { get; set; }

        public InlineIncrement(Token firstToken, Token incrementToken, Expression root, bool isPrefix) : base(firstToken, root.Owner)
        {
            this.IncrementToken = incrementToken;
            this.Expression = root;
            this.IsPrefix = isPrefix;
        }

        public override Expression ResolveNamesAndCullUnusedCode(PastelCompiler compiler)
        {
            this.Expression = this.Expression.ResolveNamesAndCullUnusedCode(compiler);
            return this;
        }

        internal override Expression ResolveType(VariableScope varScope, PastelCompiler compiler)
        {
            this.Expression = this.Expression.ResolveType(varScope, compiler);
            if (!this.Expression.ResolvedType.IsIdentical(compiler, PType.INT))
            {
                throw new ParserException(this.IncrementToken, "++ and -- can only be applied to integer types.");
            }
            this.ResolvedType = PType.INT;
            return this;
        }

        internal override Expression ResolveWithTypeContext(PastelCompiler compiler)
        {
            // TODO: check if this is either:
            // - exporting to a platform that supports this OR
            // - is running as the direct descendant of ExpressionAsExecutable, and then swap out with += 1
            return this;
        }
    }
}
