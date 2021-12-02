using Parser.Localization;
using Parser.Resolver;
using System.Collections.Generic;
using System.Linq;

namespace Parser.ParseTree
{
    internal class EnumDefinition : TopLevelEntity
    {
        public string Name { get; private set; }
        public Token NameToken { get; private set; }
        public Token[] Items { get; private set; }
        public Expression[] Values { get; private set; }
        public Dictionary<string, int> IntValue { get; private set; }
        private AnnotationCollection annotations;

        public EnumDefinition(
            Token enumToken,
            Token nameToken,
            Node owner,
            FileScope fileScope,
            ModifierCollection modifiers,
            AnnotationCollection annotations)
            : base(enumToken, owner, fileScope, modifiers)
        {
            this.NameToken = nameToken;
            this.Name = nameToken.Value;
            this.annotations = annotations;

            if (modifiers.AccessModifierType == AccessModifierType.PRIVATE ||
                modifiers.AccessModifierType == AccessModifierType.PROTECTED)
            {
                // TODO: this will not be true when you can start nesting these into classes.
                throw new ParserException(modifiers.PrivateToken ?? modifiers.ProtectedToken, "This is not a valid access modifier for enums.");
            }
        }

        private Dictionary<Locale, string> namesByLocale = null;
        public override string GetFullyQualifiedLocalizedName(Locale locale)
        {
            if (this.namesByLocale == null) this.namesByLocale = this.annotations.GetNamesByLocale(1);
            string name = this.NameToken.Value;
            if (this.namesByLocale.ContainsKey(locale)) name = this.namesByLocale[locale];

            if (this.TopLevelEntity != null)
            {
                name = this.TopLevelEntity.GetFullyQualifiedLocalizedName(locale) + "." + name;
            }
            return name;
        }

        public void SetItems(IList<Token> items, IList<Expression> values)
        {
            this.Items = items.ToArray();
            this.Values = values.ToArray();
            this.IntValue = new Dictionary<string, int>();

            if (this.Items.Length == 0)
            {
                throw new ParserException(this, "Enum definitions cannot be empty.");
            }
        }

        internal override void Resolve(ParserContext parser)
        {
            ConstantResolutionState resolutionState = parser.ConstantAndEnumResolutionState[this];
            if (resolutionState == ConstantResolutionState.RESOLVED) return;
            if (resolutionState == ConstantResolutionState.RESOLVING)
            {
                throw new ParserException(this, "The resolution of this enum creates a cycle.");
            }
            parser.ConstantAndEnumResolutionState[this] = ConstantResolutionState.RESOLVING;

            HashSet<int> consumed = new HashSet<int>();

            for (int i = 0; i < this.Items.Length; ++i)
            {
                string itemName = this.Items[i].Value;

                if (itemName == "length")
                {
                    throw new ParserException(this.Items[i], "The name 'length' is not allowed as an enum value as it is a reserved field. In general, enum members should be in ALL CAPS anyway.");
                }

                if (this.IntValue.ContainsKey(itemName))
                {
                    throw new ParserException(this.Items[i], "Duplicate item in same enum. ");
                }

                this.IntValue[itemName] = -1;

                if (this.Values[i] != null)
                {
                    IntegerConstant ic = this.Values[i].Resolve(parser) as IntegerConstant;
                    if (ic == null)
                    {
                        throw new ParserException(this.Values[i], "Enum values must be integers or left blank.");
                    }
                    this.Values[i] = ic;
                    if (consumed.Contains(ic.Value))
                    {
                        throw new ParserException(this.Values[i], "This integer value has already been used in the same enum.");
                    }

                    consumed.Add(ic.Value);
                    this.IntValue[itemName] = ic.Value;
                }
            }
            parser.ConstantAndEnumResolutionState[this] = ConstantResolutionState.RESOLVED;

            int next = 0;
            for (int i = 0; i < this.Items.Length; ++i)
            {
                if (this.Values[i] == null)
                {
                    while (consumed.Contains(next))
                    {
                        ++next;
                    }

                    this.IntValue[this.Items[i].Value] = next;
                    consumed.Add(next);
                }
            }
        }

        internal override void ResolveEntityNames(ParserContext parser)
        {
            this.BatchExpressionEntityNameResolver(parser, this.Values);
        }

        internal override void ResolveSignatureTypes(ParserContext parser, TypeResolver typeResolver)
        {
            // Nothing to do.
        }

        // enums are exempt from this check
        internal override void EnsureModifierAndTypeSignatureConsistency(TypeContext tc)
        {
            throw new System.NotImplementedException();
        }

        internal override void ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            foreach (Expression value in this.Values)
            {
                if (value != null)
                {
                    value.ResolveTypes(parser, typeResolver);
                    if (value.ResolvedType != parser.TypeContext.INTEGER)
                    {
                        throw new ParserException(value, "Enum value must resolve to an integer.");
                    }
                }
            }
        }
    }
}
