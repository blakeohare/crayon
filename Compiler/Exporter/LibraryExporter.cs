using AssemblyResolver;
using Common;
using CommonUtil.Disk;
using System.Collections.Generic;
using System.Linq;

namespace Exporter
{
    // Library is only instantiable in the context of a specific platform, which is not ideal, but not causing any problems at the moment.
    public class LibraryExporter
    {
        public AssemblyMetadata Metadata { get; private set; }
        private string platformName;

        public Dictionary<string, object> CompileTimeConstants { get; set; }

        internal LibraryResourceDatabase Resources { get; private set; }

        private static Dictionary<string, LibraryExporter> libraryCache = new Dictionary<string, LibraryExporter>();

        private static string GetLibKey(AssemblyMetadata metadata, Platform.AbstractPlatform platform)
        {
            return metadata.CanonicalKey + "#" + platform.Name;
        }

        // TODO: this ought to go away and the cache needs to move to some sort of scope whose lifetime is tied to a specific compilation scope.
        public static LibraryExporter Get(AssemblyMetadata metadata, Platform.AbstractPlatform platform)
        {
            string key = GetLibKey(metadata, platform);
            if (!libraryCache.ContainsKey(key))
            {
                libraryCache[key] = new LibraryExporter(metadata, platform);
            }
            return libraryCache[key];
        }

        private LibraryExporter(AssemblyMetadata metadata, Platform.AbstractPlatform platform)
        {
            TODO.LibrariesNeedVersionNumber();

            this.Metadata = metadata;
            this.platformName = platform.Name;

            this.Resources = new LibraryResourceDatabase(this, platform);

            this.CompileTimeConstants = this.LoadFlagsForPlatform(platform);
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
            string path = FileUtil.JoinAndCanonicalizePath(this.Metadata.Directory, "pastel", "flags", platformId + ".txt");
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
    }
}
