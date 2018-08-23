using System.Collections.Generic;

namespace Parser.ParseTree
{
    public class ThrowStatement : Executable
    {
        public override bool IsTerminator { get { return true; } }

        public Expression Expression { get; set; }
        public Token ThrowToken { get; set; }

        public ThrowStatement(Token throwToken, Expression expression, TopLevelConstruct owner) : base(throwToken, owner)
        {
            this.ThrowToken = throwToken;
            this.Expression = expression;
        }

        internal override IList<Executable> Resolve(ParserContext parser)
        {
            this.Expression = this.Expression.Resolve(parser);
            return Listify(this);
        }

        internal override Executable ResolveEntityNames(ParserContext parser)
        {
            this.Expression.ResolveEntityNames(parser);
            return this;
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableIdAllocator varIds, VariableIdAllocPhase phase)
        {
            this.Expression.PerformLocalIdAllocation(parser, varIds, phase);
        }
    }
}
