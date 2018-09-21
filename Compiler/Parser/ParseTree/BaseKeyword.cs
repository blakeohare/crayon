using Parser.Resolver;
using System.Collections.Generic;

namespace Parser.ParseTree
{
    public class BaseKeyword : Expression
    {
        internal override IEnumerable<Expression> Descendants { get { return Expression.NO_DESCENDANTS; } }

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
