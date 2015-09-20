namespace Crayon.ParseTree
{
	internal class NullCoalescer : Expression
	{
		public Expression PrimaryExpression { get; set; }
		public Expression SecondaryExpression { get; set; }

		public NullCoalescer(Expression primaryExpression, Expression secondaryExpression)
			: base(primaryExpression.FirstToken)
		{
			this.PrimaryExpression = primaryExpression;
			this.SecondaryExpression = secondaryExpression;
		}

		public override Expression Resolve(Parser parser)
		{
			this.PrimaryExpression = this.PrimaryExpression.Resolve(parser);
			this.SecondaryExpression = this.SecondaryExpression.Resolve(parser);

			if (this.PrimaryExpression is NullConstant)
			{
				return this.SecondaryExpression;
			}

			if (this.PrimaryExpression is IntegerConstant ||
				this.PrimaryExpression is BooleanConstant ||
				this.PrimaryExpression is StringConstant ||
				this.PrimaryExpression is ListDefinition ||
				this.PrimaryExpression is DictionaryDefinition)
			{
				return this.PrimaryExpression;
			}

			return this;
		}
	}
}
