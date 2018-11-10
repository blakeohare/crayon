using System;

namespace Pastel.Nodes
{
    internal class NativeFunctionReference : Expression
    {
        public NativeFunction NativeFunctionId { get; set; }
        public Expression Context { get; set; }
        public PType ReturnType { get; set; }
        public PType[] ArgTypes { get; set; }
        public bool[] ArgTypesIsRepeated { get; set; }

        public NativeFunctionReference(Token firstToken, NativeFunction nativeFunctionId, ICompilationEntity owner) : this(firstToken, nativeFunctionId, null, owner) { }
        public NativeFunctionReference(Token firstToken, NativeFunction nativeFunctionId, Expression context, ICompilationEntity owner) : base(firstToken, owner)
        {
            this.NativeFunctionId = nativeFunctionId;
            this.Context = context;

            this.ReturnType = NativeFunctionUtil.GetNativeFunctionReturnType(this.NativeFunctionId);
            this.ArgTypes = NativeFunctionUtil.GetNativeFunctionArgTypes(this.NativeFunctionId);
            this.ArgTypesIsRepeated = NativeFunctionUtil.GetNativeFunctionIsArgTypeRepeated(this.NativeFunctionId);
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
                // NativeFunctionReferences only get introduced before the ResolveType phase for Core.* functions, in which case they have no Context and nothing to resolve.
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
