namespace Crayon.ParseTree
{
	internal class SystemFunctionCall : Expression
	{
		public string Name { get; private set; }
		public Expression[] Args { get; private set; }

		public SystemFunctionCall(Token token, Expression[] args)
			: base(token)
		{
			this.Name = token.Value;
			this.Args = args;
		}

		public override Expression Resolve(Parser parser)
		{
			if (this.Name == "$_comment" && !parser.PreserveTranslationComments)
			{
				return null;
			}
			// args have already been resolved.
			return this;
		}
	}
}
