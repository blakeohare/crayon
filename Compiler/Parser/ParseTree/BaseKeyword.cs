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
            throw new ParserException(this.FirstToken, "'base' keyword can only be used as part of a method reference.");
        }

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            return this;
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableIdAllocator varIds, VariableIdAllocPhase phase) { }
    }
}
