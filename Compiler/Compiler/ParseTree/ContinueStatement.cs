using Builder.Resolver;
using System.Collections.Generic;

namespace Builder.ParseTree
{
    internal class ContinueStatement : Executable
    {
        public ContinueStatement(Token continueToken, Node owner)
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

        internal override void ResolveVariableOrigins(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase) { }

        internal override void ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        { }
    }
}
