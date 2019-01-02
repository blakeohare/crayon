using Common;

namespace Platform
{
    public class LibraryForExport
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public bool HasNativeCode { get; set; }
        public string Directory { get; private set; }

        public Multimap<string, ExportEntity> ExportEntities { get; set; }

        public LibraryForExport(string directory)
        {
            this.Directory = directory;
        }

        public override string ToString()
        {
            return "LibraryForExport: <" + this.Name + ">";
        }
    }
}
