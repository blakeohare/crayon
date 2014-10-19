using System.Collections.Generic;

namespace Crayon.ParseTree
{
	internal abstract class Expression : Node
	{
		public Expression(Token firstToken)
			: base(firstToken)
		{
			this.Annotation = null;
		}

		public abstract Expression Resolve(Parser parser);

		public virtual bool IsLiteral { get { return false; } }

		public Annotation Annotation { get; set; }

		// To be overridden if necessary.
		public override void GetAllVariableNames(Dictionary<string, bool> lookup)
		{ }
	}
}
