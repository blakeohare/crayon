using System;
using System.Collections.Generic;

namespace %%%PROJECT_ID%%%.Library.FileIOCommon
{
    internal static class FileIOCommonHelper
    {
        public static bool IsWindows()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                case PlatformID.Xbox:
                    return true;
                default:
                    return false;
            }
        }

        public static string GetUserDirectory()
        {
            if (IsWindows())
            {
                return Environment.GetEnvironmentVariable("USERPROFILE");
            }
            return "~";
        }

        public static string GetCurrentDirectory()
        {
            return System.IO.Directory.GetCurrentDirectory();
        }

        private static string NormalizePath(string path)
        {
            if (path.Length == 2 && path[1] == ':')
            {
                path += "/";
            }
            if (IsWindows())
            {
                path = path.Replace('/', '\\');
            }
            return path;
        }

        public static int GetDirectoryList(string path, bool includeFullPath, List<string> output)
        {
            int trimLength = path.Length + 1;
            path = NormalizePath(path);
            List<string> items = new List<string>();
            try
            {
                items.AddRange(System.IO.Directory.GetFiles(path));
                items.AddRange(System.IO.Directory.GetDirectories(path));
            }
            catch (System.IO.DirectoryNotFoundException)
            {
                return 4;
            }
            catch (System.IO.PathTooLongException)
            {
                return 5;
            }
            catch (Exception)
            {
                return 1; // UNKNOWN ERROR
            }

            if (!includeFullPath)
            {
                for (int i = 0; i < items.Count; ++i)
                {
                    items[i] = items[i].Substring(trimLength);
                }
            }

            items.Sort();
            output.AddRange(items);

            return 0;
        }

        public static int FileRead(string path, bool isBytes, string[] stringOut, Value[] integers, List<Value> byteOutput)
        {
            try
            {
                if (isBytes)
                {
                    byte[] bytes = System.IO.File.ReadAllBytes(path);
                    int length = bytes.Length;
                    for (int i = 0; i < length; ++i)
                    {
                        byteOutput.Add(integers[bytes[i]]);
                    }
                }
                else
                {
                    stringOut[0] = System.IO.File.ReadAllText(path);
                }
            }
            catch (System.IO.FileNotFoundException)
            {
                return 4;
            }
            catch (System.IO.DirectoryNotFoundException)
            {
                return 4;
            }
            catch (System.IO.PathTooLongException)
            {
                return 5;
            }
            catch (Exception)
            {
                return 1;
            }
            return 0;
        }
    }
}
