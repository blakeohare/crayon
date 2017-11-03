using Common;
using Localization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Parser.ParseTree
{
    public class EnumDefinition : TopLevelConstruct
    {
        public string Name { get; private set; }
        public Token NameToken { get; private set; }
        public Token[] Items { get; private set; }
        public Expression[] Values { get; private set; }
        public Dictionary<string, int> IntValue { get; private set; }
        private Multimap<string, Annotation> annotations;

        public EnumDefinition(
            Token enumToken, 
            Token nameToken, 
            TopLevelConstruct owner, 
            LibraryMetadata library, 
            FileScope fileScope,
            Multimap<string, Annotation> annotations)
            : base(enumToken, owner, fileScope)
        {
            this.Library = library;
            this.NameToken = nameToken;
            this.Name = nameToken.Value;
            this.annotations = annotations;
        }

        private Dictionary<Locale, string> namesByLocale = null;
        public override string GetFullyQualifiedLocalizedName(Locale locale)
        {
            if (this.namesByLocale == null) this.namesByLocale = Annotation.GetNamesByLocale(this.annotations, 1);
            string name = this.NameToken.Value;
            if (this.namesByLocale.ContainsKey(locale)) name = this.namesByLocale[locale];

            if (this.Owner != null)
            {
                name = this.Owner.GetFullyQualifiedLocalizedName(locale) + "." + name;
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
                throw new ParserException(this.FirstToken, "Enum definitions cannot be empty.");
            }
        }

        internal override void Resolve(ParserContext parser)
        {
            ConstantResolutionState resolutionState = parser.ConstantAndEnumResolutionState[this];
            if (resolutionState == ConstantResolutionState.RESOLVED) return;
            if (resolutionState == ConstantResolutionState.RESOLVING)
            {
                throw new ParserException(this.FirstToken, "The resolution of this enum creates a cycle.");
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
                        throw new ParserException(this.Values[i].FirstToken, "Enum values must be integers or left blank.");
                    }
                    this.Values[i] = ic;
                    if (consumed.Contains(ic.Value))
                    {
                        throw new ParserException(this.Values[i].FirstToken, "This integer value has already been used in the same enum.");
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

        internal override void ResolveNames(ParserContext parser)
        {
            this.BatchExpressionNameResolver(parser, this.Values);
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableIdAllocator varIds, VariableIdAllocPhase phase)
        {
            // Not called this way.
            throw new InvalidOperationException();
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }
    }
}
