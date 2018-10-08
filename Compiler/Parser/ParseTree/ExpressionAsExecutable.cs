using Parser.Resolver;
using System.Collections.Generic;

namespace Parser.ParseTree
{
    public class ExpressionAsExecutable : Executable
    {
        public Expression Expression { get; private set; }

        public ExpressionAsExecutable(Expression expression, Node owner)
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
                    null,
                    inc.IncrementToken,
                    new IntegerConstant(inc.IncrementToken, 1, this.Owner),
                    false,
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

        internal override void ResolveVariableOrigins(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase)
        {
            if ((phase & VariableIdAllocPhase.ALLOC) != 0)
            {
                this.Expression.ResolveVariableOrigins(parser, varIds, phase);
            }
        }

        internal override void ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            this.Expression.ResolveTypes(parser, typeResolver);
        }
    }
}
