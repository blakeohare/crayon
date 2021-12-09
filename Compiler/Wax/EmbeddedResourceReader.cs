using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Wax
{
    public class EmbeddedResourceReader
    {
        private System.Reflection.Assembly assembly;
        private string prefix;
        private string[] paths;

        public EmbeddedResourceReader(System.Reflection.Assembly assembly)
        {
            this.assembly = assembly;
            this.paths = assembly.GetManifestResourceNames();
            this.prefix = this.paths.Length == 0 ? "" : this.paths[0].Split('.')[0];
        }

        public string[] ListFiles(string pathPrefix)
        {
            return this.paths
                .Where(path => path.StartsWith((this.prefix + '/' + pathPrefix + '/').Replace('/', '.').Replace("..", ".")))
                .Select(path => path.Substring(this.prefix.Length + 1))
                .Select(path =>
                {
                    Stack<string> parts = new Stack<string>(path.Split('.'));
                    string last = parts.Pop();
                    if (parts.Count == 0) return last;
                    parts.Push(parts.Pop() + "." + last);
                    return string.Join('/', parts.ToArray().Reverse());
                })
                .ToArray();
        }

        public byte[] GetBytes(string path)
        {
            List<byte> bytes = this.GetByteList(path);
            if (bytes == null) return null;
            return bytes.ToArray();
        }

        public string GetText(string path)
        {
            List<byte> bytes = this.GetByteList(path);
            if (bytes == null) return null;
            IEnumerable<byte> trimmedBytes = (bytes.Count >= 3 && bytes[0] == 239 && bytes[1] == 187 && bytes[2] == 191) ? bytes.Skip(3) : bytes;
            try
            {
                return Encoding.UTF8.GetString(trimmedBytes.ToArray());
            }
            catch (DecoderFallbackException)
            {
                return null;
            }
        }

        private List<byte> GetByteList(string path)
        {
            System.IO.Stream stream = this.assembly.GetManifestResourceStream((this.prefix + "/" + path).Replace('/', '.'));
            if (stream == null) return null;

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
            return bytes;
        }
    }
}
