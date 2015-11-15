namespace Crayon.ParseTree
{
	internal class Increment : Expression
	{
		public bool IsIncrement { get; private set; }
		public bool IsPrefix { get; private set; }
		public Expression Root { get; private set; }
		public Token IncrementToken { get; private set; }

		public Increment(Token firstToken, Token incrementToken, bool isIncrement, bool isPrefix, Expression root)
			: base(firstToken)
		{
			this.IncrementToken = incrementToken;
			this.IsIncrement = isIncrement;
			this.IsPrefix = isPrefix;
			this.Root = root;
		}

		public override Expression Resolve(Parser parser)
		{
			if (parser.IsTranslateMode)
			{
				throw new ParserException(this.IncrementToken, "++ and -- aren't supported in translate mode.");
			}

			this.Root = this.Root.Resolve(parser);

			if (!(this.Root is Variable) &&
				!(this.Root is BracketIndex) &&
				!(this.Root is DotStep))
			{
				throw new ParserException(this.IncrementToken, "Inline increment/decrement operation is not valid for this expression.");
			}
			return this;
		}

		public override void VariableUsagePass(Parser parser)
		{
			// does not declare a variable, despite technically being an assignment. Do not create an ID.
			this.Root.VariableUsagePass(parser);
		}

		public override void VariableIdAssignmentPass(Parser parser)
		{
			this.Root.VariableIdAssignmentPass(parser);
		}
	}
}
