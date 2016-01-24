namespace Crayon.ParseTree
{
	internal class Ternary : Expression
	{
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

		internal override void VariableUsagePass(Parser parser)
		{
			this.Condition.VariableUsagePass(parser);
			this.TrueValue.VariableUsagePass(parser);
			this.FalseValue.VariableUsagePass(parser);
		}

		internal override void VariableIdAssignmentPass(Parser parser)
		{
			this.Condition.VariableIdAssignmentPass(parser);
			this.TrueValue.VariableIdAssignmentPass(parser);
			this.FalseValue.VariableIdAssignmentPass(parser);
		}
	}
}
