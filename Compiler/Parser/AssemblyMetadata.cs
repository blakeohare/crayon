using Common;
using Localization;
using System.Collections.Generic;
using System.Linq;

namespace Parser
{
    public class AssemblyMetadata
    {
        public override string ToString()
        {
            return "AssemblyMetadata: " + this.ID + " (" + this.Version + ")";
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
        public bool IsUserDefined { get; private set; }

        // Null until library is imported and parsed.
        public CompilationScope Scope { get; set; }

        // For user defined scopes
        public AssemblyMetadata(Locale locale)
        {
            this.ID = ".";
            this.InternalLocale = locale;
            this.CanonicalKey = ".";
            this.SupportedLocales = new HashSet<Locale>() { locale };
            this.OnlyImportableFrom = new HashSet<string>();
            this.IsUserDefined = true;
            this.CniFunctions = new Dictionary<string, int>();
        }

        public AssemblyMetadata(string directory, string id)
        {
            this.Directory = directory;
            this.ID = id;
            this.IsUserDefined = false;

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
            this.CniStartupFunction = this.Manifest.GetAsString("cni-startup");
        }

        public Dictionary<string, int> CniFunctions { get; private set; }

        public string CniStartupFunction { get; private set; }

        private Dictionary<string, string> nameByLocale = new Dictionary<string, string>();
        public string GetName(Locale locale)
        {
            if (!nameByLocale.ContainsKey(locale.ID))
            {
                nameByLocale[locale.ID] = this.Manifest.GetAsString("localization.names." + locale.ID, this.ID);
            }
            return nameByLocale[locale.ID];
        }

        public bool IsAllowedImport(AssemblyMetadata fromAssembly)
        {
            if (this.IsImportRestricted)
            {
                // Non-empty list means it must be only accessible from a specific library and not top-level user code.
                if (fromAssembly.IsUserDefined) return false;

                // Is the current library on the list?
                return this.OnlyImportableFrom.Contains(fromAssembly.ID);
            }
            return true;
        }

        public Dictionary<string, string> GetSourceCode()
        {
            Dictionary<string, string> output = new Dictionary<string, string>();
            string srcDir = FileUtil.JoinPath(this.Directory, "src");
            if (!FileUtil.DirectoryExists(srcDir))
            {
                throw new System.InvalidOperationException(this.Directory + " is missing a 'src' directory");
            }
            string[] srcFiles = FileUtil.GetAllFilePathsRelativeToRoot(srcDir);
            foreach (string srcFile in srcFiles)
            {
                string code = this.ReadFile(false, "src/" + srcFile, false);
                output[this.ID + ":" + srcFile] = code;
            }
            return output;
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

        public string GetPastelCodeDirectory()
        {
            return FileUtil.JoinPath(this.Directory, "pastel", "src");
        }

        public bool HasNativeCode { get { return this.CniFunctions.Count > 0; } }

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
            string methodTranslations = this.ReadFile(false, FileUtil.JoinPath("pastel", "extensions", platformName + ".txt"), true);
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
