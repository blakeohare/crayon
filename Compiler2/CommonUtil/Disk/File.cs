namespace CommonUtil.Disk
{
    public static class File
    {
        public static bool Exists(string path)
        {
            return System.IO.File.Exists(path);
        }

        public static byte[] ReadBytes(string path)
        {
            return System.IO.File.ReadAllBytes(path);
        }

        public static string[] ReadLines(string path)
        {
            return System.IO.File.ReadAllLines(path);
        }

        public static void Move(string source, string dest, bool overwriteOkay)
        {
            if (!Environment.Platform.IsWindows)
            {
                source = source.Replace('\\', '/');
                dest = dest.Replace('\\', '/');
            }
            if (overwriteOkay && System.IO.File.Exists(dest))
            {
                System.IO.File.Delete(dest);
            }
            System.IO.File.Move(source, dest);
        }

        public static void Copy(string source, string dest)
        {
            if (!Environment.Platform.IsWindows)
            {
                source = source.Replace('\\', '/');
            }

            try
            {
                System.IO.File.Copy(source, dest, true);
            }
            catch (System.IO.IOException ioe)
            {
                if (ioe.Message.Contains("it is being used by another process"))
                {
                    throw new System.InvalidOperationException("The file '" + dest + "' appears to be in use.");
                }
                else
                {
                    throw new System.InvalidOperationException("The file '" + dest + "' could not be copied to the output directory.");
                }
            }
        }
    }
}
