using Localization;
using System.Collections.Generic;

namespace Parser.ParseTree
{
    public class ConstStatement : TopLevelConstruct
    {
        public Expression Expression { get; set; }
        public Token NameToken { get; private set; }
        public string Name { get; private set; }
        private AnnotationCollection annotations;

        public ConstStatement(
            Token constToken,
            Token nameToken,
            TopLevelConstruct owner,
            LibraryMetadata library,
            FileScope fileScope,
            AnnotationCollection annotations)
            : base(constToken, owner, fileScope)
        {
            this.Library = library;
            this.NameToken = nameToken;
            this.Name = nameToken.Value;
            this.annotations = annotations;
        }

        private Dictionary<Locale, string> namesByLocale = null;
        public override string GetFullyQualifiedLocalizedName(Locale locale)
        {
            if (this.namesByLocale == null) this.namesByLocale = this.annotations.GetNamesByLocale(1);
            string name = this.NameToken.Value;
            if (this.namesByLocale.ContainsKey(locale)) name = this.namesByLocale[locale];

            if (this.Owner != null)
            {
                name = this.Owner.GetFullyQualifiedLocalizedName(locale) + "." + name;
            }
            return name;
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

            this.Expression = this.Expression.Resolve(parser);

            if (!(this.Expression is IConstantValue))
            {
                throw new ParserException(this.FirstToken, "Invalid value for const. Expression must resolve to a constant at compile time.");
            }

            parser.ConstantAndEnumResolutionState[this] = ConstantResolutionState.RESOLVED;
        }

        internal override void ResolveEntityNames(ParserContext parser)
        {
            this.Expression = this.Expression.ResolveEntityNames(parser);
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableIdAllocator varIds, VariableIdAllocPhase phase) { }
    }
}
