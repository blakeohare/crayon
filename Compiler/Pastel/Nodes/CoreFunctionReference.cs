using System;

namespace Pastel.Nodes
{
    internal class CoreFunctionReference : Expression
    {
        public CoreFunction CoreFunctionId { get; set; }
        public Expression Context { get; set; }
        public PType ReturnType { get; set; }
        public PType[] ArgTypes { get; set; }
        public bool[] ArgTypesIsRepeated { get; set; }

        public CoreFunctionReference(Token firstToken, CoreFunction coreFunctionId, ICompilationEntity owner) : this(firstToken, coreFunctionId, null, owner) { }
        public CoreFunctionReference(Token firstToken, CoreFunction coreFunctionId, Expression context, ICompilationEntity owner) : base(firstToken, owner)
        {
            this.CoreFunctionId = coreFunctionId;
            this.Context = context;

            this.ReturnType = CoreFunctionUtil.GetCoreFunctionReturnType(this.CoreFunctionId);
            this.ArgTypes = CoreFunctionUtil.GetCoreFunctionArgTypes(this.CoreFunctionId);
            this.ArgTypesIsRepeated = CoreFunctionUtil.GetCoreFunctionIsArgTypeRepeated(this.CoreFunctionId);
        }

        public override Expression ResolveNamesAndCullUnusedCode(PastelCompiler compiler)
        {
            // Introduced in ResolveTypes phase
            throw new NotImplementedException();
        }

        internal override Expression ResolveType(VariableScope varScope, PastelCompiler compiler)
        {
            if (this.Context != null)
            {
                // CoreFunctionReferences only get introduced before the ResolveType phase for Core.* functions, in which case they have no Context and nothing to resolve.
                throw new Exception();
            }
            return this;
        }

        internal override Expression ResolveWithTypeContext(PastelCompiler compiler)
        {
            throw new NotImplementedException();
        }
    }
}
