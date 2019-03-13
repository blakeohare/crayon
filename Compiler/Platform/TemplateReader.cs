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

        public TemplateSet GetLibraryTemplates(LibraryForExport library)
        {
            Dictionary<string, byte[]> output = new Dictionary<string, byte[]>();
            foreach (string platformName in this.platformNamesMostGeneralFirst)
            {
                string libTemplateDir = System.IO.Path.Combine(library.Directory, "native", platformName);
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

            string crayonHome = System.Environment.GetEnvironmentVariable("CRAYON_HOME");
            if (crayonHome != null)
            {
                // Search %CRAYON_HOME%/vmsrc directory for the VM files for the given platforms.
                // Files associated with more specific platforms will overwrite the less specific ones.
                foreach (string platformName in this.platformNamesMostGeneralFirst)
                {
                    string packagedVmSource = System.IO.Path.Combine(crayonHome, "vmsrc", platformName + ".crypkg");
                    if (System.IO.File.Exists(packagedVmSource))
                    {
                        byte[] pkgBytes = System.IO.File.ReadAllBytes(packagedVmSource);
                        Common.CryPkgDecoder pkgDecoder = new Common.CryPkgDecoder(pkgBytes);
                        ReadAllFilesCryPkg(pkgDecoder, output, "");
                    }
                }
            }

#if DEBUG
            // If you're running a debug build and have the source directory present,
            // then use the Interpreter/gen files instead of the crypkg versions in CRAYON_HOME.
            string crayonSourceDir = Common.SourceDirectoryFinder.CrayonSourceDirectory;

            if (crayonSourceDir != null)
            {
                output.Clear(); // reset.

                foreach (string platformName in this.platformNamesMostGeneralFirst)
                {
                    string vmTemplateDir = System.IO.Path.Combine(crayonSourceDir, "Interpreter", "gen", platformName);
                    if (System.IO.Directory.Exists(vmTemplateDir))
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
