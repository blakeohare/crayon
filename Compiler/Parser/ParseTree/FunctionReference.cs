using Parser.Resolver;
using System;
using System.Linq;

namespace Parser.ParseTree
{
    public class FunctionReference : Expression
    {
        public override bool CanAssignTo { get { return false; } }

        public FunctionDefinition FunctionDefinition { get; set; }

        public FunctionReference(Token token, FunctionDefinition funcDef, Node owner)
            : base(token, owner)
        {
            this.FunctionDefinition = funcDef;
        }

        internal override Expression Resolve(ParserContext parser)
        {
            return this;
        }

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            throw new InvalidOperationException(); // Generated in the resolve name phase.
        }

        internal override void ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            ResolvedType returnType = this.FunctionDefinition.ResolvedReturnType;
            ResolvedType[] argTypes = this.FunctionDefinition.ResolvedArgTypes;
            int optionalCount = argTypes.Length == 0 || this.FunctionDefinition.DefaultValues[argTypes.Length - 1] == null
                ? 0
                : this.FunctionDefinition.DefaultValues.Where(e => e != null).Count();
            this.ResolvedType = ResolvedType.GetFunctionType(returnType, argTypes, optionalCount);
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase) { }
    }
}
