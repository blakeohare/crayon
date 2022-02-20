namespace Wax.Util.Disk
{
    public class DiskUtil
    {
        private WaxHub hub;

        public DiskUtil(WaxHub hub)
        {
            this.hub = hub;
        }

        public static string JoinPathCanonical(params string[] args)
        {
            return string.Join('/', args);
        }

        public static string JoinPathNative(params string[] args)
        {
            return string.Join(System.IO.Path.DirectorySeparatorChar, args);
        }

        public static string GetFileName(string path)
        {
            string[] parts = path.Split('/');
            string part = parts[parts.Length - 1];
            parts = part.Split('\\');
            part = parts[parts.Length - 1];
            return part;
        }

        public static string GetCurrentDirectory()
        {
            return System.IO.Directory.GetCurrentDirectory();
        }

        public static bool IsAbsolute(string path)
        {
            if (PlatformUtil.IsWindows)
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
    }
}
