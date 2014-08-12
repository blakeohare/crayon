namespace Crayon.ParseTree
{
	internal class BooleanConstant : Expression
	{
		public bool Value { get; private set; }

		public override bool IsLiteral { get { return true; } }

		public BooleanConstant(Token token, bool value)
			: base(token)
		{
			this.Value = value;
		}

		public override Expression Resolve(Parser parser)
		{
			return this;
		}
	}
}
