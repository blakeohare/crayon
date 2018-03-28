using Common;
using Parser;
using System.Collections.Generic;
using System.Linq;

namespace Exporter
{
    // Library is only instantiable in the context of a specific platform, which is not ideal, but not causing any problems at the moment.
    public class LibraryExporter
    {
        public LibraryMetadata Metadata { get; private set; }
        private string platformName;

        public Dictionary<string, object> CompileTimeConstants { get; set; }

        internal LibraryResourceDatabase Resources { get; private set; }

        private static Dictionary<string, LibraryExporter> libraryCache = new Dictionary<string, LibraryExporter>();

        private static string GetLibKey(LibraryMetadata metadata, Platform.AbstractPlatform platform)
        {
            return metadata.CanonicalKey + "#" + platform.Name;
        }

        // TODO: this ought to go away and the cache needs to move to some sort of scope whose lifetime is tied to a specific compilation scope.
        public static LibraryExporter Get(LibraryMetadata metadata, Platform.AbstractPlatform platform)
        {
            string key = GetLibKey(metadata, platform);
            if (!libraryCache.ContainsKey(key))
            {
                libraryCache[key] = new LibraryExporter(metadata, platform);
            }
            return libraryCache[key];
        }

        private LibraryExporter(LibraryMetadata metadata, Platform.AbstractPlatform platform)
        {
            TODO.LibrariesNeedVersionNumber();

            this.Metadata = metadata;
            this.platformName = platform.Name;

            this.Resources = new LibraryResourceDatabase(this, platform);

            this.CompileTimeConstants = this.LoadFlagsForPlatform(platform);

            this.filepathsByFunctionName = new Dictionary<string, string>();
            // Build a lookup dictionary of all file names that are simple function names e.g. "foo.cry"
            // Then go through and look up all the file names that contain . prefixes with the platform name and
            // overwrite the lookup value for that entry with the more specific path.
            // myFunction.cry
            // android.myFunction.cry
            // on Python, myFunction will be included for lib_foo_myFunction(), but on Android, android.myFunction.cry will be included instead.

            string[] files = new string[0];
            if (FileUtil.DirectoryExists(this.Metadata.Directory + "/translate"))
            {
                files = FileUtil.DirectoryListFileNames(FileUtil.JoinPath(this.Metadata.Directory, "translate"));
            }
            Dictionary<string, string> moreSpecificFiles = new Dictionary<string, string>();
            foreach (string file in files)
            {
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
            string path = FileUtil.JoinAndCanonicalizePath(this.Metadata.Directory, "flags", platformId + ".txt");
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

        private Dictionary<string, string> filepathsByFunctionName;

        public Dictionary<string, string> GetNativeCode()
        {
            Dictionary<string, string> output = new Dictionary<string, string>();
            foreach (string key in this.filepathsByFunctionName.Keys)
            {
                string filename = "translate/" + this.filepathsByFunctionName[key].Replace(".cry", ".pst");
                output[key] = this.Metadata.ReadFile(false, filename, false);
            }

            return output;
        }

        private Dictionary<string, string> translationsLookup = null;

        public string TranslateNativeInvocation(object throwToken, Platform.AbstractTranslator translator, string functionName, object[] args)
        {
            if (translationsLookup == null)
            {
                translationsLookup = new Dictionary<string, string>();
                foreach (string inheritedPlatformName in translator.Platform.InheritanceChain.Reverse())
                {
                    Dictionary<string, string> translationsForPlatform = this.Metadata.GetMethodTranslations(inheritedPlatformName);
                    translationsLookup = Util.MergeDictionaries(translationsLookup, translationsForPlatform);
                }
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            string output = null;
            string lookup = "$" + functionName;
            if (translationsLookup.ContainsKey(lookup))
            {
                output = translationsLookup[lookup];
            }

            if (output != null)
            {
                for (int i = 0; i < args.Length; ++i)
                {

                    translator.TranslateExpression(sb, (Pastel.Nodes.Expression)args[i]);
                    string argAsString = sb.ToString();
                    sb.Clear();
                    output = output.Replace("[ARG:" + (i + 1) + "]", argAsString);
                }
                return output;
            }

            // Use this to determine which function is causing the problem:
            string MISSING_FUNCTION_NAME_FOR_DEBUGGER = functionName;
            MISSING_FUNCTION_NAME_FOR_DEBUGGER.Trim(); // no compile warnings

            string msg = "The " + this.Metadata.ID + " library does not support " + translator.Platform.Name + " projects.";
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
            string fullPath = FileUtil.JoinPath(this.Metadata.Directory, pathRelativeToLibraryRoot);
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

        private Dictionary<string, Pastel.Nodes.PType> returnTypeInfoForNativeMethods = null;
        private Dictionary<string, Pastel.Nodes.PType[]> argumentTypeInfoForNativeMethods = null;

        private void InitTypeInfo()
        {
            this.returnTypeInfoForNativeMethods = new Dictionary<string, Pastel.Nodes.PType>();
            this.argumentTypeInfoForNativeMethods = new Dictionary<string, Pastel.Nodes.PType[]>();

            string typeInfoFile = FileUtil.JoinPath(this.Metadata.Directory, "native_method_type_info.txt");
            if (FileUtil.FileExists(typeInfoFile))
            {
                string typeInfo = FileUtil.ReadFileText(typeInfoFile);
                Pastel.TokenStream tokens = new Pastel.TokenStream(Pastel.Tokenizer.Tokenize("LIB:" + this.Metadata.ID + "/native_method_type_info.txt", typeInfo));

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
