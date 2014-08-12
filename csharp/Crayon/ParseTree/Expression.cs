namespace Crayon.ParseTree
{
	internal abstract class Expression : Node
	{
		public Expression(Token firstToken)
			: base(firstToken)
		{ }

		public abstract Expression Resolve(Parser parser);

		public virtual bool IsLiteral { get { return false; } }
	}
}
