using System.Collections.Generic;
using System.Linq;

namespace Parser.Localization
{
    public static class ResourceReader
    {
        private static Dictionary<string, string> data = new Dictionary<string, string>();
        private static string prefix = null;

        public static string GetFile(string path)
        {
            if (!data.ContainsKey(path))
            {
                System.Reflection.Assembly asm = typeof(Locale).Assembly;
                if (prefix == null)
                {
                    string[] names = asm.GetManifestResourceNames();
                    prefix = names[0].Split(".Languages.")[0];
                }
                System.IO.Stream stream = asm.GetManifestResourceStream(prefix + ".Languages." + path.Replace('/', '.'));
                if (stream == null)
                {
                    data[path] = null;
                    return null;
                }
                byte[] buffer = new byte[500];
                int bytesRead = 1;
                List<byte> bytes = new List<byte>();
                while (bytesRead > 0)
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == buffer.Length)
                    {
                        bytes.AddRange(buffer);
                    }
                    else
                    {
                        for (int i = 0; i < bytesRead; i++)
                        {
                            bytes.Add(buffer[i]);
                        }
                    }
                }
                byte[] arr = bytes[0] == 239 && bytes[1] == 187 && bytes[2] == 191 ? (bytes.Skip(3).ToArray()) : bytes.ToArray();
                string content = System.Text.Encoding.UTF8.GetString(arr);
                data[path] = content;
            }
            return data[path];
        }
    }
}
