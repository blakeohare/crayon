using Build;
using Common;
using Localization;
using Parser.ParseTree;
using System.Collections.Generic;
using System.Linq;

namespace Parser
{
    public abstract class CompilationScope
    {
        public virtual Locale Locale { get; }
        public virtual string ScopeKey { get; }

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
                    throw new ParserException(throwToken, "Cannot import the same library multiple times from different locales.");
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
                    throw new ParserException(entity.FirstToken, "Two items have identical fully-qualified names: '" + name + "'");
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
