using CommonUtil.Collections;

namespace Platform
{
    public class LibraryForExport
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Directory { get; private set; }

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
