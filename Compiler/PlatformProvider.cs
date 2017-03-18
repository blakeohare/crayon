using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Platform;

namespace Crayon
{
    class PlatformProvider : IPlatformProvider
    {
        private Dictionary<string, Platform.AbstractPlatform> platforms;
        public Platform.AbstractPlatform GetPlatform(string name)
        {
            if (platforms == null)
            {
                platforms = new Dictionary<string, Platform.AbstractPlatform>();
                foreach (System.Reflection.Assembly assembly in GetRawAssemblies())
                {
                    Platform.AbstractPlatform platform = this.GetPlatformInstance(assembly);
                    platform.PlatformProvider = this;
                    string key = platform.Name;
                    if (platforms.ContainsKey(key))
                    {
                        throw new InvalidOperationException("Multiple platforms with the same ID: '" + key + "'");
                    }
                    platforms[key] = platform;
                }
            }

            if (name != null && platforms.ContainsKey(name))
            {
                return platforms[name];
            }

            return null;
        }

        private Platform.AbstractPlatform GetPlatformInstance(System.Reflection.Assembly assembly)
        {
            foreach (System.Type type in assembly.GetExportedTypes())
            {
                // TODO: check to make sure it inherits from AbstractPlatform
                // Perhaps make that the only qualification instead of going by name?
                if (type.Name == "PlatformImpl")
                {
                    return (Platform.AbstractPlatform)assembly.CreateInstance(type.FullName);
                }
            }
            throw new InvalidOperationException("This assembly does not define a PlatformImpl type: " + assembly.FullName);
        }

        private static System.Reflection.Assembly[] GetRawAssemblies()
        {
            // TODO: create a dev Crayon csproj that has a strong project reference to the platforms
            // and a release csproj that does not and then ifdef out the implementation of this function.
            return new System.Reflection.Assembly[] {
                typeof(GameCSharpOpenTk.PlatformImpl).Assembly,
                typeof(GameJavaScriptHtml5.PlatformImpl).Assembly,
                typeof(GamePythonPygame.PlatformImpl).Assembly,
                typeof(LangCSharp.PlatformImpl).Assembly,
                typeof(LangJavaScript.PlatformImpl).Assembly,
                typeof(LangPython.PlatformImpl).Assembly,
            };
        }
    }
}
