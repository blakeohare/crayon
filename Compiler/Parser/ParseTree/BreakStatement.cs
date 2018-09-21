using Parser.Resolver;
using System.Collections.Generic;

namespace Parser.ParseTree
{
    public class BreakStatement : Executable
    {
        public BreakStatement(Token breakToken, Node owner)
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

        internal override void ResolveVariableOrigins(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase) { }

        internal override void ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        { }
    }
}
