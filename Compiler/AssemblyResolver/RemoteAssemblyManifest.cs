namespace AssemblyResolver
{
    internal class RemoteAssemblyManifest
    {
        private string directory = null;

        public string Directory
        {
            get
            {
                if (this.directory == null)
                {
                    string appData = System.Environment.GetEnvironmentVariable("%APPDATA%");
                    this.directory = System.IO.Path.Combine(appData, "Roaming", "Crayon", "libstore");
                    Common.FileUtil.EnsureFolderExists(this.directory);
                }
                return this.directory;
            }
        }
    }
}
