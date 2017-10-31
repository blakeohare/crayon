using Common;
using Localization;
using System.Collections.Generic;
using System.Linq;

namespace Parser
{
    public class LibraryMetadata
    {
        public string Directory { get; private set; }
        public string ID { get; private set; }
        public string Version { get { return "v1"; } } // TODO: versions
        public JsonLookup Manifest { get; private set; }
        public Locale InternalLocale { get; private set; }
        public string CanonicalKey { get; private set; }
        public HashSet<Locale> SupportedLocales { get; private set; }
        public bool IsImportRestricted { get { return this.OnlyImportableFrom.Count > 0; } }
        public HashSet<string> OnlyImportableFrom { get; private set; }

        public LibraryMetadata(string directory, string id)
        {
            this.Directory = directory;
            this.ID = id;

            string manifestText = System.IO.File.ReadAllText(System.IO.Path.Combine(directory, "manifest.json"));
            try
            {
                this.Manifest = new JsonLookup(new JsonParser(manifestText)
                    .AddOption(JsonOption.ALLOW_TRAILING_COMMA)
                    .AddOption(JsonOption.ALLOW_COMMENTS)
                    .ParseAsDictionary());
            }
            catch (JsonParser.JsonParserException jpe)
            {
                throw new System.InvalidOperationException("Syntax error while parsing the library manifest for '" + id + "'.", jpe);
            }
            
            this.InternalLocale = Locale.Get(this.Manifest.GetAsString("localization.default", "en"));
            this.CanonicalKey = this.InternalLocale.ID + ":" + this.ID;
            this.SupportedLocales = new HashSet<Locale>(this.Manifest.GetAsLookup("localization.names").Keys.Select(localeName => Locale.Get(localeName)));
            this.SupportedLocales.Add(this.InternalLocale);
            this.OnlyImportableFrom = new HashSet<string>(this.Manifest.GetAsList("onlyAllowImportFrom").Cast<string>());
        }

        private Dictionary<string, string> nameByLocale = new Dictionary<string, string>();
        public string GetName(Locale locale)
        {
            if (!nameByLocale.ContainsKey(locale.ID))
            {
                nameByLocale[locale.ID] = this.Manifest.GetAsString("localization.names." + locale.ID, this.ID);
            }
            return nameByLocale[locale.ID];
        }
        
        private List<LibraryMetadata> libraryDependencies = new List<LibraryMetadata>();
        private HashSet<Locale> localesAccessed = new HashSet<Locale>();
        private HashSet<LibraryMetadata> libraryDependencyDuplicateCheck = new HashSet<LibraryMetadata>();
        private LibraryMetadata[] libraryDependenciesArray = null;
        public void AddLibraryDependency(LibraryMetadata library)
        {
            if (!libraryDependencyDuplicateCheck.Contains(library) && library != this)
            {
                this.libraryDependencies.Add(library);
                this.libraryDependenciesArray = null;
            }
        }
        
        public LibraryMetadata[] LibraryDependencies
        {
            get
            {
                if (this.libraryDependenciesArray == null)
                {
                    this.libraryDependenciesArray = this.libraryDependencies.ToArray();
                }
                return this.libraryDependenciesArray;
            }
        }

        public bool IsAllowedImport(LibraryMetadata currentLibrary)
        {
            if (this.IsImportRestricted)
            {
                // Non-empty list means it must be only accessible from a specific library and not top-level user code.
                if (currentLibrary == null) return false;
                
                // Is the current library on the list?
                return this.OnlyImportableFrom.Contains(currentLibrary.ID);
            }
            return true;
        }

        public Dictionary<string, string> GetEmbeddedCode()
        {
            Dictionary<string, string> output = new Dictionary<string, string>() {
                { this.ID, this.ReadFile(false, "embed.cry", true) }
            };
            string embedDir = FileUtil.JoinPath(this.Directory, "embed");
            if (FileUtil.DirectoryExists(embedDir))
            {
                string[] additionalFiles = FileUtil.GetAllFilePathsRelativeToRoot(embedDir);
                foreach (string additionalFile in additionalFiles)
                {
                    string embedCode = this.ReadFile(false, "embed/" + additionalFile, false);
                    output[this.ID + ":" + additionalFile] = embedCode;
                }
            }
            return output;
        }

        public string GetRegistryCode()
        {
            string path = System.IO.Path.Combine(this.Directory, "function_registry.pst");
            if (!System.IO.File.Exists(path))
            {
                return null;
            }

            return System.IO.File.ReadAllText(path);
        }

        private Dictionary<string, string> structFiles = null;

        public Dictionary<string, string> GetStructFilesCode()
        {
            if (this.structFiles == null)
            {
                this.structFiles = new Dictionary<string, string>();
                string structFilesDir = System.IO.Path.Combine(this.Directory, "structs");
                if (System.IO.Directory.Exists(structFilesDir))
                {
                    foreach (string filepath in System.IO.Directory.GetFiles(structFilesDir))
                    {
                        string name = System.IO.Path.GetFileName(filepath);
                        this.structFiles[name] = this.ReadFile(false, System.IO.Path.Combine("structs", name), false);
                    }
                }
            }
            return this.structFiles;
        }

        private Dictionary<string, string> supplementalFiles = null;

        public Dictionary<string, string> GetSupplementalTranslatedCode()
        {
            if (this.supplementalFiles == null)
            {
                this.supplementalFiles = new Dictionary<string, string>();
                string supplementalFilesDir = System.IO.Path.Combine(this.Directory, "supplemental");
                if (System.IO.Directory.Exists(supplementalFilesDir))
                {
                    foreach (string filepath in System.IO.Directory.GetFiles(supplementalFilesDir))
                    {
                        string name = System.IO.Path.GetFileName(filepath);
                        if (name.EndsWith(".pst"))
                        {
                            string key = name.Substring(0, name.Length - ".pst".Length);
                            this.supplementalFiles[key] = this.ReadFile(false, System.IO.Path.Combine("supplemental", name), false);
                        }
                    }
                }
            }
            return this.supplementalFiles;
        }

        private int isMoreThanJustEmbedCode = -1;
        public bool IsMoreThanJustEmbedCode
        {
            get
            {
                if (isMoreThanJustEmbedCode == -1)
                {
                    isMoreThanJustEmbedCode = FileUtil.DirectoryExists(FileUtil.JoinPath(this.Directory, "translate")) ? 1 : 0;
                }
                return isMoreThanJustEmbedCode == 1;
            }
        }

        public byte[] ReadFileBytes(string pathRelativeToLibraryRoot)
        {
            string fullPath = FileUtil.JoinPath(this.Directory, pathRelativeToLibraryRoot);
            if (System.IO.File.Exists(fullPath))
            {
                return FileUtil.ReadFileBytes(fullPath);
            }
            throw new ParserException(null, "The '" + this.ID + "' library does not contain the resource '" + pathRelativeToLibraryRoot + "'");
        }

        public string ReadFile(bool keepPercents, string pathRelativeToLibraryRoot, bool failSilently)
        {
            string fullPath = FileUtil.JoinPath(this.Directory, pathRelativeToLibraryRoot);
            if (System.IO.File.Exists(fullPath))
            {
                return FileUtil.ReadFileText(fullPath);
            }

            if (failSilently)
            {
                return "";
            }

            throw new System.InvalidOperationException("Missing resource in library '" + this.ID + "': '" + pathRelativeToLibraryRoot + "'");
        }

        // This ONLY gets the translations that are specific only for this platform and does not do any inheritance chain walking.
        public Dictionary<string, string> GetMethodTranslations(string platformName)
        {
            string methodTranslations = this.ReadFile(false, System.IO.Path.Combine("methods", platformName + ".txt"), true);
            Dictionary<string, string> translationsLookup = new Dictionary<string, string>();
            if (methodTranslations != null)
            {
                foreach (string line in methodTranslations.Split('\n'))
                {
                    string[] parts = line.Trim().Split(':');
                    if (parts.Length > 1)
                    {
                        string key = parts[0];
                        string value = parts[1];
                        for (int i = 2; i < parts.Length; ++i)
                        {
                            value += ":" + parts[i];
                        }
                        translationsLookup[key.Trim()] = value.Trim();
                    }
                }
            }
            return translationsLookup;
        }
    }
}
