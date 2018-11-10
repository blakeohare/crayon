using System;

namespace Pastel.Nodes
{
    internal class ExtensibleFunctionReference : Expression
    {
        public string Name { get; set; }

        public ExtensibleFunctionReference(Token token, string name, ICompilationEntity owner) : base(token, owner)
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
