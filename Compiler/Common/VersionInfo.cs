namespace Common
{
    public static class VersionInfo
    {
        // 2.1.0
        public const int VersionMajor = 2;
        public const int VersionMinor = 1;
        public const int VersionBuild = 0;

        public static string VersionString { get { return VersionMajor + "." + VersionMinor + "." + VersionBuild; } }
        public static int[] VersionArray { get { return new int[] { VersionMajor, VersionMinor, VersionBuild }; } }
        public static string UserAgent { get { return "Crayon (" + VersionString + ") Have a nice day."; } }
    }
}
