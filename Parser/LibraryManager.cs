using Build;
using Common;
using Localization;
using System.Collections.Generic;

namespace Parser
{
    public class LibraryManager
    {
        private BuildContext buildContext;
        private LibraryFinder libraryFinder;
        public LibraryFunctionTracker LibraryFunctionTracker { get; private set; }

        private Dictionary<string, LibraryCompilationScope> importedLibraryScopes = new Dictionary<string, LibraryCompilationScope>();
        private Dictionary<string, LibraryCompilationScope> librariesByKey = new Dictionary<string, LibraryCompilationScope>();

        public List<LibraryCompilationScope> ImportedLibraries { get; private set; }
        // The index + 1 is the reference ID
        private readonly Dictionary<string, int> librariesAlreadyImportedIndexByKey = new Dictionary<string, int>();

        public LibraryManager(BuildContext buildContext)
        {
            this.buildContext = buildContext;
            this.libraryFinder = new LibraryFinder(buildContext.CrayonPath, buildContext.ProjectDirectory);
            this.LibraryFunctionTracker = new LibraryFunctionTracker();
            this.ImportedLibraries = new List<LibraryCompilationScope>();
        }

        public bool IsValidLibraryName(ParserContext parser, string name)
        {
            return this.libraryFinder.GetLibraryMetadataFromAnyPossibleKey(parser.CurrentLocale.ID + ":" + name) != null;
        }

        public LibraryCompilationScope GetLibraryFromName(string name)
        {
            LibraryCompilationScope libraryScope = this.GetLibraryFromKey(name.ToLower());
            if (libraryScope == null) return null;
            return name == libraryScope.Library.ID ? libraryScope : null;
        }

        public LibraryCompilationScope GetLibraryFromKey(string key)
        {
            LibraryCompilationScope output;
            return this.librariesByKey.TryGetValue(key, out output) ? output : null;
        }

        private LibraryCompilationScope coreLibraryScope = null;
        public LibraryCompilationScope GetCoreLibrary(ParserContext parser)
        {
            if (this.coreLibraryScope == null)
            {
                this.coreLibraryScope = this.GetLibraryFromKey("en:Core"); // canonical key will work even if english locale not used.
                if (this.coreLibraryScope == null)
                {
                    LibraryMetadata coreLibMetadata = this.libraryFinder.GetLibraryMetadataFromAnyPossibleKey("en:Core");

                    TODO.GetCoreNameFromMetadataWithLocale();
                    string coreNameInLocale = coreLibMetadata.ID;

                    this.coreLibraryScope = this.ImportLibrary(parser, null, coreNameInLocale);
                }
            }
            return this.coreLibraryScope;
        }

        public Dictionary<string, string> GetEmbeddedCode(string libraryName)
        {
            return this.importedLibraryScopes[libraryName].Library.GetEmbeddedCode();
        }

        public int GetLibraryReferenceIdFromKey(string key)
        {
            return this.librariesAlreadyImportedIndexByKey[key] + 1;
        }
        
        public LibraryCompilationScope ImportLibrary(ParserContext parser, Token throwToken, string name)
        {
            name = name.Split('.')[0];
            string key = parser.CurrentLocale.ID + ":" + name;
            LibraryMetadata libraryMetadata = this.libraryFinder.GetLibraryMetadataFromAnyPossibleKey(key);
            if (libraryMetadata == null)
            {
                // check for default locale
                libraryMetadata = this.libraryFinder.GetLibraryMetadataFromAnyPossibleKey(name);
                if (libraryMetadata == null)
                {
                    // No library found. Could just be a local namespace import.
                    // If this is a bogus import, it'll throw in the Resolver.
                    return null;
                }

                if (libraryMetadata.SupportedLocales.Contains(parser.CurrentLocale))
                {
                    // If you import something by its default name from a supported locale, then it doesn't count.
                    // Don't throw an error. A user should be able to define a namespace that happens to have the
                    // same name as a library in some locale they aren't using.
                    return null;
                }
            }

            LibraryCompilationScope libraryScope = this.librariesAlreadyImportedIndexByKey.ContainsKey(libraryMetadata.CanonicalKey)
                ? this.ImportedLibraries[librariesAlreadyImportedIndexByKey[libraryMetadata.CanonicalKey]]
                : null;

            if (libraryScope == null)
            {
                string platformName = parser.BuildContext.Platform;
                libraryScope = new LibraryCompilationScope(parser.BuildContext, libraryMetadata);
                libraryMetadata.AddLocaleAccess(parser.CurrentLocale);

                this.librariesAlreadyImportedIndexByKey[libraryMetadata.CanonicalKey] = this.ImportedLibraries.Count;
                this.ImportedLibraries.Add(libraryScope);

                this.importedLibraryScopes[name] = libraryScope;
                this.librariesByKey[name.ToLowerInvariant()] = libraryScope;

                parser.PushScope(libraryScope);
                Dictionary<string, string> embeddedCode = libraryMetadata.GetEmbeddedCode();
                foreach (string embeddedFile in embeddedCode.Keys)
                {
                    string fakeName = "[" + embeddedFile + "]";
                    string code = embeddedCode[embeddedFile];
                    parser.ParseInterpretedCode(fakeName, code);
                }
                parser.PopScope();
            }

            // Even if already imported, still must check to see if this import is allowed here.
            if (!libraryScope.Library.IsAllowedImport(parser.CurrentLibrary))
            {
                throw new ParserException(throwToken, "This library cannot be imported from here.");
            }

            return libraryScope;
        }
    }
}
