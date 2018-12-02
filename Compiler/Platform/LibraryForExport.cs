using Common;

namespace Platform
{
    public class LibraryForExport
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public bool HasNativeCode { get; set; }

        public Pastel.PastelContext PastelContext { get; set; }
        public Multimap<string, ExportEntity> ExportEntities { get; set; }

        public override string ToString()
        {
            return "LibraryForExport: <" + this.Name + ">";
        }
    }
}
