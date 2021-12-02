using Parser.Localization;
using System.Collections.Generic;
using System.Linq;
using Wax.Util.Disk;

namespace AssemblyResolver
{
    internal class InternalAssemblyMetadata
    {
        public override string ToString()
        {
            return "AssemblyMetadata: " + this.ID + " (" + this.Version + ")";
        }

        public string Directory { get; set; }
        public string ID { get; set; }
        public Locale InternalLocale { get; set; }
        public Locale[] SupportedLocales { get; set; }
        public string[] OnlyImportableFrom { get; set; }
        public Dictionary<string, string> NameByLocale { get; set; }

        public string Version { get { return "v1"; } } // TODO: versions

        public InternalAssemblyMetadata(string id, Locale internalLocale, string directory)
        {
            this.ID = id;
            this.InternalLocale = internalLocale;
            this.NameByLocale = new Dictionary<string, string>() {
                { this.InternalLocale.ID, this.ID },
            };
            this.Directory = directory;
        }

        public string GetName(Locale locale)
        {
            return this.NameByLocale.ContainsKey(locale.ID) ? this.NameByLocale[locale.ID] : this.ID;
        }

        internal Dictionary<string, string> GetSourceCode()
        {
            Dictionary<string, string> output = new Dictionary<string, string>();
            string srcDir = FileUtil.JoinPath(this.Directory, "src");
            if (!FileUtil.DirectoryExists(srcDir))
            {
                throw new System.InvalidOperationException(this.Directory + " is missing a 'src' directory");
            }
            string[] srcFiles = FileUtil.GetAllFilePathsRelativeToRoot(srcDir);
            foreach (string srcFile in srcFiles.Where(name => name.ToLowerInvariant().EndsWith(".cry")))
            {
                string code = this.ReadFile(false, "src/" + srcFile, false);
                output[this.ID + ":" + srcFile] = code;
            }
            return output;
        }

        private Wax.PkgAwareFileUtil fileUtil = new Wax.PkgAwareFileUtil();

        public byte[] ReadFileBytes(string pathRelativeToLibraryRoot)
        {
            string fullPath = FileUtil.JoinPath(this.Directory, pathRelativeToLibraryRoot);
            if (fileUtil.FileExists(fullPath))
            {
                return fileUtil.ReadFileBytes(fullPath);
            }
            throw new System.InvalidOperationException("The '" + this.ID + "' library does not contain the resource '" + pathRelativeToLibraryRoot + "'");
        }

        public string ReadFile(bool keepPercents, string pathRelativeToLibraryRoot, bool failSilently)
        {
            string fullPath = FileUtil.JoinPath(this.Directory, pathRelativeToLibraryRoot);
            if (fileUtil.FileExists(fullPath))
            {
                return fileUtil.ReadFileText(fullPath);
            }

            if (failSilently)
            {
                return "";
            }

            throw new System.InvalidOperationException("Missing resource in library '" + this.ID + "': '" + pathRelativeToLibraryRoot + "'");
        }
    }
}
