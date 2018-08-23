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

        internal override Executable ResolveEntityNames(ParserContext parser)
        {
            return this;
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableIdAllocator varIds, VariableIdAllocPhase phase) { }
    }
}
