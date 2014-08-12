namespace Crayon.ParseTree
{
	internal class NegativeSign : Expression
	{
		public Expression Root { get; private set; }

		public NegativeSign(Token sign, Expression root)
			: base(sign)
		{
			this.Root = root;
		}

		public override Expression Resolve(Parser parser)
		{
			if (parser.Mode == PlatformTarget.ByteCode)
			{
			}
			this.Root = this.Root.Resolve(parser);
			if (this.Root is IntegerConstant)
			{
				return new IntegerConstant(this.FirstToken, ((IntegerConstant)this.Root).Value * -1);
			}

			if (this.Root is FloatConstant)
			{
				return new FloatConstant(this.FirstToken, ((FloatConstant)this.Root).Value * -1);
			}

			return this;
		}
	}
}
