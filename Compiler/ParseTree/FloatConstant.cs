namespace Crayon.ParseTree
{
	internal class FloatConstant : Expression
	{
		public double Value { get; private set; }

		public override bool IsLiteral { get { return true; } }

		public FloatConstant(Token startValue, double value, Executable owner)
			: base(startValue, owner)
		{
			this.Value = value;
		}

		public static double ParseValue(Token firstToken, string fullValue)
		{
			double value;
			if (!double.TryParse(fullValue, out value))
			{
				throw new ParserException(firstToken, "Invalid float literal.");
			}
			return value;
		}

		internal override Expression Resolve(Parser parser)
		{
			return this;
		}

		internal override void VariableUsagePass(Parser parser)
		{
		}

		internal override void VariableIdAssignmentPass(Parser parser)
		{
		}
	}
}
