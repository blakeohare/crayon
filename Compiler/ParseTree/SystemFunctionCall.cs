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

			if (this.Name == "$_has_increment")
			{
				bool hasIncrement = !parser.NullablePlatform.GetType().IsAssignableFrom(typeof(Crayon.Translator.Python.PythonPlatform));
				return new BooleanConstant(this.FirstToken, hasIncrement);
			}

			if (this.Name == "$_is_javascript")
			{
				bool isJavaScript = parser.NullablePlatform.GetType().IsAssignableFrom(typeof(Crayon.Translator.JavaScript.JavaScriptPlatform));
				return new BooleanConstant(this.FirstToken, isJavaScript);
			}

			// args have already been resolved.
			return this;
		}

		public override void VariableUsagePass(Parser parser)
		{
			for (int i = 0; i < this.Args.Length; ++i)
			{
				this.Args[i].VariableUsagePass(parser);
			}
		}

		public override void VariableIdAssignmentPass(Parser parser)
		{
			for (int i = 0; i < this.Args.Length; ++i) {
				this.Args[i].VariableIdAssignmentPass(parser);
			}
		}
	}
}
