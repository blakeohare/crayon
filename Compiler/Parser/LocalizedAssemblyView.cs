using Localization;
using Parser.ParseTree;
using System.Collections.Generic;

namespace Parser
{
    public class LocalizedAssemblyView
    {
        public Locale Locale { get; private set; }
        public CompilationScope Scope { get; private set; }

        public Dictionary<string, TopLevelEntity> FullyQualifiedEntityLookup { get; private set; }

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
