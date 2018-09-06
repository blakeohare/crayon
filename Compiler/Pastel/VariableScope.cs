using Pastel.Nodes;
using System.Collections.Generic;

namespace Pastel
{
    internal class VariableScope
    {
        public FunctionDefinition RootFunctionDefinition;
        private VariableScope parent = null;
        private Dictionary<string, PType> type = new Dictionary<string, PType>();

        public VariableScope() { }

        public VariableScope(FunctionDefinition functionDef)
        {
            this.RootFunctionDefinition = functionDef;
        }

        public VariableScope(VariableScope parent)
        {
            this.parent = parent;
            this.RootFunctionDefinition = parent.RootFunctionDefinition;
        }

        public void DeclareVariables(Token nameToken, PType type)
        {
            string name = nameToken.Value;
            if (this.GetTypeOfVariable(name) != null)
            {
                throw new ParserException(nameToken, "This declaration of '" + name + "' conflicts with a previous declaration.");
            }

            this.type[name] = type;
        }

        public PType GetTypeOfVariable(string name)
        {
            PType output;
            if (this.type.TryGetValue(name, out output))
            {
                return output;
            }

            if (this.parent != null)
            {
                return this.parent.GetTypeOfVariable(name);
            }

            return null;
        }
    }
}
