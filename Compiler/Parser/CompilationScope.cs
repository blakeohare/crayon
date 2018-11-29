using Build;
using Localization;
using Parser.ParseTree;
using System.Collections.Generic;
using System.Linq;

namespace Parser
{
    public class CompilationScope
    {
        public string ScopeKey { get; private set; }
        public Dictionary<string, CniFunction> CniFunctionsByName { get; private set; }
        public AssemblyMetadata Metadata { get; private set; }
        public int ScopeNumId { get; private set; }

        public bool IsCrayon {  get { return this.ProgrammingLanguage == ProgrammingLanguage.CRAYON; } }
        public bool IsAcrylic {  get { return this.ProgrammingLanguage == ProgrammingLanguage.ACRYLIC; } }
        public bool IsStaticallyTyped {  get { return this.IsAcrylic; } }

        private BuildContext buildContext;
        private Dictionary<CompilationScope, LocalizedAssemblyView> dependenciesAndViews = new Dictionary<CompilationScope, LocalizedAssemblyView>();

        private List<TopLevelEntity> executables = new List<TopLevelEntity>();

        private ScopedNamespaceLocaleFlattener namespaceFlattener = new ScopedNamespaceLocaleFlattener();

        private static int numIdAlloc = 1;

        public CompilationScope(BuildContext buildContext, AssemblyMetadata metadata)
        {
            this.ScopeNumId = numIdAlloc++;
            this.buildContext = buildContext;
            this.CniFunctionsByName = new Dictionary<string, CniFunction>();
            this.Metadata = metadata;
            this.Metadata.Scope = this;
            this.ScopeKey = this.Metadata.CanonicalKey;

            foreach (string cniFuncName in metadata.CniFunctions.Keys)
            {
                this.RegisterCniFunction(cniFuncName, metadata.CniFunctions[cniFuncName]);
            }
        }

        private ProgrammingLanguage? programmingLanguage = null;

        public ProgrammingLanguage ProgrammingLanguage
        {
            get
            {
                if (this.programmingLanguage == null)
                {
                    this.programmingLanguage = this.GetProgrammingLanguage();
                }
                return this.programmingLanguage.Value;
            }
        }

        public ProgrammingLanguage GetProgrammingLanguage()
        {
            if (this.Metadata.IsUserDefined)
            {
                return this.buildContext.TopLevelAssembly.ProgrammingLanguage;
            }

            Dictionary<string, string> embeddedCode = this.Metadata.GetSourceCode();
            string filename = embeddedCode.Keys.Where(name => name.Contains('.')).FirstOrDefault();
            if (filename != null)
            {
                filename = filename.ToLowerInvariant();
                if (filename.EndsWith(".acr"))
                {
                    return ProgrammingLanguage.ACRYLIC;
                }
            }
            return ProgrammingLanguage.CRAYON;
        }

        public Locale Locale
        {
            get
            {
                if (this.Metadata != null)
                {
                    return this.Metadata.InternalLocale;
                }
                return this.buildContext.CompilerLocale;
            }
        }

        public List<TopLevelEntity> GetExecutables_HACK()
        {
            return this.executables;
        }

        public void AddExecutable(TopLevelEntity executable)
        {
            if (executable is Namespace)
            {
                Namespace ns = (Namespace)executable;
                this.namespaceFlattener.AddNamespace(ns);
                foreach (TopLevelEntity tlc in ns.Code)
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

        public TopLevelEntity[] GetTopLevelConstructs()
        {
            return this.executables.ToArray();
        }

        public void RegisterCniFunction(string name, int args)
        {
            if (this.CniFunctionsByName.ContainsKey(name))
            {
                throw new ParserException("There are multiple CNI functions registered with the same name: '" + name + "'");
            }

            CniFunction func = new CniFunction(this, name, args);
            this.CniFunctionsByName.Add(name, func);
        }

        public void AddDependency(Token throwToken, LocalizedAssemblyView view)
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
        }

        public LocalizedAssemblyView[] Dependencies
        {
            get
            {
                return this.dependenciesAndViews.Values.OrderBy(lib => lib.Scope.Metadata.ID).ToArray();
            }
        }

        public void FlattenFullyQualifiedLookupsIntoGlobalLookup(Dictionary<string, TopLevelEntity> output, Locale verifiedCallingLocale)
        {
            // Add namespaces to the lookup but then remove them. I'd like for the collision detection to run here for namespaces.
            HashSet<string> keysToRemove = new HashSet<string>();

            foreach (TopLevelEntity entity in this.GetTopLevelConstructs())
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
