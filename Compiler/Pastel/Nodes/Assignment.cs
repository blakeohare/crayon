namespace Pastel.Nodes
{
    internal class Assignment : Executable
    {
        public Expression Target { get; set; }
        public Token OpToken { get; set; }
        public Expression Value { get; set; }

        public Assignment(
            Expression target,
            Token opToken,
            Expression value) : base(target.FirstToken)
        {
            this.Target = target;
            this.OpToken = opToken;
            this.Value = value;
        }

        public override Executable ResolveNamesAndCullUnusedCode(PastelCompiler compiler)
        {
            this.Target = this.Target.ResolveNamesAndCullUnusedCode(compiler);
            this.Value = this.Value.ResolveNamesAndCullUnusedCode(compiler);
            return this;
        }

        internal override void ResolveTypes(VariableScope varScope, PastelCompiler compiler)
        {
            this.Value = this.Value.ResolveType(varScope, compiler);
            this.Target = this.Target.ResolveType(varScope, compiler);

            if (!PType.CheckAssignment(compiler, this.Target.ResolvedType, this.Value.ResolvedType))
            {
                if (this.OpToken.Value != "=" &&
                    this.Target.ResolvedType.IsIdentical(compiler, PType.DOUBLE) &&
                    this.Value.ResolvedType.IsIdentical(compiler, PType.INT))
                {
                    // You can apply incremental ops such as += with an int to a float and that is fine without explicit conversion in any platform.
                }
                else
                {
                    throw new ParserException(this.OpToken, "Cannot assign a " + this.Value.ResolvedType + " to a " + this.Target.ResolvedType);
                }
            }
        }

        internal override Executable ResolveWithTypeContext(PastelCompiler compiler)
        {
            if (this.Target is BracketIndex)
            {
                if (this.OpToken.Value != "=")
                {
                    // Java will need to be special as it will require things to be broken down into a get-then-set.
                    throw new ParserException(this.OpToken, "Incremental assignment on a key/index is not currently supported (although it really ought to be).");
                }

                BracketIndex bi = (BracketIndex)this.Target;
                string rootType = bi.Root.ResolvedType.RootValue;
                Expression[] args = new Expression[] { bi.Root, bi.Index, this.Value };
                CoreFunction nf;
                if (rootType == "Array")
                {
                    nf = CoreFunction.ARRAY_SET;
                }
                else if (rootType == "List")
                {
                    nf = CoreFunction.LIST_SET;
                }
                else if (rootType == "Dictionary")
                {
                    nf = CoreFunction.DICTIONARY_SET;
                }
                else
                {
                    throw new ParserException(bi.BracketToken, "Can't use brackets here.");
                }
                return new ExpressionAsExecutable(new CoreFunctionInvocation(
                    this.FirstToken,
                    nf,
                    args,
                    bi.Owner)).ResolveWithTypeContext(compiler);
            }

            this.Target = this.Target.ResolveWithTypeContext(compiler);
            this.Value = this.Value.ResolveWithTypeContext(compiler);

            return this;
        }
    }
}
