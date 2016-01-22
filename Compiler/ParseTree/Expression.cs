using System.Collections.Generic;

namespace Crayon.ParseTree
{
	internal abstract class Expression : Node
	{
		public Expression(Token firstToken)
			: base(firstToken)
		{
			this.Annotations = null;
		}

		public abstract Expression Resolve(Parser parser);

		public virtual bool IsLiteral { get { return false; } }

		public Dictionary<string, Annotation> Annotations { get; set; }

		// To be overridden if necessary.
		public override void GetAllVariableNames(Dictionary<string, bool> lookup)
		{ }

		public Annotation GetAnnotation(string type)
		{
			if (this.Annotations != null && this.Annotations.ContainsKey(type))
			{
				return this.Annotations[type];
			}
			return null;
		}

		public virtual void AssignVariablesToIds(VariableIdAllocator varIds)
		{
			// Override me!
		}
	}
}
