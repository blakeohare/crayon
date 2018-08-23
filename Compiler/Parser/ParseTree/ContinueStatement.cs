using System.Collections.Generic;

namespace Parser.ParseTree
{
    public class ContinueStatement : Executable
    {
        public ContinueStatement(Token continueToken, TopLevelConstruct owner)
            : base(continueToken, owner)
        { }

        internal override IList<Executable> Resolve(ParserContext parser)
        {
            return Listify(this);
        }

        internal override Executable ResolveEntityNames(ParserContext parser)
        {
            return this;
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }
        internal override void PerformLocalIdAllocation(ParserContext parser, VariableIdAllocator varIds, VariableIdAllocPhase phase) { }
    }
}
