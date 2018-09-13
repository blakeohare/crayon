using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    public class ModifierCollection
    {
        public Dictionary<string, Token> modifierTokens;

        public static readonly ModifierCollection EMPTY = new ModifierCollection(new Token[0]);

        public ModifierCollection(ICollection<Token> modifiers)
        {
            // TODO: throw error if duplicates
            this.modifierTokens = modifiers.ToDictionary(token => token.Value);
        }

        public bool HasStatic {  get { return this.modifierTokens.ContainsKey("static"); } }
    }
}
