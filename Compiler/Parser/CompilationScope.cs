using Build;
using Localization;
using Parser.ParseTree;
using System.Collections.Generic;
using System.Linq;

namespace Parser
{
    public abstract class CompilationScope
    {
        public abstract Locale Locale { get; }
        public abstract string ScopeKey { get; }

        protected BuildContext buildContext;
        private Dictionary<LibraryCompilationScope, LocalizedLibraryView> dependenciesAndViews = new Dictionary<LibraryCompilationScope, LocalizedLibraryView>();

        private List<TopLevelConstruct> executables = new List<TopLevelConstruct>();

        private ScopedNamespaceLocaleFlattener namespaceFlattener = new ScopedNamespaceLocaleFlattener();

        public List<TopLevelConstruct> GetExecutables_HACK()
        {
            return this.executables;
        }

        public void AddExecutable(TopLevelConstruct executable)
        {
            if (executable is Namespace)
            {
                Namespace ns = (Namespace)executable;
                this.namespaceFlattener.AddNamespace(ns);
                foreach (TopLevelConstruct tlc in ns.Code)
                {
                    this.AddExecutable(tlc);
                }
            }
            else
            {
                this.executables.Add(executable);
            }
        }

        // The localized view is an external view of a library, whereas this
        // is the actual compilation of the internal localization data.
        private Dictionary<string, string> getNamespaceNameForLocaleCache = new Dictionary<string, string>();
        public string GetNamespaceNameForLocale(Locale locale, Namespace ns)
        {
            string fullyQualifiedDefaultName = ns.FullyQualifiedDefaultName;
            string key = locale.ID + ":" + fullyQualifiedDefaultName;
            if (!this.getNamespaceNameForLocaleCache.ContainsKey(key))
            {
                Dictionary<string, NamespaceReferenceTemplate> lookup = this.namespaceFlattener.GetLookup(locale);
                foreach (NamespaceReferenceTemplate nrt in lookup.Values)
                {
                    string originalName = nrt.OriginalNamespace.FullyQualifiedDefaultName;
                    string localizedName = nrt.Name;
                    getNamespaceNameForLocaleCache[locale.ID + ":" + originalName] = localizedName;
                }
            }
            return this.getNamespaceNameForLocaleCache[key];
        }

        public Dictionary<string, NamespaceReferenceTemplate> GetFlattenedNamespaceLookup(Locale locale)
        {
            return this.namespaceFlattener.GetLookup(locale);
        }

        public TopLevelConstruct[] GetTopLevelConstructs()
        {
            return this.executables.ToArray();
        }

        public CompilationScope(BuildContext buildContext)
        {
            this.buildContext = buildContext;
        }

        public void AddDependency(Token throwToken, LocalizedLibraryView libraryView)
        {
            if (this == libraryView.LibraryScope) throw new System.Exception(); // This should not happen.

            if (this.dependenciesAndViews.ContainsKey(libraryView.LibraryScope))
            {
                if (this.dependenciesAndViews[libraryView.LibraryScope] != libraryView)
                {
                    throw ParserException.ThrowException(
                        libraryView.Locale,
                        ErrorMessages.CANNOT_IMPORT_SAME_LIBRARY_FROM_DIFFERENT_LOCALES,
                        throwToken);
                }
            }
            this.dependenciesAndViews[libraryView.LibraryScope] = libraryView;
        }

        public LocalizedLibraryView[] Dependencies
        {
            get
            {
                return this.dependenciesAndViews.Values.OrderBy(lib => lib.LibraryScope.Library.ID).ToArray();
            }
        }

        public void FlattenFullyQualifiedLookupsIntoGlobalLookup(Dictionary<string, TopLevelConstruct> output, Locale verifiedCallingLocale)
        {
            // Add namespaces to the lookup but then remove them. I'd like for the collision detection to run here for namespaces.
            HashSet<string> keysToRemove = new HashSet<string>();

            foreach (TopLevelConstruct entity in this.GetTopLevelConstructs())
            {
                string name = entity.GetFullyQualifiedLocalizedName(verifiedCallingLocale);
                if (output.ContainsKey(name))
                {
                    throw new ParserException(entity, "Two items have identical fully-qualified names: '" + name + "'");
                }
                output[name] = entity;
                if (entity is Namespace)
                {
                    keysToRemove.Add(name);
                }
            }

            foreach (string key in keysToRemove)
            {
                output.Remove(key);
            }
        }

        public ClassDefinition[] GetAllClassDefinitions()
        {
            return this.executables
                .OfType<ClassDefinition>()
                .OrderBy(cd => cd.NameToken.Value)
                .ToArray();
        }
    }

    public class UserCodeCompilationScope : CompilationScope
    {
        public UserCodeCompilationScope(BuildContext buildContext) : base(buildContext)
        { }

        public override Locale Locale
        {
            get { return this.buildContext.CompilerLocale; }
        }

        public override string ScopeKey
        {
            get { return "."; }
        }

        public override string ToString()
        {
            return "User Code Scope [" + this.Locale + "]";
        }
    }

    public class LibraryCompilationScope : CompilationScope
    {
        public LibraryMetadata Library { get; private set; }
        private string scopeKey;

        public LibraryCompilationScope(BuildContext buildContext, LibraryMetadata library) : base(buildContext)
        {
            this.Library = library;
            this.scopeKey = library.CanonicalKey;
            this.Library.LibraryScope = this;
        }

        public override Locale Locale
        {
            get { return this.Library.InternalLocale; }
        }

        public override string ScopeKey { get { return this.scopeKey; } }

        public override string ToString()
        {
            return "Library Scope [" + this.Library.ID + " | " + this.Locale + "]";
        }
    }
}
