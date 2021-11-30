using Parser.Localization;
using CommonUtil.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Parser
{
    internal class ScopeManager
    {
        private Wax.WaxHub wax;
        private string[] localDeps;
        private string projectDirectory;

        private Dictionary<string, CompilationScope> importedAssembliesById = new Dictionary<string, CompilationScope>();
        private Dictionary<Locale, Dictionary<string, LocalizedAssemblyView>> importedAssembliesByLocalizedName = new Dictionary<Locale, Dictionary<string, LocalizedAssemblyView>>();

        internal List<CompilationScope> ImportedAssemblyScopes { get; private set; }
        // The index + 1 is the reference ID
        private readonly Dictionary<string, int> assembliesAlreadyImportedIndexByKey = new Dictionary<string, int>();

        public ScopeManager(CompileRequest compileRequest, Wax.WaxHub waxHub)
        {
            this.wax = waxHub;
            this.localDeps = compileRequest.LocalDeps;
            this.projectDirectory = compileRequest.ProjectDirectory;
            this.ImportedAssemblyScopes = new List<CompilationScope>();
        }

        public bool IsValidAssemblyNameFromLocale(Locale locale, string name)
        {
            // TODO: This is only checking if the library is available in this locale.
            // It needs to check for all libraries.
            // This is currently only used in slim circumstances.

            Dictionary<string, object> response = this.wax.AwaitSendRequest("assembly", new Dictionary<string, object>() {
                { "command", "GetAssemblyMetadataFromAnyPossibleKey" },
                { "locale", locale.ID },
                { "name", name },
                { "localDeps", this.localDeps },
                { "projectDir", this.projectDirectory },
                { "includeSource", false },
            });

            return (bool)response["found"];
        }

        private Dictionary<string, ExternalAssemblyMetadata> assemblyCache = new Dictionary<string, ExternalAssemblyMetadata>();
        private ExternalAssemblyMetadata GetAssemblyMetadataFromAnyPossibleKey(string localeId, string name)
        {
            string key = (localeId == null || localeId == "") ? name : (localeId + ":" + name);
            if (!this.assemblyCache.ContainsKey(key))
            {

                Dictionary<string, object> response = this.wax.AwaitSendRequest("assembly", new Dictionary<string, object>() {
                    { "command", "GetAssemblyMetadataFromAnyPossibleKey" },
                    { "locale", localeId },
                    { "name", name },
                    { "localDeps", this.localDeps },
                    { "projectDir", this.projectDirectory },
                    { "includeSource", true },
                });
                if (!(bool)response["found"])
                {
                    this.assemblyCache[key] = null;
                }
                else
                {
                    Dictionary<string, string> nameByLocale = DictionaryUtil.FlattenedDictionaryToDictionary((string[])response["nameByLocale"]);
                    Dictionary<string, string> sourceCode = DictionaryUtil.FlattenedDictionaryToDictionary((string[])response["sourceCode"]);

                    ExternalAssemblyMetadata md = new ExternalAssemblyMetadata()
                    {
                        ID = (string)response["id"],
                        IsUserDefined = false,
                        InternalLocale = Locale.Get((string)response["internalLocale"]),
                        SupportedLocales = new HashSet<Locale>(((string[])response["supportedLocales"]).Select(id => Locale.Get(id))),
                        NameByLocale = nameByLocale,
                        OnlyImportableFrom = new HashSet<string>((string[])response["onlyImportableFrom"]),
                        SourceCode = sourceCode,
                    };
                    md.CanonicalKey = md.InternalLocale.ID + ":" + md.ID;
                    this.assemblyCache[key] = md;
                    foreach (Locale applicableLocale in md.SupportedLocales)
                    {
                        this.assemblyCache[applicableLocale.ID + ":" + nameByLocale[applicableLocale.ID]] = md;
                    }
                }
            }
            return this.assemblyCache[key];
        }

        internal LocalizedAssemblyView GetCoreLibrary(ParserContext parser)
        {
            ExternalAssemblyMetadata coreLib = this.GetAssemblyMetadataFromAnyPossibleKey("en", "Core");
            string name = coreLib.GetName(parser.CurrentLocale);
            return this.GetOrImportAssembly(parser, null, name);
        }

        internal LocalizedAssemblyView GetOrImportAssembly(ParserContext parser, Token throwToken, string fullImportNameWithDots)
        {
            LocalizedAssemblyView asmView = this.GetOrImportAssemblyImpl(parser, throwToken, fullImportNameWithDots);
            if (asmView != null && asmView.Scope != parser.CurrentScope)
            {
                parser.CurrentScope.AddDependency(throwToken, asmView);
            }
            return asmView;
        }

        private LocalizedAssemblyView GetOrImportAssemblyImpl(ParserContext parser, Token throwToken, string fullImportNameWithDots)
        {
            // TODO: allow importing from a user-specified locale
            Locale fromLocale = parser.CurrentLocale;
            string name = fullImportNameWithDots.Contains('.') ? fullImportNameWithDots.Split('.')[0] : fullImportNameWithDots;

            string secondAttemptedKey = name;
            ExternalAssemblyMetadata assemblyMetadata = this.GetAssemblyMetadataFromAnyPossibleKey(fromLocale.ID, name);
            Locale effectiveLocale = fromLocale;

            if (assemblyMetadata == null)
            {
                assemblyMetadata = this.GetAssemblyMetadataFromAnyPossibleKey(null, name);
                if (assemblyMetadata != null &&
                    assemblyMetadata.SupportedLocales.Contains(fromLocale) &&
                    assemblyMetadata.InternalLocale != fromLocale)
                {
                    // Coincidental cross-language collision.
                    return null;
                }

                if (assemblyMetadata == null)
                {
                    // Simply no matches at all.
                    return null;
                }

                effectiveLocale = assemblyMetadata.InternalLocale;
            }

            // Are there any restrictions on importing that library from this location?
            if (!assemblyMetadata.IsAllowedImport(parser.CurrentLibrary))
            {
                throw new ParserException(throwToken, "This library cannot be imported from here.");
            }

            // Ensure all secondary lookups for each locale is instantiated to make the upcoming code more readable.
            if (!this.importedAssembliesByLocalizedName.ContainsKey(effectiveLocale)) this.importedAssembliesByLocalizedName[effectiveLocale] = new Dictionary<string, LocalizedAssemblyView>();
            if (!this.importedAssembliesByLocalizedName.ContainsKey(assemblyMetadata.InternalLocale)) this.importedAssembliesByLocalizedName[assemblyMetadata.InternalLocale] = new Dictionary<string, LocalizedAssemblyView>();

            // Check to see if this library has been imported before.
            if (this.importedAssembliesById.ContainsKey(assemblyMetadata.ID))
            {
                // Is it imported by the same locale?
                if (this.importedAssembliesByLocalizedName[effectiveLocale].ContainsKey(name))
                {
                    // Then just return the previous instance as-is.
                    return this.importedAssembliesByLocalizedName[effectiveLocale][name];
                }

                // Wrap the previous instance in the new locale.
                LocalizedAssemblyView output = new LocalizedAssemblyView(effectiveLocale, this.importedAssembliesById[assemblyMetadata.ID]);
                this.importedAssembliesByLocalizedName[effectiveLocale][output.Name] = output;
                return output;
            }

            Dictionary<string, string> sourceCode = assemblyMetadata.SourceCode;

            string arbitraryFilename = sourceCode.Keys.Where(t => t.Contains('.')).Select(t => t.ToLowerInvariant()).FirstOrDefault();
            Build.ProgrammingLanguage programmingLanguage = arbitraryFilename != null && arbitraryFilename.EndsWith(".acr")
                ? Build.ProgrammingLanguage.ACRYLIC
                : Build.ProgrammingLanguage.CRAYON;

            // If the assembly exists but hasn't been imported before, instantiate it and
            // add it to all the lookups. This needs to happen before parsing the embedded
            // code to prevent infinite recursion.
            CompilationScope compilationScope = new CompilationScope(assemblyMetadata, assemblyMetadata.InternalLocale, programmingLanguage);
            this.assembliesAlreadyImportedIndexByKey[assemblyMetadata.CanonicalKey] = this.ImportedAssemblyScopes.Count;
            this.ImportedAssemblyScopes.Add(compilationScope);
            this.importedAssembliesById[assemblyMetadata.ID] = compilationScope;
            LocalizedAssemblyView localizedView = new LocalizedAssemblyView(effectiveLocale, compilationScope);
            this.importedAssembliesByLocalizedName[effectiveLocale][name] = localizedView;

            // Parse the assembly.
            parser.PushScope(compilationScope);
            foreach (string file in sourceCode.Keys.OrderBy(s => s.ToLowerInvariant()))
            {
                string fakeName = "[" + file + "]";
                string code = sourceCode[file];
                parser.ParseFile(fakeName, code);
            }
            parser.PopScope();

            return localizedView;
        }
    }
}
