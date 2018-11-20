using System;
using System.Collections.Generic;

namespace Pastel.Nodes
{
    internal class InlineConstant : Expression
    {
        public object Value { get; set; }
        public PType Type { get; set; }

        public static InlineConstant Of(object value, ICompilationEntity owner)
        {
            Token dummyToken = Token.CreateDummyToken(value.ToString());
            if (value is int)
            {
                return (InlineConstant)new InlineConstant(PType.INT, dummyToken, value, owner).ResolveType(null, null);
            }

            throw new NotImplementedException();
        }

        public InlineConstant(PType type, Token firstToken, object value, ICompilationEntity owner) : base(firstToken, owner)
        {
            this.Type = type;
            this.ResolvedType = type;
            this.Value = value;
        }

        public override Expression ResolveNamesAndCullUnusedCode(PastelCompiler compiler)
        {
            return this;
        }

        public InlineConstant CloneWithNewToken(Token token)
        {
            return new InlineConstant(this.Type, token, this.Value, this.Owner);
        }

        public InlineConstant CloneWithNewTokenAndOwner(Token token, ICompilationEntity owner)
        {
            return new InlineConstant(this.Type, token, this.Value, owner);
        }

        internal override InlineConstant DoConstantResolution(HashSet<string> cycleDetection, PastelCompiler compiler)
        {
            return this;
        }

        internal override Expression ResolveType(VariableScope varScope, PastelCompiler compiler)
        {
            this.ResolvedType = this.Type;
            return this;
        }

        internal override Expression ResolveWithTypeContext(PastelCompiler compiler)
        {
            return this;
        }
    }
}
