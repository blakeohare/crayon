using System;
using System.Collections.Generic;

namespace Crayon.ParseTree
{
    internal class ReturnStatement : Executable
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

        internal override Executable ResolveNames(ParserContext parser)
        {
            if (this.Expression != null)
            {
                this.Expression = this.Expression.ResolveNames(parser);
            }
            return this;
        }

        public override bool IsTerminator { get { return true; } }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableIdAllocator varIds, VariableIdAllocPhase phase)
        {
            if (this.Expression != null)
            {
                this.Expression.PerformLocalIdAllocation(parser, varIds, phase);
            }
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars)
        {
            if (this.Expression != null) this.Expression.GetAllVariablesReferenced(vars);
        }

        internal override Executable PastelResolve(ParserContext parser)
        {
            if (this.Expression != null)
            {
                this.Expression = this.Expression.PastelResolve(parser);
            }
            return this;
        }
    }
}
