using System.Collections.Generic;
using System.Linq;

namespace Crayon.Pastel.Nodes
{
    class StructDefinition
    {
        public Token FirstToken { get; set; }
        public Token NameToken { get; set; }
        public PType[] ArgTypes { get; set; }
        public Token[] ArgNames { get; set; }
        public Dictionary<string, int> ArgIndexByName { get; set; }

        public StructDefinition(Token structToken, Token name, IList<PType> argTypes, IList<Token> argNames)
        {
            this.FirstToken = structToken;
            this.NameToken = name;
            this.ArgTypes = argTypes.ToArray();
            this.ArgNames = argNames.ToArray();
            this.ArgIndexByName = new Dictionary<string, int>();
            for (int i = this.ArgNames.Length - 1; i >= 0; --i)
            {
                string argName = this.ArgNames[i].Value;
                this.ArgIndexByName[argName] = i;
            }
        }
    }
}
