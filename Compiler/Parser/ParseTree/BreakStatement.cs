using System.Collections.Generic;

namespace Parser.ParseTree
{
    public class BreakStatement : Executable
    {
        public BreakStatement(Token breakToken, TopLevelConstruct owner)
            : base(breakToken, owner)
        { }

        internal override IList<Executable> Resolve(ParserContext parser)
        {
            return Listify(this);
        }

        public override bool IsTerminator { get { return true; } }

        internal override Executable ResolveNames(ParserContext parser)
        {
            return this;
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableIdAllocator varIds, VariableIdAllocPhase phase) { }

        internal override Executable PastelResolve(ParserContext parser)
        {
            return this;
        }
    }
}
