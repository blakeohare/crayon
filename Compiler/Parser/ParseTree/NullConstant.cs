using Parser.Resolver;

namespace Parser.ParseTree
{
    public class NullConstant : Expression, IConstantValue
    {
        public override bool IsInlineCandidate { get { return true; } }

        public override bool CanAssignTo { get { return false; } }

        public NullConstant(Token token, Node owner)
            : base(token, owner)
        { }

        public override bool IsLiteral { get { return true; } }

        internal override Expression Resolve(ParserContext parser)
        {
            return this;
        }

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            return this;
        }

        internal override Expression ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            this.ResolvedType = ResolvedType.NULL;
            return this;
        }

        public Expression CloneValue(Token token, Node owner)
        {
            return new NullConstant(token, owner);
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase) { }
    }
}
