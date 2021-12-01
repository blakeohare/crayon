using CommonUtil.Disk;
using System.Collections.Generic;

namespace Platform
{
    public class TemplateReader
    {
        private List<string> platformNamesMostGeneralFirst = new List<string>();
        private Common.PkgAwareFileUtil fileUtil;
        private string activeCrayonSourceRoot;

        public TemplateReader(Common.PkgAwareFileUtil fileUtil, AbstractPlatform platform, string activeCrayonSourceRoot)
        {
            this.activeCrayonSourceRoot = activeCrayonSourceRoot;
            this.fileUtil = fileUtil;
            AbstractPlatform walker = platform;
            while (walker != null)
            {
                platformNamesMostGeneralFirst.Add(walker.Name);
                walker = walker.ParentPlatform;
            }
            platformNamesMostGeneralFirst.Reverse();
        }

        public TemplateSet GetVmTemplates()
        {
            Dictionary<string, byte[]> output = new Dictionary<string, byte[]>();

            string crayonHome = CommonUtil.Environment.EnvironmentVariables.Get("CRAYON_HOME");
            if (crayonHome != null)
            {
                // Search %CRAYON_HOME%/vmsrc directory for the VM files for the given platforms.
                // Files associated with more specific platforms will overwrite the less specific ones.
                foreach (string platformName in this.platformNamesMostGeneralFirst)
                {
                    string packagedVmSource = Path.Join(crayonHome, "vmsrc", platformName + ".crypkg");
                    if (File.Exists(packagedVmSource))
                    {
                        byte[] pkgBytes = File.ReadBytes(packagedVmSource);
                        Common.CryPkgDecoder pkgDecoder = new Common.CryPkgDecoder(pkgBytes);
                        ReadAllFilesCryPkg(pkgDecoder, output, "");
                    }
                }
            }

#if DEBUG
            // If you're running a debug build and have the source directory present,
            // then use the Interpreter/gen files instead of the crypkg versions in CRAYON_HOME.
            if (this.activeCrayonSourceRoot != null)
            {
                output.Clear(); // reset.

                foreach (string platformName in this.platformNamesMostGeneralFirst)
                {
                    string vmTemplateDir = Path.Join(this.activeCrayonSourceRoot, "Interpreter", "gen", platformName);
                    if (Directory.Exists(vmTemplateDir))
                    {
                        ReadAllFiles(output, System.IO.Path.GetFullPath(vmTemplateDir).Length + 1, vmTemplateDir);
                    }
                }
            }
#endif

            if (output.Count == 0)
            {
                throw new System.InvalidOperationException("Could not find VM templates. Is the CRAYON_HOME environment variable set correctly?");
            }

            return new TemplateSet(output);
        }

        private void ReadAllFilesCryPkg(Common.CryPkgDecoder pkg, Dictionary<string, byte[]> output, string prefix)
        {
            string currentDir = prefix.Length == 0 ? "." : prefix;

            foreach (string dir in pkg.ListDirectory(currentDir, false, true))
            {
                string path = currentDir == "." ? dir : currentDir + "/" + dir;
                ReadAllFilesCryPkg(pkg, output, path);
            }

            foreach (string file in pkg.ListDirectory(currentDir, true, false))
            {
                string path = currentDir == "." ? file : currentDir + "/" + file;
                output[path] = pkg.ReadFileBytes(path);
            }
        }

        private void ReadAllFiles(Dictionary<string, byte[]> output, int pathTrimLength, string dir)
        {
            foreach (string path in fileUtil.ListFiles(dir))
            {
                string fullPath = Path.Join(dir, path);
                output[fullPath.Substring(pathTrimLength).Replace('\\', '/')] = fileUtil.ReadFileBytes(fullPath);
            }

            foreach (string path in fileUtil.ListDirectories(dir))
            {
                string fullPath = Path.Join(dir, path);
                ReadAllFiles(output, pathTrimLength, fullPath);
            }
        }
    }
}
