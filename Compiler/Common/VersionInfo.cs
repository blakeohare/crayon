namespace Common
{
    public static class VersionInfo
    {
        public const int VersionMajor = 2; // VERSION_MAJOR_SCRIPT_UPDATE
        public const int VersionMinor = 1; // VERSION_MINOR_SCRIPT_UPDATE
        public const int VersionBuild = 1; // VERSION_BUILD_SCRIPT_UPDATE

        public static string VersionString { get { return VersionMajor + "." + VersionMinor + "." + VersionBuild; } }
        public static int[] VersionArray { get { return new int[] { VersionMajor, VersionMinor, VersionBuild }; } }
        public static string UserAgent { get { return "Crayon (" + VersionString + ") Have a nice day."; } }
    }
}
