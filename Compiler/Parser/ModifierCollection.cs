using System.Collections.Generic;
using System.Linq;

namespace Parser
{
    public enum AccessModifierType
    {
        PUBLIC,
        PRIVATE,
        INTERNAL,
        PROTECTED,
        INTERNAL_PROTECTED,
    }

    public class ModifierCollection
    {
        public static readonly ModifierCollection EMPTY = new ModifierCollection(new Token[0]);

        private enum ModifierType
        {
            ABSTRACT = 0,
            FINAL = 1,
            INTERNAL = 2,
            OVERRIDE = 3,
            PRIVATE = 4,
            PROTECTED = 5,
            PUBLIC = 6,
            STATIC = 7,

            MAX = 8,
        }

        private static readonly Dictionary<string, ModifierType> LOOKUP = new Dictionary<string, ModifierType>()
        {
            { "abstract", ModifierType.ABSTRACT },
            { "final", ModifierType.FINAL },
            { "internal", ModifierType.INTERNAL},
            { "private", ModifierType.PRIVATE },
            { "protected", ModifierType.PROTECTED },
            { "public", ModifierType.PUBLIC },
            { "override", ModifierType.OVERRIDE },
            { "static", ModifierType.STATIC },
        };

        private Token[] tokens = new Token[(int)ModifierType.MAX];
        public Token FirstToken { get; private set; }
        public AccessModifierType AccessModifierType { get; private set; }

        public static ModifierCollection Parse(TokenStream tokens)
        {
            if (!tokens.HasMore || !LOOKUP.ContainsKey(tokens.PeekValue()))
            {
                return EMPTY;
            }

            List<Token> modifierTokens = new List<Token>();
            while (LOOKUP.ContainsKey(tokens.PeekValue()))
            {
                modifierTokens.Add(tokens.Pop());
            }
            return new ModifierCollection(modifierTokens);
        }

        private ModifierCollection(IList<Token> modifiers)
        {
            this.FirstToken = modifiers.Count == 0 ? null : modifiers[0];

            foreach (Token token in modifiers)
            {
                int index = (int)LOOKUP[token.Value];
                if (this.tokens[index] != null) throw new ParserException(token, "Multiple declarations of '" + token.Value + "' have occurred.");
                this.tokens[index] = token;
            }

            if (this.HasPrivate) this.AccessModifierType = AccessModifierType.PRIVATE;
            else if (this.HasInternal) this.AccessModifierType = AccessModifierType.INTERNAL;
            else if (this.HasProtected) this.AccessModifierType = AccessModifierType.PROTECTED;
            else this.AccessModifierType = AccessModifierType.PUBLIC;

            if (modifiers.Count > 1)
            {
                int accessModifierCount =
                    (this.HasPublicExplicitly ? 1 : 0) +
                    (this.HasPrivate ? 1 : 0) +
                    (this.HasInternal ? 1 : 0) +
                    (this.HasProtected ? 1 : 0);

                if (accessModifierCount > 1)
                {
                    if (accessModifierCount != 2 || !this.HasProtected || !this.HasInternal)
                    {
                        throw new ParserException(
                            this.PublicToken ?? this.PrivateToken ?? this.InternalToken,
                            "Multiple access modifiers are not allowed (with the exception of internal + protected");
                    }
                    else
                    {
                        this.AccessModifierType = AccessModifierType.INTERNAL_PROTECTED;
                    }
                }
            }

            if (this.HasFinal && this.HasAbstract)
            {
                throw new ParserException(this.FinalToken, "Entity cannot be both final and abstract.");
            }
        }

        public static ModifierCollection CreateStaticModifier(Token aToken)
        {
            ModifierCollection modifiers = new ModifierCollection(new Token[0]);
            modifiers.FirstToken = aToken;
            modifiers.tokens[(int)ModifierType.STATIC] = aToken;
            return modifiers;
        }

        public bool HasAbstract { get { return this.tokens[(int)ModifierType.ABSTRACT] != null; } }
        public Token AbstractToken { get { return this.tokens[(int)ModifierType.ABSTRACT]; } }
        public bool HasFinal { get { return this.tokens[(int)ModifierType.FINAL] != null; } }
        public Token FinalToken { get { return this.tokens[(int)ModifierType.FINAL]; } }
        public bool HasInternal { get { return this.tokens[(int)ModifierType.INTERNAL] != null; } }
        public Token InternalToken { get { return this.tokens[(int)ModifierType.INTERNAL]; } }
        public bool HasOverride { get { return this.tokens[(int)ModifierType.OVERRIDE] != null; } }
        public Token OverrideToken { get { return this.tokens[(int)ModifierType.OVERRIDE]; } }
        public bool HasPrivate { get { return this.tokens[(int)ModifierType.PRIVATE] != null; } }
        public Token PrivateToken { get { return this.tokens[(int)ModifierType.PRIVATE]; } }
        public bool HasProtected { get { return this.tokens[(int)ModifierType.PROTECTED] != null; } }
        public Token ProtectedToken { get { return this.tokens[(int)ModifierType.PROTECTED]; } }
        public bool HasPublicExplicitly { get { return this.tokens[(int)ModifierType.PUBLIC] != null; } }
        public Token PublicToken { get { return this.tokens[(int)ModifierType.PUBLIC]; } }
        public bool HasStatic { get { return this.tokens[(int)ModifierType.STATIC] != null; } }
        public Token StaticToken { get { return this.tokens[(int)ModifierType.STATIC]; } }
    }
}
