using System.Collections.Generic;

namespace Parser.ParseTree
{
    public class ThisKeyword : Expression
    {
        public override bool CanAssignTo { get { return false; } }

        public ThisKeyword(Token token, TopLevelConstruct owner)
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

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }
        internal override void PerformLocalIdAllocation(ParserContext parser, VariableIdAllocator varIds, VariableIdAllocPhase phase) { }
    }
}
