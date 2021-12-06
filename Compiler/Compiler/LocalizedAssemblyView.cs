using Builder.Localization;
using Builder.ParseTree;
using System.Collections.Generic;

namespace Builder
{
    internal class LocalizedAssemblyView
    {
        public Locale Locale { get; private set; }
        public CompilationScope Scope { get; private set; }

        internal Dictionary<string, TopLevelEntity> FullyQualifiedEntityLookup { get; private set; }

        public LocalizedAssemblyView(Locale locale, CompilationScope scope)
        {
            this.Locale = locale;
            this.Scope = scope;
            this.FullyQualifiedEntityLookup = null;
        }

        public string Name
        {
            get { return this.Scope.Metadata.GetName(this.Locale); }
        }
    }
}
