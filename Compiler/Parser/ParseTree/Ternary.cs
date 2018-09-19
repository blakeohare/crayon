using Parser.Resolver;

namespace Parser.ParseTree
{
    public class Ternary : Expression
    {
        public override bool CanAssignTo { get { return false; } }

        public Expression Condition { get; private set; }
        public Expression TrueValue { get; private set; }
        public Expression FalseValue { get; private set; }

        public Ternary(Expression condition, Expression trueValue, Expression falseValue, Node owner)
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

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            this.Condition = this.Condition.ResolveEntityNames(parser);
            this.TrueValue = this.TrueValue.ResolveEntityNames(parser);
            this.FalseValue = this.FalseValue.ResolveEntityNames(parser);
            return this;
        }

        internal override Expression ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            throw new System.NotImplementedException();
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase)
        {
            this.Condition.PerformLocalIdAllocation(parser, varIds, phase);
            this.TrueValue.PerformLocalIdAllocation(parser, varIds, phase);
            this.FalseValue.PerformLocalIdAllocation(parser, varIds, phase);
        }
    }
}
