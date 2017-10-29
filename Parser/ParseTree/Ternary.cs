using System;
using System.Collections.Generic;

namespace Crayon.ParseTree
{
    public class Ternary : Expression
    {
        internal override Expression PastelResolve(ParserContext parser)
        {
            throw new NotImplementedException();
        }

        public override bool CanAssignTo { get { return false; } }

        public Expression Condition { get; private set; }
        public Expression TrueValue { get; private set; }
        public Expression FalseValue { get; private set; }

        public Ternary(Expression condition, Expression trueValue, Expression falseValue, TopLevelConstruct owner)
            : base(condition.FirstToken, owner)
        {
            this.Condition = condition;
            this.TrueValue = trueValue;
            this.FalseValue = falseValue;
        }

        internal override Expression Resolve(ParserContext parser)
        {
            this.Condition = this.Condition.Resolve(parser);
            this.TrueValue = this.TrueValue.Resolve(parser);
            this.FalseValue = this.FalseValue.Resolve(parser);

            BooleanConstant bc = this.Condition as BooleanConstant;
            if (bc != null)
            {
                return bc.Value ? this.TrueValue : this.FalseValue;
            }

            return this;
        }

        internal override Expression ResolveNames(ParserContext parser)
        {
            this.Condition = this.Condition.ResolveNames(parser);
            this.TrueValue = this.TrueValue.ResolveNames(parser);
            this.FalseValue = this.FalseValue.ResolveNames(parser);
            return this;
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableIdAllocator varIds, VariableIdAllocPhase phase)
        {
            this.Condition.PerformLocalIdAllocation(parser, varIds, phase);
            this.TrueValue.PerformLocalIdAllocation(parser, varIds, phase);
            this.FalseValue.PerformLocalIdAllocation(parser, varIds, phase);
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars)
        {
            this.Condition.GetAllVariablesReferenced(vars);
            this.TrueValue.GetAllVariablesReferenced(vars);
            this.FalseValue.GetAllVariablesReferenced(vars);
        }
    }
}
