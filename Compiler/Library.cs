using Common;
using Localization;
using System.Collections.Generic;
using System.Linq;

namespace Crayon
{
    // Library is only instantiable in the context of a specific platform, which is not ideal, but not causing any problems at the moment.
    public class Library
    {
        public LibraryMetadata Metadata { get; private set; }
        public CompilationScope Scope { get; set; }
        private string platformName;

        public string Name { get { return this.Metadata.Name; } }
        public string Version { get { return "v1"; } } // TODO: versions
        public string RootDirectory { get { return this.Metadata.Directory; } }
        public string CanonicalKey { get { return this.Metadata.CanonicalKey; } }

        public Dictionary<string, object> CompileTimeConstants { get; set; }

        internal LibraryResourceDatabase Resources { get; private set; }

        public Library CloneWithNewPlatform(Platform.AbstractPlatform platform)
        {
            return new Library(this.Metadata, platform);
        }

        private Dictionary<string, object> LoadFlagsForPlatform(Platform.AbstractPlatform platform)
        {
            Dictionary<string, object> flags = new Dictionary<string, object>();
            List<string> platformChain = new List<string>() { "default" };
            if (platform != null)
            {
                platformChain.AddRange(platform.InheritanceChain.Reverse());
            }
            foreach (string platformId in platformChain)
            {
                Dictionary<string, object> mergeFlagsWith = this.LoadFlagsFromFile(platformId);
                flags = Util.MergeDictionaries(flags, mergeFlagsWith);
            }
            return flags;
        }

        private Dictionary<string, object> LoadFlagsFromFile(string platformId)
        {
            Dictionary<string, object> output = new Dictionary<string, object>();
            string path = FileUtil.JoinAndCanonicalizePath(this.RootDirectory, "flags", platformId + ".txt");
            if (FileUtil.FileExists(path))
            {
                foreach (string line in FileUtil.ReadFileText(path).Split('\n'))
                {
                    string fline = line.Trim();
                    if (fline.Length > 0 && fline[0] != '#')
                    {
                        string[] parts = fline.Split(new char[] { ':' }, 2);
                        string key = parts[0].Trim();
                        string value = parts[1].Trim();
                        if (value == "false" || value == "true")
                        {
                            output[key] = value == "true";
                        }
                        else
                        {
                            output[key] = value;
                        }
                    }
                }
            }
            return output;
        }

        public Library(LibraryMetadata metadata, Platform.AbstractPlatform nullablePlatform)
        {
            TODO.LibrariesNeedVersionNumber();

            this.Metadata = metadata;
            this.platformName = nullablePlatform == null ? null : nullablePlatform.Name;

            this.Resources = new LibraryResourceDatabase(this, nullablePlatform);

            this.CompileTimeConstants = this.LoadFlagsForPlatform(nullablePlatform);

            this.filepathsByFunctionName = new Dictionary<string, string>();
            // Build a lookup dictionary of all file names that are simple function names e.g. "foo.cry"
            // Then go through and look up all the file names that contain . prefixes with the platform name and
            // overwrite the lookup value for that entry with the more specific path.
            // myFunction.cry
            // android.myFunction.cry
            // on Python, myFunction will be included for lib_foo_myFunction(), but on Android, android.myFunction.cry will be included instead.

            string[] files = new string[0];
            if (FileUtil.DirectoryExists(this.RootDirectory + "/translate"))
            {
                files = System.IO.Directory.GetFiles(System.IO.Path.Combine(this.RootDirectory, "translate"));
            }
            Dictionary<string, string> moreSpecificFiles = new Dictionary<string, string>();
            foreach (string fileWithDirectory in files)
            {
                string file = System.IO.Path.GetFileName(fileWithDirectory);
                if (file.EndsWith(".pst"))
                {
                    string functionName = file.Substring(0, file.Length - ".pst".Length);
                    if (functionName.Contains('.'))
                    {
                        // Add this file to the more specific lookup, but only if it contains the current platform.
                        if (functionName.StartsWith(platformName + ".") ||
                            functionName.Contains("." + platformName + "."))
                        {
                            string[] parts = functionName.Split('.');
                            moreSpecificFiles[parts[parts.Length - 1]] = file;
                        }
                        else
                        {
                            // just let it get filtered away.
                        }
                    }
                    else
                    {
                        this.filepathsByFunctionName[functionName] = file;
                    }
                }
            }

            foreach (string functionName in moreSpecificFiles.Keys)
            {
                this.filepathsByFunctionName[functionName] = moreSpecificFiles[functionName];
            }
        }

        private List<Library> libraryDependencies = new List<Library>();
        private HashSet<Locale> localesAccessed = new HashSet<Locale>();
        private HashSet<Library> libraryDependencyDuplicateCheck = new HashSet<Library>();
        private Library[] libraryDependenciesArray = null;
        public void AddLibraryDependency(Library library)
        {
            if (!libraryDependencyDuplicateCheck.Contains(library) && library != this)
            {
                this.libraryDependencies.Add(library);
                this.libraryDependenciesArray = null;
            }
            library.AddLocaleAccess(this.Metadata.InternalLocale);
        }

        public void AddLocaleAccess(Locale locale)
        {
            this.localesAccessed.Add(locale);
        }

        public Library[] LibraryDependencies
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

        public bool IsMoreThanJustEmbedCode
        {
            get { return this.filepathsByFunctionName.Count > 0; }
        }

        public bool IsAllowedImport(Library currentLibrary)
        {
            if (this.Metadata.IsImportRestricted)
            {
                // Non-empty list means it must be only accessible from a specific library and not top-level user code.
                if (currentLibrary == null) return false;


                // Is the current library on the list?
                return this.Metadata.OnlyImportableFrom.Contains(currentLibrary.Name);
            }
            return true;
        }

        private Dictionary<string, string> filepathsByFunctionName;

        public Dictionary<string, string> GetEmbeddedCode()
        {
            Dictionary<string, string> output = new Dictionary<string, string>() {
                { this.Name, this.ReadFile(false, "embed.cry", true) }
            };
            string embedDir = FileUtil.JoinPath(this.RootDirectory, "embed");
            if (FileUtil.DirectoryExists(embedDir))
            {
                string[] additionalFiles = FileUtil.GetAllFilePathsRelativeToRoot(embedDir);
                foreach (string additionalFile in additionalFiles)
                {
                    string embedCode = this.ReadFile(false, "embed/" + additionalFile, false);
                    output[this.Name + ":" + additionalFile] = embedCode;
                }
            }
            return output;
        }

        public string GetRegistryCode()
        {
            string path = System.IO.Path.Combine(this.RootDirectory, "function_registry.pst");
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
                string structFilesDir = System.IO.Path.Combine(this.RootDirectory, "structs");
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

        public Dictionary<string, string> GetSupplementalTranslatedCode(bool isPastel)
        {
            if (this.supplementalFiles == null)
            {
                this.supplementalFiles = new Dictionary<string, string>();
                string supplementalFilesDir = System.IO.Path.Combine(this.RootDirectory, "supplemental");
                if (System.IO.Directory.Exists(supplementalFilesDir))
                {
                    foreach (string filepath in System.IO.Directory.GetFiles(supplementalFilesDir))
                    {
                        string name = System.IO.Path.GetFileName(filepath);
                        if ((isPastel && name.EndsWith(".cry")) || (!isPastel && name.EndsWith(".pst")))
                        {
                            string key = name.Substring(0, name.Length - ".cry".Length);
                            this.supplementalFiles[key] = this.ReadFile(false, System.IO.Path.Combine("supplemental", name), false);
                        }
                    }
                }
            }
            return this.supplementalFiles;
        }

        public Dictionary<string, string> GetNativeCode()
        {
            Dictionary<string, string> output = new Dictionary<string, string>();
            foreach (string key in this.filepathsByFunctionName.Keys)
            {
                string filename = "translate/" + this.filepathsByFunctionName[key].Replace(".cry", ".pst");
                output[key] = this.ReadFile(false, filename, false);
            }

            return output;
        }

        Dictionary<string, string> translations = null;

        private Dictionary<string, string> GetMethodTranslations(string platformName)
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

        public string TranslateNativeInvocation(object throwToken, object legacyAbstractPlatformOrCbxTranslator, string functionName, object[] args)
        {
            if (this.translations == null)
            {
                Platform.AbstractTranslator translator = (Platform.AbstractTranslator)legacyAbstractPlatformOrCbxTranslator;
                Dictionary<string, string> translationsBuilder = new Dictionary<string, string>();
                foreach (string platformName in translator.Platform.InheritanceChain.Reverse())
                {
                    Dictionary<string, string> translationsForPlatform = this.GetMethodTranslations(platformName);
                    translationsBuilder = Util.MergeDictionaries(translationsBuilder, translationsForPlatform);
                }
                this.translations = translationsBuilder;
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            string output = null;
            string lookup = "$" + functionName;
            if (this.translations.ContainsKey(lookup))
            {
                output = this.translations[lookup];
            }

            if (output != null)
            {
                for (int i = 0; i < args.Length; ++i)
                {

                    ((Platform.AbstractTranslator)legacyAbstractPlatformOrCbxTranslator).TranslateExpression(sb, (Pastel.Nodes.Expression)args[i]);
                    string argAsString = sb.ToString();
                    sb.Clear();
                    output = output.Replace("[ARG:" + (i + 1) + "]", argAsString);
                }
                return output;
            }

            // Use this to determine which function is causing the problem:
            string MISSING_FUNCTION_NAME_FOR_DEBUGGER = functionName;
            MISSING_FUNCTION_NAME_FOR_DEBUGGER.Trim(); // no compile warnings

            string msg = "The " + this.Name + " library does not support " + this.platformName + " projects.";
            if (throwToken is Token)
            {
                throw new ParserException((Token)throwToken, msg);
            }
            else
            {
                throw new Pastel.ParserException((Pastel.Token)throwToken, msg);
            }
        }

        private HashSet<string> IGNORABLE_FILES = new HashSet<string>(new string[] { ".ds_store", "thumbs.db" });
        internal string[] ListDirectory(string pathRelativeToLibraryRoot)
        {
            string fullPath = FileUtil.JoinPath(this.RootDirectory, pathRelativeToLibraryRoot);
            List<string> output = new List<string>();
            if (FileUtil.DirectoryExists(fullPath))
            {
                foreach (string file in FileUtil.DirectoryListFileNames(fullPath))
                {
                    if (!IGNORABLE_FILES.Contains(file.ToLower()))
                    {
                        output.Add(file);
                    }
                }
            }
            return output.ToArray();
        }

        public byte[] ReadFileBytes(string pathRelativeToLibraryRoot)
        {
            string fullPath = FileUtil.JoinPath(this.RootDirectory, pathRelativeToLibraryRoot);
            if (System.IO.File.Exists(fullPath))
            {
                return FileUtil.ReadFileBytes(fullPath);
            }
            throw new ParserException(null, "The '" + this.Name + "' library does not contain the resource '" + pathRelativeToLibraryRoot + "'");
        }

        public string ReadFile(bool keepPercents, string pathRelativeToLibraryRoot, bool failSilently)
        {
            string fullPath = FileUtil.JoinPath(this.RootDirectory, pathRelativeToLibraryRoot);
            if (System.IO.File.Exists(fullPath))
            {
                return FileUtil.ReadFileText(fullPath);
            }

            if (failSilently)
            {
                return "";
            }

            throw new ParserException(null, "The " + this.Name + " library does not support " + this.platformName + " projects.");
        }

        private Dictionary<string, Pastel.Nodes.PType> returnTypeInfoForNativeMethods = null;
        private Dictionary<string, Pastel.Nodes.PType[]> argumentTypeInfoForNativeMethods = null;

        private void InitTypeInfo()
        {
            this.returnTypeInfoForNativeMethods = new Dictionary<string, Pastel.Nodes.PType>();
            this.argumentTypeInfoForNativeMethods = new Dictionary<string, Pastel.Nodes.PType[]>();

            string typeInfoFile = System.IO.Path.Combine(this.RootDirectory, "native_method_type_info.txt");
            if (System.IO.File.Exists(typeInfoFile))
            {
                string typeInfo = System.IO.File.ReadAllText(typeInfoFile);
                Pastel.TokenStream tokens = new Pastel.TokenStream(Pastel.Tokenizer.Tokenize("LIB:" + this.Name + "/native_method_type_info.txt", typeInfo));

                while (tokens.HasMore)
                {
                    Pastel.Nodes.PType returnType = Pastel.Nodes.PType.Parse(tokens);
                    string functionName = GetValidNativeLibraryFunctionNameFromPastelToken(tokens.Pop());
                    tokens.PopExpected("(");
                    List<Pastel.Nodes.PType> argTypes = new List<Pastel.Nodes.PType>();
                    while (!tokens.PopIfPresent(")"))
                    {
                        if (argTypes.Count > 0) tokens.PopExpected(",");
                        argTypes.Add(Pastel.Nodes.PType.Parse(tokens));

                        // This is unused but could be later used as part of an auto-generated documentation for third-party platform implements of existing libraries.
                        string argumentName = GetValidNativeLibraryFunctionNameFromPastelToken(tokens.Pop());
                    }
                    tokens.PopExpected(";");

                    this.returnTypeInfoForNativeMethods[functionName] = returnType;
                    this.argumentTypeInfoForNativeMethods[functionName] = argTypes.ToArray();
                }
            }
        }

        private string GetValidNativeLibraryFunctionNameFromPastelToken(Pastel.Token token)
        {
            string name = token.Value;
            char c;
            bool okay = true;
            for (int i = name.Length - 1; i >= 0; --i)
            {
                c = name[i];
                if ((c >= 'a' && c <= 'z') ||
                    (c >= 'A' && c <= 'Z') ||
                    c == '_')
                {
                    // this is fine
                }
                else if (c >= '0' && c <= '9')
                {
                    if (i == 0)
                    {
                        okay = false;
                        break;
                    }
                }
                else
                {
                    okay = false;
                    break;
                }
            }
            if (!okay)
            {
                throw new Pastel.ParserException(token, "Invalid name for a native function or argument.");
            }
            return name;
        }

        public Dictionary<string, Pastel.Nodes.PType> GetReturnTypesForNativeMethods()
        {
            if (this.returnTypeInfoForNativeMethods == null) this.InitTypeInfo();

            return this.returnTypeInfoForNativeMethods;
        }

        public Dictionary<string, Pastel.Nodes.PType[]> GetArgumentTypesForNativeMethods()
        {
            if (this.argumentTypeInfoForNativeMethods == null) this.InitTypeInfo();

            return this.argumentTypeInfoForNativeMethods;
        }
    }
}
