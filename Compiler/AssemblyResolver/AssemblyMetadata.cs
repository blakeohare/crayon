using Common;
using CommonUtil.Disk;
using Common.Localization;
using System.Collections.Generic;
using System.Linq;

namespace AssemblyResolver
{
    public static class AssemblyTODO
    {
        public static ExternalAssemblyMetadata Bridge(InternalAssemblyMetadata md)
        {
            return new ExternalAssemblyMetadata() {
                ID = md.ID,
                IsUserDefined = false,
                InternalLocale = md.InternalLocale,
                SupportedLocales = new HashSet<Locale>(md.SupportedLocales),
                NameByLocale = md.NameByLocale,
                OnlyImportableFrom = new HashSet<string>(md.OnlyImportableFrom),
                CanonicalKey = md.InternalLocale.ID + ":" + md.ID,
                SourceCode = md.GetSourceCode(),
            };
        }

        public static ExternalAssemblyMetadata[] Bridge(IList<InternalAssemblyMetadata> md)
        {
            return md.Cast<ExternalAssemblyMetadata>().ToArray();
        }
    }

    public class InternalAssemblyMetadata
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

        public InternalAssemblyMetadata()
        {
            this.NameByLocale = new Dictionary<string, string>();
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

        private PkgAwareFileUtil fileUtil = new PkgAwareFileUtil();

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
