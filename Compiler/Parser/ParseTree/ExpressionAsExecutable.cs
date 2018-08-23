using System.Collections.Generic;

namespace Parser.ParseTree
{
    public class ExpressionAsExecutable : Executable
    {
        public Expression Expression { get; private set; }

        public ExpressionAsExecutable(Expression expression, TopLevelConstruct owner)
            : base(expression.FirstToken, owner)
        {
            this.Expression = expression;
        }

        internal override IList<Executable> Resolve(ParserContext parser)
        {
            this.Expression = this.Expression.Resolve(parser);

            if (this.Expression == null)
            {
                return new Executable[0];
            }

            if (this.Expression is Increment)
            {
                Increment inc = (Increment)this.Expression;
                Assignment output = new Assignment(
                    inc.Root,
                    inc.IncrementToken,
                    inc.IsIncrement ? "+=" : "-=",
                    new IntegerConstant(inc.IncrementToken, 1, this.Owner),
                    this.Owner);
                return output.Resolve(parser);
            }

            return Listify(this);
        }

        internal override Executable ResolveEntityNames(ParserContext parser)
        {
            this.Expression = this.Expression.ResolveEntityNames(parser);
            return this;
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars)
        {
            this.Expression.GetAllVariablesReferenced(vars);
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableIdAllocator varIds, VariableIdAllocPhase phase)
        {
            if ((phase & VariableIdAllocPhase.ALLOC) != 0)
            {
                this.Expression.PerformLocalIdAllocation(parser, varIds, phase);
            }
        }
    }
}
