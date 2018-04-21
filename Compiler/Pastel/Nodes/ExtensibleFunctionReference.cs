using System;

namespace Pastel.Nodes
{
    public class ExtensibleFunctionReference : Expression
    {
        public string Name { get; set; }

        public ExtensibleFunctionReference(Token token, string name) : base(token)
        {
            this.Name = name;
        }

        public override Expression ResolveNamesAndCullUnusedCode(PastelCompiler compiler)
        {
            throw new NotImplementedException();
        }

        internal override Expression ResolveType(VariableScope varScope, PastelCompiler compiler)
        {
            return this;
        }

        internal override Expression ResolveWithTypeContext(PastelCompiler compiler)
        {
            throw new NotImplementedException();
        }
    }
}
