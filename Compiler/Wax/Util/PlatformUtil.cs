namespace Wax.Util
{
    public static class PlatformUtil
    {
        private static bool isWindows = System.IO.Path.DirectorySeparatorChar == '\\';
        public static bool IsWindows { get { return isWindows; } }
    }
}
