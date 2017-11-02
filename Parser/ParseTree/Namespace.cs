using Common;
using Localization;
using System;
using System.Collections.Generic;

namespace Parser.ParseTree
{
    // Despite being an "Executable", this isn't an executable thing.
    // It will get optimized away at resolution time.
    public class Namespace : TopLevelConstruct
    {
        public TopLevelConstruct[] Code { get; set; }
        public string DefaultName { get; set; }
        public string FullyQualifiedDefaultName { get; set; }
        public Dictionary<Locale, string> NamesByLocale { get; private set; }

        public Namespace(
            Token namespaceToken,
            string name,
            TopLevelConstruct owner,
            LibraryMetadata library,
            FileScope fileScope,
            Multimap<string, Annotation> annotations)
            : base(namespaceToken, owner, fileScope)
        {
            this.Library = library;
            this.DefaultName = name;
            this.FullyQualifiedDefaultName = owner == null
                ? name
                : (((Namespace)owner).FullyQualifiedDefaultName + "." + name);
            this.NamesByLocale = new Dictionary<Locale, string>();
            // TODO: move this
            if (annotations != null)
            {
                foreach (Annotation localeAnnotation in annotations["localized"])
                {
                    string locale = ((StringConstant)localeAnnotation.Args[0]).Value;
                    string localizedName = ((StringConstant)localeAnnotation.Args[1]).Value;
                    this.NamesByLocale[Locale.Get(locale)] = localizedName;
                }
            }
        }

        public override string GetFullyQualifiedLocalizedName(Locale locale)
        {
            if (this.NamesByLocale.ContainsKey(locale))
            {
                return this.NamesByLocale[locale];
            }
            return this.DefaultName;
        }

        internal override void Resolve(ParserContext parser)
        {
            throw new ParserException(this.FirstToken, "Namespace declaration not allowed here. Namespaces may only exist in the root of a file or nested within other namespaces.");
        }

        internal override void ResolveNames(ParserContext parser)
        {
            throw new InvalidOperationException();
        }

        internal override void GetAllVariableNames(Dictionary<string, bool> lookup)
        {
            throw new NotImplementedException();
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }
        internal override void PerformLocalIdAllocation(ParserContext parser, VariableIdAllocator varIds, VariableIdAllocPhase phase)
        {
            // Not called in this way.
            throw new NotImplementedException();
        }
    }
}
