using System.Collections.Generic;

namespace Crayon
{
	internal class VariableIdAllocator
	{
		private Dictionary<string, int> idsByVar = new Dictionary<string, int>();
		private List<string> varsById = new List<string>();

		public VariableIdAllocator()
		{ }

		public int Size { get { return this.idsByVar.Count; } }

		public void RegisterVariable(string value)
		{
			if (!this.idsByVar.ContainsKey(value))
			{
				idsByVar[value] = varsById.Count;
				varsById.Add(value);
			}
		}

		/*
		 * nameToken - the token of the value you're getting the ID of
		 * fallbackScope - null for global scope checks. 
		 * isDereference - the user is trying to dereference a variable. Used to determine if the failure
		 *     of this function is a bug in the user's code or if it's a bug in the parser where a variable
		 *     assignment was not registered.
		 */
		public int GetVarId(Token nameToken, VariableIdAllocator fallbackScope, bool isDereference)
		{
			int id;
			if (this.idsByVar.TryGetValue(nameToken.Value, out id))
			{
				return id;
			}

			if (fallbackScope != null)
			{
				return fallbackScope.GetVarId(nameToken, null, isDereference);
			}

			if (isDereference)
			{
				throw new ParserException(nameToken, "This variable is not defined anywhere.");
			}
			else
			{
				throw new ParserException(nameToken, 
					"BAD STATE - CRAYON BUG!!!! A variable assignment was not registered by the parse tree traversal.");
			}
		}
	}
}
