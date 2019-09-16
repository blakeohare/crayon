namespace CommonUtil.Environment
{
    public static class Platform
    {
        public static bool IsWindows { get; } = System.IO.Path.DirectorySeparatorChar == '\\';
    }
}
