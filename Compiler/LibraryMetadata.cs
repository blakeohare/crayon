namespace Crayon
{
    class LibraryMetadata
    {
        public string Directory { get; private set; }
        public string Name { get; private set; }
        public string Manifest { get; private set; }

        public LibraryMetadata(string directory, string name)
        {
            this.Directory = directory;
            this.Name = name;
            this.Manifest = System.IO.File.ReadAllText(System.IO.Path.Combine(directory, "manifest.txt"));
        }
    }
}
