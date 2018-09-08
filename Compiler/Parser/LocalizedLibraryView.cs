using Localization;
using Parser.ParseTree;
using System.Collections.Generic;

namespace Parser
{
    public class LocalizedLibraryView
    {
        public Locale Locale { get; private set; }
        public CompilationScope LibraryScope { get; private set; }

        public Dictionary<string, TopLevelEntity> FullyQualifiedEntityLookup { get; private set; }

        public LocalizedLibraryView(Locale locale, CompilationScope libraryScope)
        {
            this.Locale = locale;
            this.LibraryScope = libraryScope;
            this.FullyQualifiedEntityLookup = null;
        }

        public string Name
        {
            get { return this.LibraryScope.Metadata.GetName(this.Locale); }
        }
    }
}
