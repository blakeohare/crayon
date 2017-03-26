using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    // The purpose of this class is to make it clear which codepaths belong to the old format, but also once this is
    // deleted after the CBX stuff is done, the requisite cleanup will turn into compile-time errors.
    public static class CompatibilityHack
    {
        public static bool IS_CBX_MODE { get; set; }
        public static bool IS_LEGACY_MODE { get { return !IS_CBX_MODE; } }

        public static void RemoveCallingCodeWhenCbxIsFinished() { }

        public static void CriticalTODO(string comment) { }

        public static string GetLegacyPlatformFromNewPlatform(string newPlatform)
        {
            switch (newPlatform)
            {
                case "csharp-app": return "game-csharp-opentk";
                case "python-app": return "game-python-pygame";
                case "javascript-app-gl": return "game-javascript";
                case "java-app": return "game-java-awt";
                default: return newPlatform;
            }
        }
    }
}
