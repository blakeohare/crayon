using System;
using System.Collections.Generic;

namespace Crayon.ParseTree
{
    internal class Ternary : Expression
    {
        internal override Expression PastelResolve(Parser parser)
        {
            throw new NotImplementedException();
        }

        public override bool CanAssignTo { get { return false; } }

        public Expression Condition { get; private set; }
        public Expression TrueValue { get; private set; }
        public Expression FalseValue { get; private set; }

        public Ternary(Expression condition, Expression trueValue, Expression falseValue, Executable owner)
            : base(condition.FirstToken, owner)
        {
            this.Condition = condition;
            this.TrueValue = trueValue;
            this.FalseValue = falseValue;
        }

        internal override Expression Resolve(Parser parser)
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

        internal override Expression ResolveNames(Parser parser, System.Collections.Generic.Dictionary<string, Executable> lookup, string[] imports)
        {
            this.Condition = this.Condition.ResolveNames(parser, lookup, imports);
            this.TrueValue = this.TrueValue.ResolveNames(parser, lookup, imports);
            this.FalseValue = this.FalseValue.ResolveNames(parser, lookup, imports);
            return this;
        }

        internal override void PerformLocalIdAllocation(Parser parser, VariableIdAllocator varIds, VariableIdAllocPhase phase)
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
