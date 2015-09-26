namespace Crayon.ParseTree
{
	internal class BaseKeyword : Expression
	{
		public BaseKeyword(Token token)
			: base(token)
		{
		}

		public override Expression Resolve(Parser parser)
		{
			if (parser.IsInClass && parser.CurrentClass.SubClasses.Length > 0)
			{
				return this;
			}

			throw new ParserException(this.FirstToken, "Reference to base keyword in a class that does not have any base classes.");
		}
	}
}
