namespace Common
{
    public static class SourceDirectoryFinder
    {
        private static string crayonSourceDirectoryCached = null;
        public static string CrayonSourceDirectory
        {
            // Presumably running from source. Walk up to the root directory and find the Libraries directory.
            // From there use the list of folders.
            // TODO: mark this as DEBUG only
            get
            {
                if (crayonSourceDirectoryCached == null)
                {
                    string currentDirectory = FileUtil.GetCurrentDirectory();
                    while (!string.IsNullOrEmpty(currentDirectory))
                    {
                        string librariesPath = FileUtil.JoinPath(currentDirectory, "Libraries");
                        if (FileUtil.DirectoryExists(librariesPath) &&
                            FileUtil.FileExists(FileUtil.JoinPath(currentDirectory, "Compiler", "CrayonWindows.sln"))) // quick sanity check
                        {
                            crayonSourceDirectoryCached = currentDirectory;
                            break;
                        }
                        currentDirectory = FileUtil.GetParentDirectory(currentDirectory);
                    }
                }
                return crayonSourceDirectoryCached;
            }
        }
    }
}
