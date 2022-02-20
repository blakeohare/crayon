using System;
using System.Collections.Generic;
using System.Linq;
using Wax.Util.Disk;

namespace Wax
{
    /*
        An assortment of file util functions that take into consideration that part of the path
        may possibly be a CryPkg.

        Attempts to read real files from disk first, and then falls back to checking each directory
        if a CryPkg exists. Caches results.

        If caching is no longer wanted, use Purge()

        A .crypkg file's name will be that of the directory it represents.
        e.g. "C:\foo\bar\baz.crypkg" will represent "C:\foo\bar\baz\..."
    */
    public class PkgAwareFileUtil
    {
        // TODO: I don't like that this is static. Should pass around an instance.
        private static Dictionary<string, Wax.CryPkgDecoder> packageStatuses = new Dictionary<string, Wax.CryPkgDecoder>();

        public PkgAwareFileUtil()
        { }

        public void Purge() { packageStatuses.Clear(); }

        public bool FileExists(string path)
        {
            string fullPath = System.IO.Path.GetFullPath(path);
            if (File.Exists(fullPath)) return true;

            CryPkgPath pkgPath = GetPackagedPath(fullPath, false);
            if (pkgPath == null) return false;
            return pkgPath.Package.FileExists(pkgPath.Path);
        }

        public bool DirectoryExists(string path)
        {
            string fullPath = System.IO.Path.GetFullPath(path);
            if (Directory.Exists(fullPath)) return true;

            CryPkgPath pkgPath = GetPackagedPath(fullPath, true);
            if (pkgPath == null) return false;
            return pkgPath.Package.DirectoryExists(pkgPath.Path);
        }

        public byte[] ReadFileBytes(string path)
        {
            string fullPath = System.IO.Path.GetFullPath(path);
            if (File.Exists(fullPath)) return File.ReadBytes(fullPath);

            CryPkgPath pkgPath = GetPackagedPath(fullPath, false);
            if (pkgPath == null) throw new System.IO.FileNotFoundException(path);
            return pkgPath.Package.ReadFileBytes(pkgPath.Path);
        }

        public string ReadFileText(string path)
        {
            return Wax.Util.UniversalTextDecoder.Decode(ReadFileBytes(path));
        }

        public string[] ListFiles(string path)
        {
            return this.ListDirectoryContents(path, true, false);
        }

        public string[] ListDirectories(string path)
        {
            return this.ListDirectoryContents(path, false, true);
        }

        private static string[] emptyStringArray = new string[0];
        private string[] ListDirectoryContents(string path, bool includeFiles, bool includeDirectories)
        {
            string fullPath = System.IO.Path.GetFullPath(path);
            if (Directory.Exists(fullPath))
            {
                IEnumerable<string> files = includeFiles ? Directory.ListFilesWithAbsolutePaths(fullPath) : emptyStringArray;
                IEnumerable<string> dirs = includeDirectories ? Directory.ListDirectoriesWithAbsolutePaths(fullPath) : emptyStringArray;
                return dirs.Concat(files).ToArray();
            }

            CryPkgPath pkgPath = GetPackagedPath(fullPath, true);
            if (pkgPath == null) throw new System.IO.DirectoryNotFoundException(path);
            return pkgPath.Package.ListDirectory(pkgPath.Path, includeFiles, includeDirectories);
        }

        private CryPkgPath GetPackagedPath(string fullPath, bool isDir)
        {
            string canonicalPath = fullPath.Replace('\\', '/');
            this.EnsureCachePopulated(canonicalPath, isDir);

            string pkgPath = this.GetPathOfHighestCryPkg(canonicalPath, isDir);
            if (pkgPath == null) return null;
            string pkgDir = pkgPath.Substring(0, pkgPath.Length - ".crypkg".Length);
            Wax.CryPkgDecoder pkg = packageStatuses[pkgPath];
            string relativeDir = (pkgDir == canonicalPath)
                ? "."
                : canonicalPath.Substring(pkgDir.Length + 1);
            return new CryPkgPath(relativeDir, pkg);
        }

        private class CryPkgPath
        {
            public CryPkgPath(string path, Wax.CryPkgDecoder package)
            {
                this.Package = package;
                this.Path = path;
            }

            public Wax.CryPkgDecoder Package { get; set; }
            public string Path { get; set; }
        }

        private string[] GetAllPossiblePkgPaths(string path, bool isDir)
        {
            string[] segments = path.Replace('\\', '/').Split('/');
            int segmentsLength = segments.Length;
            if (!isDir) segmentsLength--;

            string[] cumulative = new string[segmentsLength];
            cumulative[0] = segments[0];
            for (int i = 1; i < segmentsLength; ++i)
            {
                cumulative[i] = cumulative[i - 1] + "/" + segments[i];
            }
            string[] pkgPaths = new string[segmentsLength - 1];
            for (int i = 0; i < segmentsLength - 1; ++i)
            {
                pkgPaths[i] = cumulative[i] + "/" + segments[i + 1] + ".crypkg";
            }
            return pkgPaths;
        }

        private void EnsureCachePopulated(string path, bool isDir)
        {
            foreach (string pkgPath in GetAllPossiblePkgPaths(path, isDir))
            {
                if (!packageStatuses.ContainsKey(pkgPath))
                {
                    char rightSep = System.IO.Path.DirectorySeparatorChar;
                    char wrongSep = rightSep == '/' ? '\\' : '/';
                    string pkgRealPath = pkgPath.Replace(wrongSep, rightSep);
                    if (File.Exists(pkgRealPath))
                    {
                        Wax.CryPkgDecoder pkg = new Wax.CryPkgDecoder(File.ReadBytes(pkgRealPath));
                        packageStatuses[pkgPath] = pkg;
                    }
                    else
                    {
                        packageStatuses[pkgPath] = null;
                    }
                }
            }
        }

        private string GetPathOfHighestCryPkg(string path, bool isDir)
        {
            foreach (string pkgPath in GetAllPossiblePkgPaths(path, isDir).Reverse())
            {
                if (!packageStatuses.ContainsKey(pkgPath)) throw new Exception();
                if (packageStatuses[pkgPath] != null)
                {
                    return pkgPath;
                }
            }
            return null;
        }
    }
}
