using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wax.Util.Disk
{
    public static class FileUtil
    {
        private static string DIR_SEP = "" + System.IO.Path.DirectorySeparatorChar;

        public static string GetCanonicalExtension(string path)
        {
            string output = System.IO.Path.GetExtension(path);
            if (output.Length == 0) return null;
            return output.Substring(1).ToLowerInvariant();
        }

        public static Task WriteFileTextAsync(string path, string content)
        {
            WriteFileText_DEPRECATED(path, content);
            return Task.CompletedTask;
        }

        public static void WriteFileText_DEPRECATED(string path, string content)
        {
            path = NormalizePath(path);
            System.IO.File.WriteAllText(path, content, System.Text.Encoding.UTF8);
        }

        public static Task WriteFileBytesAsync(string path, byte[] content)
        {
            WriteFileBytes_DEPRECATED(path, content);
            return Task.CompletedTask;
        }

        public static void WriteFileBytes_DEPRECATED(string path, byte[] content)
        {
            path = NormalizePath(path);
            System.IO.File.WriteAllBytes(path, content);
        }

        public static Task<byte[]> ReadFileBytesAsync(string path)
        {
            return Task.FromResult(ReadFileBytes_DEPRECATED(path));
        }

        public static byte[] ReadFileBytes_DEPRECATED(string path)
        {
            path = NormalizePath(path);
            return System.IO.File.ReadAllBytes(path);
        }

        public static string JoinPath_DEPRECATED(params string[] parts)
        {
            string output = NormalizePath(string.Join(DIR_SEP, parts));
            while (output.Contains(DIR_SEP + DIR_SEP))
            {
                output = output.Replace(DIR_SEP + DIR_SEP, DIR_SEP);
            }
            return output;
        }

        public static string JoinAndCanonicalizePath_DEPRECATED(params string[] parts)
        {
            string path = JoinPath_DEPRECATED(parts);
            path = GetCanonicalizeUniversalPath(path);
            path = GetPlatformPath(path);
            return path;
        }

        public static Task<string[]> DirectoryListFileNamesAsync(string dir)
        {
            return Task.FromResult(DirectoryListFileNames_DEPRECATED(dir));
        }

        public static string[] DirectoryListFileNames_DEPRECATED(string dir)
        {
            return ListDirImpl(dir, true, false);
        }

        public static Task<string[]> DirectoryListFilePathsAsync(string dir)
        {
            return Task.FromResult(DirectoryListFilePaths_DEPRECATED(dir));
        }

        public static string[] DirectoryListFilePaths_DEPRECATED(string dir)
        {
            return ListDirImpl(dir, true, true);
        }

        public static Task<string[]> DirectoryListDirectoryNamesAsync(string dir)
        {
            return Task.FromResult(DirectoryListDirectoryNames_DEPRECATED(dir));
        }

        public static string[] DirectoryListDirectoryNames_DEPRECATED(string dir)
        {
            return ListDirImpl(dir, false, false);
        }

        public static Task<string[]> DirectoryListDirectoryPathsAsync(string dir)
        {
            return Task.FromResult(DirectoryListDirectoryPaths_DEPRECATED(dir));
        }

        public static string[] DirectoryListDirectoryPaths_DEPRECATED(string dir)
        {
            return ListDirImpl(dir, false, true);
        }

        public static Task DirectoryDeleteAsync(string dir)
        {
            DirectoryDelete_DEPRECATED(dir);
            return Task.CompletedTask;
        }

        public static void DirectoryDelete_DEPRECATED(string dir)
        {
            string path = JoinAndCanonicalizePath_DEPRECATED(dir);
            System.IO.Directory.Delete(dir, true);
        }

        public static Task EnsureFolderExistsAsync(string path)
        {
            EnsureFolderExists_DEPRECATED(path);
            return Task.CompletedTask;
        }

        public static void EnsureFolderExists_DEPRECATED(string path)
        {
            path = path.Trim();
            if (!DirectoryExists_DEPRECATED(path))
            {
                EnsureParentFolderExists_DEPRECATED(path);
                CreateDirectory_DEPRECATED(path);
            }
        }

        public static Task EnsureParentFolderExistsAsync(string path)
        {
            EnsureParentFolderExists_DEPRECATED(path);
            return Task.CompletedTask;
        }

        public static void EnsureParentFolderExists_DEPRECATED(string path)
        {
            path = path.Trim();
            if (path.Length > 0)
            {
                string folder = System.IO.Path.GetDirectoryName(path);
                if (folder.Length > 0 && !DirectoryExists_DEPRECATED(folder))
                {
                    EnsureParentFolderExists_DEPRECATED(folder);
                    CreateDirectory_DEPRECATED(folder);
                }
            }
        }

        public static Task CreateDirectoryAsync(string path)
        {
            CreateDirectory_DEPRECATED(path);
            return Task.CompletedTask;
        }

        public static void CreateDirectory_DEPRECATED(string path)
        {
            path = NormalizePath(path);
            System.IO.Directory.CreateDirectory(path);
        }

        public static Task<bool> DirectoryExistsAsync(string path)
        {
            return Task.FromResult(DirectoryExists_DEPRECATED(path));
        }

        public static bool DirectoryExists_DEPRECATED(string path)
        {
            path = NormalizePath(path);
            return System.IO.Directory.Exists(path);
        }

        public static Task<bool> FileExistsAsync(string path)
        {
            return Task.FromResult(FileExists(path));
        }

        public static bool FileExists(string path)
        {
            path = NormalizePath(path);
            return System.IO.File.Exists(path);
        }

        public static Task<string[]> GetAllAbsoluteFilePathsDescendentsOfAsync(string absoluteRoot)
        {
            return Task.FromResult(GetAllAbsoluteFilePathsDescendentsOf_DEPRECATED(absoluteRoot));
        }

        private static string[] GetAllAbsoluteFilePathsDescendentsOf_DEPRECATED(string absoluteRoot)
        {
            string[] output = GetAllFilePathsRelativeToRoot_DEPRECATED(absoluteRoot);
            for (int i = 0; i < output.Length; ++i)
            {
                output[i] = FileUtil.GetCanonicalizeUniversalPath(absoluteRoot + "/" + output[i]);
            }
            return output;
        }

        // ignores silly files such as thumbs.db, .ds_store, .svn/*, etc
        public static Task<string[]> GetAllFilePathsRelativeToRootAsync(string root)
        {
            return Task.FromResult(GetAllFilePathsRelativeToRoot_DEPRECATED(root));
        }

        public static string[] GetAllFilePathsRelativeToRoot_DEPRECATED(string root)
        {
            List<string> files = new List<string>();
            GetAllFilePathsRelativeToRootImpl(root, files);
            string[] output = files.ToArray();
            int rootLength = root.Length + 1;
            for (int i = 0; i < output.Length; ++i)
            {
                output[i] = output[i].Substring(rootLength);
            }
            return output;
        }

        private static readonly HashSet<string> IGNORED_FILES = new HashSet<string>(
            new string[] {
                ".ds_store",
                "thumbs.db",
            });

        private static readonly HashSet<string> IGNORED_DIRECTORIES = new HashSet<string>(
            new string[] {
                ".svn",
            });

        private static void GetAllFilePathsRelativeToRootImpl(string currentRoot, List<string> output)
        {
            string[] directories = ListDirImpl(currentRoot, false, false);
            foreach (string directory in directories)
            {
                if (!IGNORED_DIRECTORIES.Contains(directory))
                {
                    GetAllFilePathsRelativeToRootImpl(JoinPath_DEPRECATED(currentRoot, directory), output);
                }
            }

            string[] files = ListDirImpl(currentRoot, true, false);
            foreach (string file in files)
            {
                if (!IGNORED_FILES.Contains(file))
                {
                    output.Add(JoinPath_DEPRECATED(currentRoot, file));
                }
            }
        }

        private static string[] ListDirImpl(string dir, bool isFiles, bool fullPath)
        {
            string[] output = isFiles
                ? System.IO.Directory.GetFiles(NormalizePath(dir))
                : System.IO.Directory.GetDirectories(NormalizePath(dir));

            for (int i = 0; i < output.Length; ++i)
            {
                output[i] = NormalizePath(output[i]);
            }

            if (!fullPath)
            {
                int baseLength = dir.Length + 1;
                for (int i = 0; i < output.Length; ++i)
                {
                    output[i] = output[i].Substring(baseLength);
                }
            }

            return output;
        }

        public static string GetCanonicalizeUniversalPath(string path)
        {
            List<string> output = new List<string>();
            if (path.StartsWith("/"))
            {
                output.Add("");
            }
            foreach (string part in path.Replace('\\', '/').Split('/'))
            {
                switch (part)
                {
                    case "":
                    case ".":
                        break;

                    case "..":
                        if (output.Count > 0 && output[output.Count - 1] != "..")
                        {
                            output.RemoveAt(output.Count - 1);
                        }
                        else
                        {
                            output.Add("..");
                        }
                        break;

                    default:
                        output.Add(part);
                        break;
                }
            }
            return string.Join("/", output);
        }

        private static string NormalizePath(string dir)
        {
            dir = dir.Trim().Replace('\\', '/').TrimEnd('/');
            if (dir.Length == 0) dir = "/";
            return dir.Replace('/', DIR_SEP[0]);
        }

        public static string ConvertAbsolutePathToRelativePath(string absolutePath, string relativeToThisAbsolutePath)
        {
            string[] partsTarget = GetCanonicalizeUniversalPath(absolutePath).Split('/');
            string[] partsRelativeTo = GetCanonicalizeUniversalPath(relativeToThisAbsolutePath).Split('/');
            int min = Math.Min(partsTarget.Length, partsRelativeTo.Length);
            List<string> output = new List<string>();
            for (int i = 0; i < min; ++i)
            {
                if (partsTarget[i] != partsRelativeTo[i])
                {
                    int dotDots = partsRelativeTo.Length - i;
                    while (dotDots-- > 0)
                    {
                        output.Add("..");
                    }
                    for (int j = i; j < partsTarget.Length; ++j)
                    {
                        output.Add(partsTarget[j]);
                    }
                    break;
                }

                if (i == min - 1)
                {
                    if (partsTarget.Length == partsRelativeTo.Length)
                    {
                        return ".";
                    }

                    if (partsTarget.Length < partsRelativeTo.Length)
                    {
                        for (int j = 0; j < partsRelativeTo.Length - partsTarget.Length; ++j)
                        {
                            output.Add("..");
                        }
                    }
                    else
                    {
                        for (int j = partsRelativeTo.Length; j < partsTarget.Length; ++j)
                        {
                            output.Add(partsTarget[j]);
                        }
                    }
                }
            }
            return string.Join("/", output);
        }

        public static string GetPlatformPath(string path)
        {
            if (Wax.Util.PlatformUtil.IsWindows)
            {
                path = path.Replace('/', '\\');
            }
            else
            {
                path = path.Replace('\\', '/');
            }
            return path;
        }

        public static string FinalizeTilde(string path)
        {
            if (Wax.Util.PlatformUtil.IsWindows || !path.StartsWith("~"))
            {
                return path;
            }

            string homedir = "/Users/" + System.Environment.UserName;

            return homedir + path.Substring(1);
        }

        public static string GetParentDirectory(string path)
        {
            return System.IO.Path.GetDirectoryName(path);
        }

        public static void DeleteFile(string path)
        {
            System.IO.File.Delete(path);
        }

        public static string GetTempDirectory()
        {
            return System.IO.Path.GetTempPath();
        }

        public static string GetAbsolutePathFromRelativeOrAbsolutePath(string path)
        {
            return GetAbsolutePathFromRelativeOrAbsolutePath(DiskUtil.GetCurrentDirectory(), path);
        }

        public static string GetAbsolutePathFromRelativeOrAbsolutePath(string dirForAbsoluteFallback, string path)
        {
            if (DiskUtil.IsAbsolute(path)) return path;

            return System.IO.Path.GetFullPath(
                System.IO.Path.Combine(
                    dirForAbsoluteFallback,
                    path));
        }
    }
}
