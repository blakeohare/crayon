namespace Wax.Util.Disk
{
    public static class Directory
    {
        public static bool Exists(string path)
        {
            return System.IO.Directory.Exists(path);
        }

        public static string[] ListFilesWithAbsolutePaths(string directory)
        {
            return System.IO.Directory.GetFiles(directory);
        }

        public static string[] ListDirectoriesWithAbsolutePaths(string directory)
        {
            return System.IO.Directory.GetDirectories(directory);
        }
    }
}
