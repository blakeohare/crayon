using System.Collections.Generic;

namespace Crayon
{
    internal class VariableIdAllocator
    {
        private readonly Dictionary<string, int> idsByVar = new Dictionary<string, int>();
        private List<string> varsById = new List<string>();

        public int Size { get { return this.idsByVar.Count; } }

        public void RegisterVariable(string value)
        {
            if (!this.idsByVar.ContainsKey(value))
            {
                idsByVar[value] = varsById.Count;
                varsById.Add(value);
            }
        }

        public int GetVarId(Token variableToken, bool isRead)
        {
            int id;
            if (this.idsByVar.TryGetValue(variableToken.Value, out id))
            {
                return id;
            }

            if (isRead)
            {
                throw new ParserException(variableToken, "'" + variableToken.Value + "' is not defined anywhere.");
            }

            throw new ParserException(variableToken,
                "BAD STATE - CRAYON BUG!!!! A variable assignment was not registered by the parse tree traversal.");
        }
    }
}
