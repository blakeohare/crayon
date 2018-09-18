using Parser.Resolver;

namespace Parser.ParseTree
{
    public class BooleanConstant : Expression, IConstantValue
    {
        public override bool IsInlineCandidate {  get { return true; } }

        public override bool CanAssignTo { get { return false; } }

        public bool Value { get; private set; }

        public override bool IsLiteral { get { return true; } }

        public BooleanConstant(Token token, bool value, Node owner)
            : base(token, owner)
        {
            this.Value = value;
        }

        internal override Expression Resolve(ParserContext parser)
        {
            return this;
        }

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            return this;
        }

        public Expression CloneValue(Token token, Node owner)
        {
            return new BooleanConstant(token, this.Value, owner);
        }

        internal override void ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            this.ResolvedType = ResolvedType.BOOLEAN;
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase) { }
    }
}
