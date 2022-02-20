using Wax.Util.Disk;

namespace Crayon
{
    internal static class SourceDirectoryFinder
    {
        private static string crayonSourceDirectoryCached = null;
        public static string CrayonSourceDirectory
        {
            // Presumably running from source. Walk up to the root directory and find the Libraries directory.
            // From there use the list of folders.
            // TODO: mark this as DEBUG only
            get
            {
#if DEBUG
                if (crayonSourceDirectoryCached == null)
                {
                    string currentDirectory = DiskUtil.GetCurrentDirectory();
                    while (!string.IsNullOrEmpty(currentDirectory))
                    {
                        string librariesPath = DiskUtil.JoinPathNative(currentDirectory, "Libraries");
                        if (FileUtil.DirectoryExists(librariesPath) &&
                            FileUtil.FileExists(DiskUtil.JoinPathNative(currentDirectory, "Compiler", "CrayonWindows.sln"))) // quick sanity check
                        {
                            crayonSourceDirectoryCached = currentDirectory;
                            break;
                        }
                        currentDirectory = FileUtil.GetParentDirectory(currentDirectory);
                    }
                }
#endif
                return crayonSourceDirectoryCached;
            }
        }
    }
}
