using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Wax.Util
{
    public class ResourceLoader
    {
        private System.Reflection.Assembly assembly;
        private string assemblyPrefix = null;

        public ResourceLoader(System.Reflection.Assembly assembly)
        {
            this.assembly = assembly;
            string samplePath = assembly.GetManifestResourceNames().FirstOrDefault();
            if (samplePath != null)
            {
                this.assemblyPrefix = samplePath.Split('.')[0];
            }
        }

        public string[] GetFileList()
        {
            return this.assembly.GetManifestResourceNames()
                .Select(path =>
                {
                    List<string> parts = new List<string>(path.Split('.').Skip(1));
                    string last = parts[parts.Count - 1];
                    parts.RemoveAt(parts.Count - 1);
                    parts[parts.Count - 1] += "." + last;
                    return string.Join('/', parts);
                })
                .ToArray();
        }

        public string LoadString(string path, string newline)
        {
            string value = LoadString(path);
            if (value == null) return null;
            return value.Replace("\r\n", "\n").Replace('\r', '\n').Replace("\n", newline);
        }

        public string LoadString(string path)
        {
            byte[] bytes = LoadBytes(path);
            if (bytes == null) return null;
            if (bytes[0] == 239 && bytes[1] == 187 && bytes[2] == 191)
            {
                byte[] newBytes = new byte[bytes.Length - 3];
                Array.Copy(bytes, 3, newBytes, 0, newBytes.Length);
                bytes = newBytes;
            }
            return Encoding.UTF8.GetString(bytes);
        }

        public byte[] LoadBytes(string path)
        {
            if (this.assemblyPrefix == null) return null;
            string resourcePath = this.assemblyPrefix + "." + path.Replace('/', '.');
            System.IO.Stream stream = this.assembly.GetManifestResourceStream(resourcePath);
            if (stream == null) return null;
            List<byte> output = new List<byte>();
            byte[] buffer = new byte[500];
            int bytesRead;
            do
            {
                bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == buffer.Length)
                {
                    output.AddRange(buffer);
                }
                else
                {
                    for (int i = 0; i < bytesRead; i++)
                    {
                        output.Add(buffer[i]);
                    }
                }
            } while (bytesRead > 0);
            return output.ToArray();
        }
    }
}
