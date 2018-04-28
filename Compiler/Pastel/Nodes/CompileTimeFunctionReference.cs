using System;

namespace Pastel.Nodes
{
    internal class CompileTimeFunctionReference : Expression
    {
        public Token NameToken { get; set; }

        public CompileTimeFunctionReference(Token atToken, Token nameToken) : base(atToken)
        {
            this.NameToken = nameToken;
        }

        public override Expression ResolveNamesAndCullUnusedCode(PastelCompiler compiler)
        {
            throw new NotImplementedException();
        }

        internal override Expression ResolveType(VariableScope varScope, PastelCompiler compiler)
        {
            throw new NotImplementedException();
        }

        internal override Expression ResolveWithTypeContext(PastelCompiler compiler)
        {
            throw new NotImplementedException();
        }
    }
}
