using System.Collections.Generic;

namespace Parser.ParseTree
{
    public class ReturnStatement : Executable
    {
        public Expression Expression { get; private set; }

        public ReturnStatement(Token returnToken, Expression nullableExpression, TopLevelConstruct owner)
            : base(returnToken, owner)
        {
            this.Expression = nullableExpression;
        }

        internal override IList<Executable> Resolve(ParserContext parser)
        {
            if (this.Expression != null)
            {
                this.Expression = this.Expression.Resolve(parser);
            }
            return Listify(this);
        }

        internal override Executable ResolveEntityNames(ParserContext parser)
        {
            if (this.Expression != null)
            {
                this.Expression = this.Expression.ResolveEntityNames(parser);
            }
            return this;
        }

        public override bool IsTerminator { get { return true; } }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase)
        {
            if (this.Expression != null)
            {
                this.Expression.PerformLocalIdAllocation(parser, varIds, phase);
            }
        }
    }
}
