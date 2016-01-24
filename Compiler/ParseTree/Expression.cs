using System.Collections.Generic;

namespace Crayon.ParseTree
{
	public abstract class Expression : Node
	{
		public Expression(Token firstToken, Executable owner)
			: base(firstToken, owner)
		{
			this.Annotations = null;
		}

		internal abstract Expression Resolve(Parser parser);

		public virtual bool IsLiteral { get { return false; } }

		internal Dictionary<string, Annotation> Annotations { get; set; }

		// To be overridden if necessary.
		internal override void GetAllVariableNames(Dictionary<string, bool> lookup)
		{ }

		internal Annotation GetAnnotation(string type)
		{
			if (this.Annotations != null && this.Annotations.ContainsKey(type))
			{
				return this.Annotations[type];
			}
			return null;
		}

		internal virtual void AssignVariablesToIds(VariableIdAllocator varIds)
		{
			// Override me!
		}
	}
}
