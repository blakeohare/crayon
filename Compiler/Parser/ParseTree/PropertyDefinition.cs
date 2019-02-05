using Localization;
using Parser.Resolver;
using System;
using System.Collections.Generic;

namespace Parser.ParseTree
{
    public class PropertyDefinition : TopLevelEntity, ICodeContainer
    {
        public PropertyMember Getter { get; set; }
        public PropertyMember Setter { get; set; }
        public HashSet<string> ArgumentNameLookup { get; private set; }

        public PropertyDefinition(Token firstToken, Node owner, FileScope file, ModifierCollection modifiers)
            : base(firstToken, owner, file, modifiers)
        {
            this.ArgumentNameLookup = new HashSet<string>();
        }

        public List<Lambda> Lambdas { get; private set; }

        public override string GetFullyQualifiedLocalizedName(Locale locale)
        {
            throw new NotImplementedException();
        }

        internal override void EnsureModifierAndTypeSignatureConsistency()
        {
            throw new NotImplementedException();
        }

        internal override void Resolve(ParserContext parser)
        {
            throw new NotImplementedException();
        }

        internal override void ResolveEntityNames(ParserContext parser)
        {
            throw new NotImplementedException();
        }

        internal override void ResolveSignatureTypes(ParserContext parser, TypeResolver typeResolver)
        {
            throw new NotImplementedException();
        }

        internal override void ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            throw new NotImplementedException();
        }
    }
}
