using Common;
using Localization;
using System.Collections.Generic;
using System.Linq;

namespace Parser
{
    public class LibraryMetadata
    {
        public override string ToString()
        {
            return "LibraryMetadata: " + this.ID + " (" + this.Version + ")";
        }

        public string Directory { get; private set; }
        public string ID { get; private set; }
        public string Version { get { return "v1"; } } // TODO: versions
        public JsonLookup Manifest { get; private set; }
        public Locale InternalLocale { get; private set; }
        public string CanonicalKey { get; private set; }
        public HashSet<Locale> SupportedLocales { get; private set; }
        public bool IsImportRestricted { get { return this.OnlyImportableFrom.Count > 0; } }
        public HashSet<string> OnlyImportableFrom { get; private set; }

        // Null until library is imported and parsed.
        public LibraryCompilationScope LibraryScope { get; set; }

        public LibraryMetadata(string directory, string id)
        {
            this.Directory = directory;
            this.ID = id;

            string manifestText = FileUtil.ReadFileText(FileUtil.JoinPath(directory, "manifest.json"));
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
            this.CniFunctions = new Dictionary<string, int>();
            foreach (IDictionary<string, object> cniEntry in this.Manifest.GetAsList("cni").OfType<IDictionary<string, object>>())
            {
                if (cniEntry.ContainsKey("name") && cniEntry.ContainsKey("argc"))
                {
                    string name = cniEntry["name"].ToString();
                    int argc = (int)cniEntry["argc"];
                    this.CniFunctions[name] = argc;
                }
            }
        }

        public Dictionary<string, int> CniFunctions { get; private set; }

        private Dictionary<string, string> nameByLocale = new Dictionary<string, string>();
        public string GetName(Locale locale)
        {
            if (!nameByLocale.ContainsKey(locale.ID))
            {
                nameByLocale[locale.ID] = this.Manifest.GetAsString("localization.names." + locale.ID, this.ID);
            }
            return nameByLocale[locale.ID];
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
            string path = FileUtil.JoinPath(this.Directory, "function_registry.pst");
            if (!FileUtil.FileExists(path))
            {
                return null;
            }

            return FileUtil.ReadFileText(path);
        }

        private Dictionary<string, string> structFiles = null;

        public Dictionary<string, string> GetStructFilesCode()
        {
            if (this.structFiles == null)
            {
                this.structFiles = new Dictionary<string, string>();
                string structFilesDir = FileUtil.JoinPath(this.Directory, "structs");
                if (FileUtil.DirectoryExists(structFilesDir))
                {
                    foreach (string name in FileUtil.DirectoryListFileNames(structFilesDir))
                    {
                        this.structFiles[name] = this.ReadFile(false, FileUtil.JoinPath("structs", name), false);
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
                string supplementalFilesDir = FileUtil.JoinPath(this.Directory, "supplemental");
                if (FileUtil.DirectoryExists(supplementalFilesDir))
                {
                    foreach (string name in FileUtil.DirectoryListFileNames(supplementalFilesDir))
                    {
                        if (name.EndsWith(".pst"))
                        {
                            string key = name.Substring(0, name.Length - ".pst".Length);
                            this.supplementalFiles[key] = this.ReadFile(false, FileUtil.JoinPath("supplemental", name), false);
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
                    bool hasPastelDirectories =
                        FileUtil.DirectoryExists(FileUtil.JoinPath(this.Directory, "translate")) ||
                        FileUtil.DirectoryExists(FileUtil.JoinPath(this.Directory, "supplemental"));
                    isMoreThanJustEmbedCode = hasPastelDirectories ? 1 : 0;
                }
                return isMoreThanJustEmbedCode == 1;
            }
        }

        public byte[] ReadFileBytes(string pathRelativeToLibraryRoot)
        {
            string fullPath = FileUtil.JoinPath(this.Directory, pathRelativeToLibraryRoot);
            if (FileUtil.FileExists(fullPath))
            {
                return FileUtil.ReadFileBytes(fullPath);
            }
            throw new ParserException("The '" + this.ID + "' library does not contain the resource '" + pathRelativeToLibraryRoot + "'");
        }

        public string ReadFile(bool keepPercents, string pathRelativeToLibraryRoot, bool failSilently)
        {
            string fullPath = FileUtil.JoinPath(this.Directory, pathRelativeToLibraryRoot);
            if (FileUtil.FileExists(fullPath))
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
            string methodTranslations = this.ReadFile(false, FileUtil.JoinPath("methods", platformName + ".txt"), true);
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
