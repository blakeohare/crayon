using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class VersionInfo
    {
        public const int VersionMajor = 0;
        public const int VersionMinor = 2;
        public const int VersionBuild = 1;

        public static string VersionString { get { return VersionMajor + "." + VersionMinor + "." + VersionBuild; } }
        public static int[] VersionArray { get { return new int[] { VersionMajor, VersionMinor, VersionBuild }; } }
        public static string UserAgent { get { return "Crayon (" + VersionString + ") Have a nice day."; } }
    }
}
