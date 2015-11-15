namespace Crayon.ParseTree
{
	internal class FloatConstant : Expression
	{
		public double Value { get; private set; }

		public override bool IsLiteral { get { return true; } }

		public FloatConstant(Token startValue, double value)
			: base(startValue)
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

		public override Expression Resolve(Parser parser)
		{
			return this;
		}

		public override void VariableUsagePass(Parser parser)
		{
		}

		public override void VariableIdAssignmentPass(Parser parser)
		{
		}
	}
}
