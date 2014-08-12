namespace Crayon.ParseTree
{
	internal class BracketIndex : Expression
	{
		public Expression Root { get; set; }
		public Token BracketToken { get; private set; }
		public Expression Index { get; set; }

		public BracketIndex(Expression root, Token bracketToken, Expression index)
			: base(root.FirstToken)
		{
			this.Root = root;
			this.BracketToken = bracketToken;
			this.Index = index;
		}

		public override Expression Resolve(Parser parser)
		{
			this.Root = this.Root.Resolve(parser);
			this.Index = this.Index.Resolve(parser);
			return this;
		}
	}
}
