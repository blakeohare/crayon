﻿namespace Wax.Util.Disk
{
    public static class Path
    {
        private static string separator = System.IO.Path.DirectorySeparatorChar + "";
        public static string Separator { get { return separator; } }

        public static string Join(params string[] parts)
        {
            return System.IO.Path.Combine(parts);
        }

        public static bool IsAbsolute(string path)
        {
            if (Wax.Util.PlatformUtil.IsWindows)
            {
                if (path.Length > 1 && path[1] == ':') return true;
            }
            else
            {
                if (path.StartsWith("/")) return true;
                if (path.StartsWith("~")) return true;
            }
            return false;
        }

        public static string GetFileName(string path)
        {
            return System.IO.Path.GetFileName(path);
        }

        public static string GetCurrentDirectory()
        {
            return System.IO.Directory.GetCurrentDirectory();
        }
    }
}
