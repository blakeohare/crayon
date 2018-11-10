namespace Pastel.Nodes
{
    internal class FunctionReference : Expression
    {
        public FunctionDefinition Function { get; set; }
        public bool IsLibraryScopedFunction { get; set; }

        public FunctionReference(Token firstToken, FunctionDefinition functionDefinition, ICompilationEntity owner) : base(firstToken, owner)
        {
            this.Function = functionDefinition;
            this.IsLibraryScopedFunction = false;
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

        internal override Expression ResolveWithTypeContext(PastelCompiler compiler)
        {
            return this;
        }
    }
}
