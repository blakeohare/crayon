namespace Parser.ParseTree
{
    public class ThisKeyword : Expression
    {
        public override bool CanAssignTo { get { return false; } }

        public ThisKeyword(Token token, Node owner)
            : base(token, owner)
        { }

        internal override Expression Resolve(ParserContext parser)
        {
            return this;
        }

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            return this;
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase) { }
    }
}
