using System.Collections.Generic;
using System.Linq;

namespace Parser
{
    public class ModifierCollection
    {
        public Dictionary<string, Token> modifierTokens;
        public Token FirstToken { get; private set; }

        public static readonly ModifierCollection EMPTY = new ModifierCollection(new Token[0]);

        public ModifierCollection(IList<Token> modifiers)
        {
            this.FirstToken = modifiers.Count == 0 ? null : modifiers[0];

            // TODO: throw error if duplicates
            this.modifierTokens = modifiers.ToDictionary(token => token.Value);
        }

        public static ModifierCollection CreateStaticModifier(Token aToken)
        {
            ModifierCollection modifiers = new ModifierCollection(new Token[0]);
            modifiers.FirstToken = aToken;
            modifiers.modifierTokens["static"] = aToken;
            return modifiers;
        }

        public bool HasStatic { get { return this.modifierTokens.ContainsKey("static"); } }
    }
}
