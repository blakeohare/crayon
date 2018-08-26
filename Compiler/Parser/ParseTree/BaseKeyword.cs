using System;
using System.Collections.Generic;

namespace Parser.ParseTree
{
    public class BaseKeyword : Expression
    {
        public override bool CanAssignTo { get { return false; } }

        public BaseKeyword(Token token, TopLevelConstruct owner)
            : base(token, owner)
        {
        }

        internal override Expression Resolve(ParserContext parser)
        {
            throw new ParserException(this, "'base' keyword can only be used as part of a method reference.");
        }

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            return this;
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase) { }
    }
}
