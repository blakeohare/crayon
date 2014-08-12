using System.Collections.Generic;

namespace Crayon.ParseTree
{
	internal abstract class Node
	{
		public Node(Token firstToken)
		{
			this.FirstToken = firstToken;
		}

		public Token FirstToken { get; private set; }

		public abstract void GetAllVariableNames(Dictionary<string, bool> lookup);
	}
}
