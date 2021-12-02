using Parser.Localization;
using System.Collections.Generic;
using System.Linq;

namespace Parser
{
    public class ExternalAssemblyMetadata
    {
        private Dictionary<string, ExternalAssemblyMetadata> directDependencies = new Dictionary<string, ExternalAssemblyMetadata>();

        public string ID { get; set; }
        public bool IsUserDefined { get; set; }
        public Locale InternalLocale { get; set; }
        public HashSet<Locale> SupportedLocales { get; set; }
        public Dictionary<string, string> NameByLocale { get; set; }
        public HashSet<string> OnlyImportableFrom { get; set; }
        public bool IsImportRestricted { get { return this.OnlyImportableFrom.Count > 0; } }
        public string CanonicalKey { get; set; }
        public Dictionary<string, string> SourceCode { get; set; }

        public string GetName(Locale locale)
        {
            return this.NameByLocale.ContainsKey(locale.ID) ? this.NameByLocale[locale.ID] : this.ID;
        }

        public ExternalAssemblyMetadata[] DirectDependencies
        {
            get
            {
                return this.directDependencies.Keys
                    .OrderBy(k => k.ToLowerInvariant())
                    .Select(k => this.directDependencies[k])
                    .ToArray();
            }
        }

        public void RegisterDependencies(ExternalAssemblyMetadata assembly)
        {
            if (this.directDependencies == null)
            {
                this.directDependencies = new Dictionary<string, ExternalAssemblyMetadata>();
            }
            this.directDependencies[assembly.ID] = assembly;
        }

        public bool IsAllowedImport(ExternalAssemblyMetadata fromAssembly)
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
    }
}
