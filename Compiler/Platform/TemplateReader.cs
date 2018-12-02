using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform
{
    public class TemplateReader
    {
        private List<string> platformNamesMostGeneralFirst = new List<string>();

        public TemplateReader(AbstractPlatform platform)
        {
            AbstractPlatform walker = platform;
            while (walker != null)
            {
                platformNamesMostGeneralFirst.Add(walker.Name);
                walker = walker.ParentPlatform;
            }
            platformNamesMostGeneralFirst.Reverse();
        }

        public Dictionary<string, string> GetLibraryTemplates(string libraryName)
        {
            Dictionary<string, string> output = new Dictionary<string, string>();
            foreach (string platformName in this.platformNamesMostGeneralFirst)
            {
                string libTemplateDir = System.IO.Path.Combine(
                    Common.SourceDirectoryFinder.CrayonSourceDirectory,
                    "Libraries", libraryName, "native", platformName);
                if (System.IO.Directory.Exists(libTemplateDir))
                {
                    ReadAllFiles(output, System.IO.Path.GetFullPath(libTemplateDir).Length + 1, libTemplateDir);
                }
            }
            return output;
        }

        public Dictionary<string, string> GetVmTemplates()
        {
            Dictionary<string, string> output = new Dictionary<string, string>();
            foreach (string platformName in this.platformNamesMostGeneralFirst)
            {
                string vmTemplateDir = System.IO.Path.Combine(
                    Common.SourceDirectoryFinder.CrayonSourceDirectory,
                    "Interpreter", "gen", platformName);
                if (System.IO.Directory.Exists(vmTemplateDir))
                {
                    ReadAllFiles(output, System.IO.Path.GetFullPath(vmTemplateDir).Length + 1, vmTemplateDir);
                }
            }
            return output;
        }

        private void ReadAllFiles(Dictionary<string, string> output, int pathTrimLength, string dir)
        {
            foreach (string path in System.IO.Directory.GetFiles(dir))
            {
                string fullPath = System.IO.Path.Combine(dir, path);
                output[path.Substring(pathTrimLength).Replace('\\', '/')] = System.IO.File.ReadAllText(fullPath);
            }

            foreach (string path in System.IO.Directory.GetDirectories(dir))
            {
                string fullPath = System.IO.Path.Combine(dir, path);
                ReadAllFiles(output, pathTrimLength, fullPath);
            }
        }
    }
}
