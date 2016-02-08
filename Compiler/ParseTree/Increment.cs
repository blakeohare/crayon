namespace Crayon.ParseTree
{
	internal class Increment : Expression
	{
		public override bool CanAssignTo { get { return false; } }

		public bool IsIncrement { get; private set; }
		public bool IsPrefix { get; private set; }
		public Expression Root { get; private set; }
		public Token IncrementToken { get; private set; }

		public Increment(Token firstToken, Token incrementToken, bool isIncrement, bool isPrefix, Expression root, Executable owner)
			: base(firstToken, owner)
		{
			this.IncrementToken = incrementToken;
			this.IsIncrement = isIncrement;
			this.IsPrefix = isPrefix;
			this.Root = root;
		}

		internal override Expression Resolve(Parser parser)
		{
			if (parser.IsTranslateMode)
			{
				throw new ParserException(this.IncrementToken, "++ and -- aren't supported in translate mode.");
			}

			this.Root = this.Root.Resolve(parser);

			if (!(this.Root is Variable) &&
				!(this.Root is BracketIndex) &&
				!(this.Root is FieldReference) &&
				!(this.Root is DotStep))
			{
				throw new ParserException(this.IncrementToken, "Inline increment/decrement operation is not valid for this expression.");
			}
			return this;
		}

		internal override void SetLocalIdPass(VariableIdAllocator varIds)
		{
			this.Root.SetLocalIdPass(varIds);
		}

		internal override Expression ResolveNames(Parser parser, System.Collections.Generic.Dictionary<string, Executable> lookup, string[] imports)
		{
			this.Root = this.Root.ResolveNames(parser, lookup, imports);
			return this;
		}
	}
}
