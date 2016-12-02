using System;
using System.Collections.Generic;

namespace Crayon
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

        public static void WriteFileText(string path, string content)
        {
            path = NormalizePath(path);
            System.IO.File.WriteAllText(path, content, System.Text.Encoding.UTF8);
        }

        public static string ReadFileText(string path)
        {
            path = NormalizePath(path);
            return System.IO.File.ReadAllText(path);
        }

        public static void WriteFileBytes(string path, byte[] content)
        {
            path = NormalizePath(path);
            System.IO.File.WriteAllBytes(path, content);
        }

        public static byte[] ReadFileBytes(string path)
        {
            path = NormalizePath(path);
            return System.IO.File.ReadAllBytes(path);
        }

        public static void WriteFileImage(string path, SystemBitmap image)
        {
            image.Save(path);
        }

        public static string JoinPath(params string[] parts)
        {
            string output = NormalizePath(string.Join(DIR_SEP, parts));
            while (output.Contains(DIR_SEP + DIR_SEP))
            {
                output = output.Replace(DIR_SEP + DIR_SEP, DIR_SEP);
            }
            return output;
        }

        public static string[] DirectoryListFileNames(string dir)
        {
            return ListDirImpl(dir, true, false);
        }

        public static string[] DirectoryListFilePaths(string dir)
        {
            return ListDirImpl(dir, true, true);
        }

        public static string[] DirectoryListDirectoryNames(string dir)
        {
            return ListDirImpl(dir, false, false);
        }

        public static string[] DirectoryListDirectoryPaths(string dir)
        {
            return ListDirImpl(dir, false, true);
        }

        public static void EnsureParentFolderExists(string path)
        {
            path = path.Trim();
            if (path.Length > 0)
            {
                string folder = System.IO.Path.GetDirectoryName(path);
                if (folder.Length > 0 && !DirectoryExists(folder))
                {
                    EnsureParentFolderExists(folder);
                    CreateDirectory(folder);
                }
            }
        }

        public static void CreateDirectory(string path)
        {
            path = NormalizePath(path);
            System.IO.Directory.CreateDirectory(path);
        }

        public static bool DirectoryExists(string path)
        {
            path = NormalizePath(path);
            return System.IO.Directory.Exists(path);
        }

        public static void CopyFile(string source, string dest)
        {
#if OSX
			source = source.Replace('\\', '/');
#endif

            try
            {
                System.IO.File.Copy(source, dest, true);
            }
            catch (System.IO.IOException ioe)
            {
                if (ioe.Message.Contains("it is being used by another process"))
                {
                    throw new InvalidOperationException("The file '" + dest + "' appears to be in use. Please stop playing your game and try again.");
                }
                else
                {
                    throw new InvalidOperationException("The file '" + dest + "' could not be copied to the output directory.");
                }
            }
        }

        public static string[] GetAllAbsoluteFilePathsDescendentsOf(string absoluteRoot)
        {
            string[] output = GetAllFilePathsRelativeToRoot(absoluteRoot);
            for (int i = 0; i < output.Length; ++i)
            {
                output[i] = FileUtil.GetCanonicalizeUniversalPath(absoluteRoot + "/" + output[i]);
            }
            return output;
        }

        // ignores silly files such as thumbs.db, .ds_store, .svn/*, etc
        public static string[] GetAllFilePathsRelativeToRoot(string root)
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
                    GetAllFilePathsRelativeToRootImpl(JoinPath(currentRoot, directory), output);
                }
            }

            string[] files = ListDirImpl(currentRoot, true, false);
            foreach (string file in files)
            {
                if (!IGNORED_FILES.Contains(file))
                {
                    output.Add(JoinPath(currentRoot, file));
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
    }
}
