namespace CommonUtil.Environment
{
    public static class Platform
    {
        private static bool isWindows = System.IO.Path.DirectorySeparatorChar == '\\';
        public static bool IsWindows { get { return isWindows; } }
    }
}
