using Common;
using Parser;
using Pastel.Transpilers;
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

        private Dictionary<string, string> translationsLookup = null;

        public void ApplyExtensibleFunctionTranslationsToTranspilerContext(Platform.AbstractPlatform platform, TranspilerContext ctx)
        {
            Dictionary<string, string> translations = this.GetExtensibleFunctionTranslations(platform);
            foreach (string fnNameRaw in translations.Keys)
            {
                // TODO: remove these dollar signs from the actual library code.
                string fnName = fnNameRaw.StartsWith("$") ? fnNameRaw.Substring(1) : fnNameRaw;
                ctx.ExtensibleFunctionLookup[fnName] = translations[fnNameRaw];
            }
        }

        public Dictionary<string, string> GetExtensibleFunctionTranslations(Platform.AbstractPlatform platform)
        {
            if (this.translationsLookup == null)
            {
                this.translationsLookup = new Dictionary<string, string>();
                foreach (string inheritedPlatformName in platform.InheritanceChain.Reverse())
                {
                    Dictionary<string, string> translationsForPlatform = this.Metadata.GetMethodTranslations(inheritedPlatformName);
                    this.translationsLookup = Util.MergeDictionaries(translationsLookup, translationsForPlatform);
                }
            }
            return this.translationsLookup;
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

        private List<Pastel.ExtensibleFunction> extensibleFunctionMetadata = null;

        private void InitTypeInfo()
        {
            string typeInfoFile = FileUtil.JoinPath(this.Metadata.Directory, "native_method_type_info.txt");
            if (FileUtil.FileExists(typeInfoFile))
            {
                string typeInfo = FileUtil.ReadFileText(typeInfoFile);
                this.extensibleFunctionMetadata = Pastel.ExtensibleFunctionMetadataParser.Parse(
                    "LIB:" + this.Metadata.ID + "/native_method_type_info.txt",
                    typeInfo);
            }
            else
            {
                this.extensibleFunctionMetadata = new List<Pastel.ExtensibleFunction>();
            }
        }

        public List<Pastel.ExtensibleFunction> GetPastelExtensibleFunctions()
        {
            if (this.extensibleFunctionMetadata == null) this.InitTypeInfo();
            return this.extensibleFunctionMetadata;
        }
    }
}
