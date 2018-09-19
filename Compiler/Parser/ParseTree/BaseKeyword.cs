using Parser.Resolver;

namespace Parser.ParseTree
{
    public class BaseKeyword : Expression
    {
        public override bool CanAssignTo { get { return false; } }

        public BaseKeyword(Token token, Node owner)
            : base(token, owner)
        { }

        internal override Expression Resolve(ParserContext parser)
        {
            throw new ParserException(this, "'base' keyword can only be used as part of a method reference.");
        }

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            ThisKeyword.CheckIfThisOrBaseIsValid(this, parser);
            return this;
        }

        internal override Expression ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            throw new System.NotImplementedException();
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase) { }
    }
}
