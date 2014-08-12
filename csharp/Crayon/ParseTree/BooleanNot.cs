namespace Crayon.ParseTree
{
	internal class BooleanNot : Expression
	{
		public Expression Root { get; private set; }

		public BooleanNot(Token bang, Expression root)
			: base(bang)
		{
			this.Root = root;
		}

		public override Expression Resolve(Parser parser)
		{
			this.Root = this.Root.Resolve(parser);

			if (this.Root is BooleanConstant)
			{
				return new BooleanConstant(this.FirstToken, !((BooleanConstant)this.Root).Value);
			}

			return this;
		}
	}
}
