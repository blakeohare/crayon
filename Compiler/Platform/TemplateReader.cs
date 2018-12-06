using System.Collections.Generic;

namespace Platform
{
    public class TemplateReader
    {
        private List<string> platformNamesMostGeneralFirst = new List<string>();
        private Common.PkgAwareFileUtil fileUtil;

        public TemplateReader(Common.PkgAwareFileUtil fileUtil, AbstractPlatform platform)
        {
            this.fileUtil = fileUtil;
            AbstractPlatform walker = platform;
            while (walker != null)
            {
                platformNamesMostGeneralFirst.Add(walker.Name);
                walker = walker.ParentPlatform;
            }
            platformNamesMostGeneralFirst.Reverse();
        }

        public TemplateSet GetLibraryTemplates(string libraryName)
        {
            Dictionary<string, byte[]> output = new Dictionary<string, byte[]>();
            foreach (string platformName in this.platformNamesMostGeneralFirst)
            {
                string libTemplateDir = System.IO.Path.Combine(
                    Common.SourceDirectoryFinder.CrayonSourceDirectory,
                    "Libraries", libraryName, "native", platformName);
                if (fileUtil.DirectoryExists(libTemplateDir))
                {
                    ReadAllFiles(output, System.IO.Path.GetFullPath(libTemplateDir).Length + 1, libTemplateDir);
                }
            }
            return new TemplateSet(output);
        }

        public TemplateSet GetVmTemplates()
        {
            Dictionary<string, byte[]> output = new Dictionary<string, byte[]>();
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
            return new TemplateSet(output);
        }

        private void ReadAllFiles(Dictionary<string, byte[]> output, int pathTrimLength, string dir)
        {
            foreach (string path in fileUtil.ListFiles(dir))
            {
                string fullPath = System.IO.Path.Combine(dir, path);
                output[fullPath.Substring(pathTrimLength).Replace('\\', '/')] = fileUtil.ReadFileBytes(fullPath);
            }

            foreach (string path in fileUtil.ListDirectories(dir))
            {
                string fullPath = System.IO.Path.Combine(dir, path);
                ReadAllFiles(output, pathTrimLength, fullPath);
            }
        }
    }
}
