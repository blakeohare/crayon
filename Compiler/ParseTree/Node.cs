using System.Collections.Generic;

namespace Crayon.ParseTree
{
	public abstract class Node
	{
		internal Node(Token firstToken, Executable functionOrClassOwner)
		{
			this.FirstToken = firstToken;
			this.FunctionOrClassOwner = functionOrClassOwner;
		}

		public Token FirstToken { get; private set; }
		public Executable FunctionOrClassOwner { get; private set; }

		internal abstract void GetAllVariableNames(Dictionary<string, bool> lookup);

		internal abstract void VariableUsagePass(Parser parser);
		internal abstract void VariableIdAssignmentPass(Parser parser);
	}
}
