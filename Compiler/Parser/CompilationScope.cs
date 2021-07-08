using Common;
using Common.Localization;
using Parser.ParseTree;
using System.Collections.Generic;
using System.Linq;

namespace Parser
{
    internal class CompilationScope
    {
        public string ScopeKey { get; private set; }
        public ExternalAssemblyMetadata Metadata { get; private set; }
        public int ScopeNumId { get; private set; }
        public Locale Locale { get; private set; }
        public ProgrammingLanguage ProgrammingLanguage { get; private set; }

        public bool IsCrayon {  get { return this.ProgrammingLanguage == ProgrammingLanguage.CRAYON; } }
        public bool IsAcrylic {  get { return this.ProgrammingLanguage == ProgrammingLanguage.ACRYLIC; } }
        public bool IsStaticallyTyped {  get { return this.IsAcrylic; } }

        private Dictionary<CompilationScope, LocalizedAssemblyView> dependenciesAndViews = new Dictionary<CompilationScope, LocalizedAssemblyView>();

        private List<TopLevelEntity> entities = new List<TopLevelEntity>();

        private ScopedNamespaceLocaleFlattener namespaceFlattener = new ScopedNamespaceLocaleFlattener();

        private static int numIdAlloc = 1;

        public CompilationScope(
            ExternalAssemblyMetadata metadata,
            Locale locale,
            ProgrammingLanguage programmingLanguage)
        {
            this.Locale = locale;
            this.ProgrammingLanguage = programmingLanguage;
            this.ScopeNumId = numIdAlloc++;
            this.Metadata = metadata;
            this.ScopeKey = this.Metadata.CanonicalKey;
        }

        internal void AddExecutable(TopLevelEntity entity)
        {
            if (entity is Namespace)
            {
                Namespace ns = (Namespace)entity;
                this.namespaceFlattener.AddNamespace(ns);
                foreach (TopLevelEntity tlc in ns.Code)
                {
                    this.AddExecutable(tlc);
                }
            }
            else
            {
                this.entities.Add(entity);
            }
        }

        // The localized view is an external view of a library, whereas this
        // is the actual compilation of the internal localization data.
        private Dictionary<string, string> getNamespaceNameForLocaleCache = new Dictionary<string, string>();
        internal string GetNamespaceNameForLocale(Locale locale, Namespace ns)
        {
            string fullyQualifiedDefaultName = ns.FullyQualifiedDefaultName;
            string key = locale.ID + ":" + fullyQualifiedDefaultName;
            if (!this.getNamespaceNameForLocaleCache.ContainsKey(key))
            {
                Dictionary<string, NamespaceReferenceTemplate> lookup = this.namespaceFlattener.GetLookup(locale);
                foreach (NamespaceReferenceTemplate nrt in lookup.Values)
                {
                    string originalName = nrt.GetFullyQualifiedName();
                    string localizedName = nrt.Name;
                    getNamespaceNameForLocaleCache[locale.ID + ":" + originalName] = localizedName;
                }
            }
            return this.getNamespaceNameForLocaleCache[key];
        }

        internal Dictionary<string, NamespaceReferenceTemplate> GetFlattenedNamespaceLookup(Locale locale)
        {
            return this.namespaceFlattener.GetLookup(locale);
        }

        internal TopLevelEntity[] GetTopLevelEntities()
        {
            return this.entities.ToArray();
        }

        internal void AddDependency(Token throwToken, LocalizedAssemblyView view)
        {
            if (this.dependenciesAndViews.ContainsKey(view.Scope))
            {
                if (this.dependenciesAndViews[view.Scope] != view)
                {
                    throw ParserException.ThrowException(
                        view.Locale,
                        ErrorMessages.CANNOT_IMPORT_SAME_LIBRARY_FROM_DIFFERENT_LOCALES,
                        throwToken);
                }
            }
            this.dependenciesAndViews[view.Scope] = view;
            this.Metadata.RegisterDependencies(view.Scope.Metadata);
        }

        public LocalizedAssemblyView[] Dependencies
        {
            get
            {
                return this.dependenciesAndViews.Values.OrderBy(lib => lib.Scope.Metadata.ID).ToArray();
            }
        }

        internal void FlattenFullyQualifiedLookupsIntoGlobalLookup(Dictionary<string, TopLevelEntity> output, Locale verifiedCallingLocale)
        {
            // Add namespaces to the lookup but then remove them. I'd like for the collision detection to run here for namespaces.
            HashSet<string> keysToRemove = new HashSet<string>();

            foreach (TopLevelEntity entity in this.GetTopLevelEntities())
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

        internal ClassDefinition[] GetAllClassDefinitions()
        {
            return this.entities
                .OfType<ClassDefinition>()
                .OrderBy(cd => cd.NameToken.Value)
                .ToArray();
        }

        public override string ToString()
        {
            if (this.Metadata.IsUserDefined)
            {
                return "User Code Scope [" + this.Locale + "]";
            }
            return "Library Scope [" + this.Metadata.ID + " | " + this.Locale + "]";
        }
    }
}
