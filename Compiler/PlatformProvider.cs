using System;
using System.Collections.Generic;
using System.Linq;
using Common;

namespace Crayon
{
    class PlatformProvider : IPlatformProvider
    {
        private Dictionary<string, Common.AbstractPlatform> platforms;
        public Common.AbstractPlatform GetPlatform(string name)
        {
            if (platforms == null)
            {
                platforms = new Dictionary<string, Common.AbstractPlatform>();
                foreach (System.Reflection.Assembly assembly in GetRawAssemblies())
                {
                    Common.AbstractPlatform platform = (Common.AbstractPlatform)assembly.CreateInstance("Platform");
                    platform.PlatformProvider = this;
                    string key = platform.Name;
                    if (platforms.ContainsKey(key))
                    {
                        throw new InvalidOperationException("Multiple platforms with the same ID: '" + key + "'");
                    }
                    platforms[key] = platform;
                }
            }

            if (platforms.ContainsKey(name))
            {
                return platforms[name];
            }

            return null;
        }

        private static System.Reflection.Assembly[] GetRawAssemblies()
        {
            // TODO: create a dev Crayon csproj that has a strong project reference to the platforms
            // and a release csproj that does not and then ifdef out the implementation of this function.
            return new System.Reflection.Assembly[] {
               typeof(GamePythonPygame.Platform).Assembly,
               typeof(LangPython.Platform).Assembly,
            };
        }
    }
}
