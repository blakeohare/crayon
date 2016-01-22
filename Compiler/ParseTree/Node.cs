using System.Collections.Generic;

namespace Crayon.ParseTree
{
	public abstract class Node
	{
		internal Node(Token firstToken)
		{
			this.FirstToken = firstToken;
		}

		public Token FirstToken { get; private set; }

		// TODO: make these internal
		internal abstract void GetAllVariableNames(Dictionary<string, bool> lookup);

		internal abstract void VariableUsagePass(Parser parser);
		internal abstract void VariableIdAssignmentPass(Parser parser);
	}
}
