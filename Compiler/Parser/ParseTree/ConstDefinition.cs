using Localization;
using Parser.Resolver;
using System.Collections.Generic;

namespace Parser.ParseTree
{
    public class ConstDefinition : TopLevelEntity
    {
        public AType Type { get; set; }
        public ResolvedType ResolvedType { get; private set; }
        public Expression Expression { get; set; }
        public Token NameToken { get; private set; }
        public string Name { get; private set; }
        private AnnotationCollection annotations;

        public ConstDefinition(
            Token constToken,
            AType type,
            Token nameToken,
            Node owner,
            FileScope fileScope,
            ModifierCollection modifiers,
            AnnotationCollection annotations)
            : base(constToken, owner, fileScope, modifiers)
        {
            this.NameToken = nameToken;
            this.Name = nameToken.Value;
            this.Type = type;
            this.annotations = annotations;

            if (modifiers.AccessModifierType == AccessModifierType.PRIVATE ||
                modifiers.AccessModifierType == AccessModifierType.PROTECTED)
            {
                // TODO: this will not be true when you can start nesting these into classes.
                throw new ParserException(modifiers.PrivateToken ?? modifiers.ProtectedToken, "This is not a valid access modifier for consts.");
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

        internal override void Resolve(ParserContext parser)
        {
            ConstantResolutionState resolutionState = parser.ConstantAndEnumResolutionState[this];
            if (resolutionState == ConstantResolutionState.RESOLVED) return;
            if (resolutionState == ConstantResolutionState.RESOLVING)
            {
                throw new ParserException(this, "The resolution of this enum creates a cycle.");
            }
            parser.ConstantAndEnumResolutionState[this] = ConstantResolutionState.RESOLVING;

            this.Expression = this.Expression.Resolve(parser);

            if (!(this.Expression is IConstantValue))
            {
                throw new ParserException(this, "Invalid value for const. Expression must resolve to a constant at compile time.");
            }

            parser.ConstantAndEnumResolutionState[this] = ConstantResolutionState.RESOLVED;
        }

        internal override void ResolveEntityNames(ParserContext parser)
        {
            this.Expression = this.Expression.ResolveEntityNames(parser);
        }

        internal void ValidateConstTypeSignature()
        {
            switch (this.ResolvedType.Category)
            {
                case ResolvedTypeCategory.VOID:
                    throw new ParserException(this, "Constant expression cannot have a be void type.");

                case ResolvedTypeCategory.ANY:
                case ResolvedTypeCategory.BOOLEAN:
                case ResolvedTypeCategory.INTEGER:
                case ResolvedTypeCategory.FLOAT:
                case ResolvedTypeCategory.NULL:
                case ResolvedTypeCategory.STRING:
                    // This is fine.
                    break;

                default:
                    throw new ParserException(this, "This is not a valid constant expression.");
            }

            this.Expression.ResolvedType.EnsureCanAssignToA(this.FirstToken, this.ResolvedType);
        }

        internal override void ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            bool hasExplicitlySetType = this.CompilationScope.IsStaticallyTyped;
            if (hasExplicitlySetType)
            {
                this.ResolvedType = typeResolver.ResolveType(this.Type);
            }

            this.Expression = this.Expression.ResolveTypes(parser, typeResolver);

            if (this.Expression.ResolvedType == ResolvedType.ANY)
            {
                throw new ParserException(this.Expression, "This expression does not resolve to a constant.");
            }

            if (!hasExplicitlySetType) {
                this.ResolvedType = this.Expression.ResolvedType;
            }
        }

        // Signature types are implicitly declared by the contents for dynamically typed language (Crayon)
        // or set in the outer loop in the resolver pipeline in statically typed languages (Acrylic)
        internal override void ResolveSignatureTypes(ParserContext parser, TypeResolver typeResolver)
        {
            throw new System.NotImplementedException();
        }

        // Consts are exempt from this check.
        internal override void EnsureModifierAndTypeSignatureConsistency()
        {
            throw new System.NotImplementedException();
        }
    }
}
