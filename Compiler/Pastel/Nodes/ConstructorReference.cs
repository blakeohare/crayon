using System;

namespace Pastel.Nodes
{
    internal class ConstructorReference : Expression
    {
        public PType TypeToConstruct { get; set; }

        public ConstructorReference(Token newToken, PType type, ICompilationEntity owner) : base(newToken, owner)
        {
            this.TypeToConstruct = type;
        }

        public override Expression ResolveNamesAndCullUnusedCode(PastelCompiler compiler)
        {
            return this;
        }

        internal override Expression ResolveType(VariableScope varScope, PastelCompiler compiler)
        {
            // no function pointer type
            return this;
        }

        internal override Expression ResolveWithTypeContext(PastelCompiler compiler)
        {
            throw new NotImplementedException();
        }
    }
}
