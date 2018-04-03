using Build;
using Localization;
using System.Collections.Generic;
using System.Linq;

namespace Parser
{
    public class LibraryManager
    {
        private BuildContext buildContext;
        private LibraryFinder libraryFinder;

        private Dictionary<string, LibraryCompilationScope> importedLibrariesById = new Dictionary<string, LibraryCompilationScope>();
        private Dictionary<Locale, Dictionary<string, LocalizedLibraryView>> importedLibrariesByLocalizedName = new Dictionary<Locale, Dictionary<string, LocalizedLibraryView>>();

        public List<LibraryCompilationScope> ImportedLibraries { get; private set; }
        // The index + 1 is the reference ID
        private readonly Dictionary<string, int> librariesAlreadyImportedIndexByKey = new Dictionary<string, int>();

        public LibraryManager(BuildContext buildContext)
        {
            this.buildContext = buildContext;
            this.libraryFinder = new LibraryFinder(buildContext.CrayonPath, buildContext.ProjectDirectory);
            this.ImportedLibraries = new List<LibraryCompilationScope>();
        }

        public bool IsValidLibraryNameFromLocale(Locale locale, string name)
        {
            // TODO: This is only checking if the library is available in this locale.
            // It needs to check for all libraries.
            // This is currently only used in slim circumstances.
            Common.TODO.IsValidLibraryNameIsWrong();

            return this.libraryFinder.GetLibraryMetadataFromAnyPossibleKey(locale + ":" + name) != null;
        }

        public int GetLibraryReferenceIdFromKey(string key)
        {
            return this.librariesAlreadyImportedIndexByKey[key] + 1;
        }

        public LocalizedLibraryView GetLocalizedLibraryIfImported(Locale locale, string localizedName)
        {
            return
                this.importedLibrariesByLocalizedName.ContainsKey(locale) &&
                this.importedLibrariesByLocalizedName[locale].ContainsKey(localizedName)
                    ? this.importedLibrariesByLocalizedName[locale][localizedName]
                    : null;
        }

        public LibraryCompilationScope GetLibraryIfImported(string libraryId)
        {
            return this.importedLibrariesById.ContainsKey(libraryId) ? this.importedLibrariesById[libraryId] : null;
        }

        public LocalizedLibraryView GetCoreLibrary(ParserContext parser)
        {
            string anyValidCoreLibId = "en:Core";
            LibraryMetadata coreLib = this.libraryFinder.GetLibraryMetadataFromAnyPossibleKey(anyValidCoreLibId);
            string name = coreLib.GetName(parser.CurrentLocale);
            return this.GetOrImportLibrary(parser, null, name);
        }

        public LocalizedLibraryView GetOrImportLibrary(ParserContext parser, Token throwToken, string fullImportNameWithDots)
        {
            LocalizedLibraryView libView = this.GetOrImportLibraryImpl(parser, throwToken, fullImportNameWithDots);
            if (libView != null && libView.LibraryScope != parser.CurrentScope)
            {
                parser.CurrentScope.AddDependency(throwToken, libView);
            }
            return libView;
        }

        private LocalizedLibraryView GetOrImportLibraryImpl(ParserContext parser, Token throwToken, string fullImportNameWithDots) {
            // TODO: allow importing from a user-specified locale
            Locale fromLocale = parser.CurrentLocale;
            string name = fullImportNameWithDots.Contains('.') ? fullImportNameWithDots.Split('.')[0] : fullImportNameWithDots;

            string secondAttemptedKey = name;
            LibraryMetadata libraryMetadata = this.libraryFinder.GetLibraryMetadataFromAnyPossibleKey(fromLocale.ID + ":" + name);
            Locale effectiveLocale = fromLocale;

            if (libraryMetadata == null)
            {
                libraryMetadata = this.libraryFinder.GetLibraryMetadataFromAnyPossibleKey(name);
                if (libraryMetadata != null &&
                    libraryMetadata.SupportedLocales.Contains(fromLocale) &&
                    libraryMetadata.InternalLocale != fromLocale) {
                    // Coincidental cross-language collision.
                    return null;
                }

                if (libraryMetadata == null)
                {
                    // Simply no matches at all.
                    return null;
                }

                effectiveLocale = libraryMetadata.InternalLocale;
            }

            // Are there any restrictions on importing that library from this location?
            if (!libraryMetadata.IsAllowedImport(parser.CurrentLibrary))
            {
                throw new ParserException(throwToken, "This library cannot be imported from here.");
            }

            // Ensure all secondary lookups for each locale is instantiated to make the upcoming code more readable.
            if (!this.importedLibrariesByLocalizedName.ContainsKey(effectiveLocale)) this.importedLibrariesByLocalizedName[effectiveLocale] = new Dictionary<string, LocalizedLibraryView>();
            if (!this.importedLibrariesByLocalizedName.ContainsKey(libraryMetadata.InternalLocale)) this.importedLibrariesByLocalizedName[libraryMetadata.InternalLocale] = new Dictionary<string, LocalizedLibraryView>();

            // Check to see if this library has been imported before.
            if (this.importedLibrariesById.ContainsKey(libraryMetadata.ID))
            {
                // Is it imported by the same locale?
                if (this.importedLibrariesByLocalizedName[effectiveLocale].ContainsKey(name))
                {
                    // Then just return the previous instance as-is.
                    return this.importedLibrariesByLocalizedName[effectiveLocale][name];
                }

                // Wrap the previous instance in the new locale.
                LocalizedLibraryView output = new LocalizedLibraryView(effectiveLocale,  this.importedLibrariesById[libraryMetadata.ID]);
                this.importedLibrariesByLocalizedName[effectiveLocale][output.Name] = output;
                return output;
            }

            // If the library exists but hasn't been imported before, instantiate it and
            // add it to all the lookups. This needs to happen before parsing the embedded
            // code to prevent infinite recursion.
            LibraryCompilationScope libraryScope = new LibraryCompilationScope(parser.BuildContext, libraryMetadata);
            this.librariesAlreadyImportedIndexByKey[libraryMetadata.CanonicalKey] = this.ImportedLibraries.Count;
            this.ImportedLibraries.Add(libraryScope);
            this.importedLibrariesById[libraryMetadata.ID] = libraryScope;
            LocalizedLibraryView localizedView = new LocalizedLibraryView(effectiveLocale, libraryScope);
            this.importedLibrariesByLocalizedName[effectiveLocale][name] = localizedView;

            // Parse the library.
            parser.PushScope(libraryScope);
            Dictionary<string, string> embeddedCode = libraryMetadata.GetEmbeddedCode();
            foreach (string embeddedFile in embeddedCode.Keys.OrderBy(s => s.ToLower()))
            {
                string fakeName = "[" + embeddedFile + "]";
                string code = embeddedCode[embeddedFile];
                parser.ParseInterpretedCode(fakeName, code);
            }
            parser.PopScope();

            return localizedView;
        }
    }
}
