namespace Pastel.Nodes
{
    internal class BreakStatement : Executable
    {
        public BreakStatement(Token breakToken) : base(breakToken)
        { }

        public override Executable ResolveNamesAndCullUnusedCode(PastelCompiler compiler)
        {
            return this;
        }

        internal override void ResolveTypes(VariableScope varScope, PastelCompiler compiler)
        {
            // nothing to do
        }

        internal override Executable ResolveWithTypeContext(PastelCompiler compiler)
        {
            return this;
        }
    }
}
