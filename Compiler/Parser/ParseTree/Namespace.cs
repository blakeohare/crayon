using Localization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Parser.ParseTree
{
    // Despite being an "Executable", this isn't an executable thing.
    // It will get optimized away at resolution time.
    public class Namespace : TopLevelConstruct
    {
        public TopLevelConstruct[] Code { get; set; }
        public string DefaultName { get; set; }
        public string FullyQualifiedDefaultName { get; set; }
        public Dictionary<Locale, string[]> NamesByLocale { get; private set; }
        public string[] DefaultNameSegments { get; private set; }
        public string[] FullyQualifiedDefaultNameSegments { get; private set; }

        // How many namespaces is this nested under.
        public int NestDepth { get; private set; }

        private Dictionary<Locale, string[]> fullyQualifiedNamesByLocale = new Dictionary<Locale, string[]>();

        public override string ToString()
        {
            return "Namespace: " + this.DefaultName + " (" + this.FullyQualifiedDefaultName + ")";
        }

        public Namespace(
            Token namespaceToken,
            string name,
            TopLevelConstruct owner,
            LibraryMetadata library,
            FileScope fileScope,
            AnnotationCollection annotations)
            : base(namespaceToken, owner, fileScope)
        {
            this.Library = library;
            this.DefaultName = name;
            this.FullyQualifiedDefaultName = owner == null
                ? name
                : (((Namespace)owner).FullyQualifiedDefaultName + "." + name);
            this.FullyQualifiedDefaultNameSegments = this.FullyQualifiedDefaultName.Split('.');
            this.DefaultNameSegments = this.DefaultName.Split('.');

            this.NamesByLocale = annotations.GetNamesByLocale(this.DefaultNameSegments.Length)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Split('.'));

            Locale defaultLocale = fileScope.CompilationScope.Locale;
            if (!this.NamesByLocale.ContainsKey(defaultLocale))
            {
                this.NamesByLocale[defaultLocale] = this.DefaultName.Split('.');
            }

            this.NestDepth = this.FullyQualifiedDefaultNameSegments.Length - this.DefaultNameSegments.Length;
        }

        public override string GetFullyQualifiedLocalizedName(Locale locale)
        {
            return this.FileScope.CompilationScope.GetNamespaceNameForLocale(locale, this);
        }

        internal override void Resolve(ParserContext parser)
        {
            throw new ParserException(this.FirstToken, "Namespace declaration not allowed here. Namespaces may only exist in the root of a file or nested within other namespaces.");
        }

        internal override void ResolveNames(ParserContext parser)
        {
            throw new InvalidOperationException();
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }
        internal override void PerformLocalIdAllocation(ParserContext parser, VariableIdAllocator varIds, VariableIdAllocPhase phase)
        {
            // Not called in this way.
            throw new NotImplementedException();
        }
    }
}
