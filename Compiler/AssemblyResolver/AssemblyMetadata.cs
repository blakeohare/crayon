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
            return (ExternalAssemblyMetadata)md;
        }

        public static ExternalAssemblyMetadata[] Bridge(IList<InternalAssemblyMetadata> md)
        {
            return md.Cast<ExternalAssemblyMetadata>().ToArray();
        }
    }

    public class ExternalAssemblyMetadata : InternalAssemblyMetadata { }

    public class InternalAssemblyMetadata
    {
        public override string ToString()
        {
            return "AssemblyMetadata: " + this.ID + " (" + this.Version + ")";
        }

        public string Directory { get; set; }
        public string ID { get; set; }
        public Locale InternalLocale { get; set; }
        public string CanonicalKey { get; set; }
        public HashSet<Locale> SupportedLocales { get; set; }
        public HashSet<string> OnlyImportableFrom { get; set; }
        public bool IsUserDefined { get; set; }
        public Dictionary<string, string> NameByLocale { get; set; }
        private Dictionary<string, InternalAssemblyMetadata> directDependencies = new Dictionary<string, InternalAssemblyMetadata>();

        public string Version { get { return "v1"; } } // TODO: versions
        public bool IsImportRestricted { get { return this.OnlyImportableFrom.Count > 0; } }

        public InternalAssemblyMetadata()
        {
            this.NameByLocale = new Dictionary<string, string>();
        }

        public InternalAssemblyMetadata[] DirectDependencies
        {
            get
            {
                return this.directDependencies.Keys
                    .OrderBy(k => k.ToLowerInvariant())
                    .Select(k => this.directDependencies[k])
                    .ToArray();
            }
        }

        public void RegisterDependencies(InternalAssemblyMetadata assembly)
        {
            if (this.directDependencies == null)
            {
                this.directDependencies = new Dictionary<string, InternalAssemblyMetadata>();
            }
            this.directDependencies[assembly.ID] = assembly;
        }

        public string GetName(Locale locale)
        {
            return this.NameByLocale.ContainsKey(locale.ID) ? this.NameByLocale[locale.ID] : this.ID;
        }

        public bool IsAllowedImport(InternalAssemblyMetadata fromAssembly)
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
