using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wax;

namespace Interpreter
{
    internal class FileBasedResourceReader : ResourceReader
    {
        private System.IO.Stream GetResourceStream(string path)
        {
            // TODO: this needs to change
            System.Reflection.Assembly assembly = typeof(ResourceReader).Assembly;
            string assemblyPrefix = assembly.GetManifestResourceNames()[0].Split('.')[0];
            path = assemblyPrefix + "." + path.Replace('\\', '.').Replace('/', '.').TrimStart('.');
            return assembly.GetManifestResourceStream(path);
        }

        // C# does not like it when you access resources from different threads.
        private object resMutex = new object();

        public override List<byte> ReadBytes(string path)
        {
            lock (resMutex)
            {
                byte[] buffer = new byte[500];
                System.IO.Stream stream = GetResourceStream(path);
                if (stream == null)
                {
                    return null;
                }
                StringBuilder sb = new StringBuilder();
                List<byte> output = new List<byte>();

                int bytesRead = 1;
                while (bytesRead > 0)
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == buffer.Length)
                    {
                        output.AddRange(buffer);
                    }
                    else
                    {
                        for (int i = 0; i < bytesRead; ++i)
                        {
                            output.Add(buffer[i]);
                        }
                        bytesRead = 0;
                    }
                }
                return output;
            }
        }
    }

    internal class InMemoryResourceReader : ResourceReader
    {
        private Dictionary<string, FileOutput> rawFileMetadata = new Dictionary<string, FileOutput>();
        private Dictionary<string, List<byte>> fileBytes = new Dictionary<string, List<byte>>();

        public InMemoryResourceReader(string[] fileNames, FileOutput[] files)
        {
            for (int i = 0; i < files.Length; i++)
            {
                rawFileMetadata[fileNames[i]] = files[i];
            }
        }

        public override List<byte> ReadBytes(string path)
        {
            if (!this.fileBytes.ContainsKey(path))
            {
                if (!this.rawFileMetadata.ContainsKey(path))
                {
                    return null;
                }

                FileOutput file = this.rawFileMetadata[path];
                this.fileBytes[path] = new List<byte>(file.GetFinalBinaryContent());
            }
            return this.fileBytes[path];
        }
    }

    internal abstract class ResourceReader
    {
        public abstract List<byte> ReadBytes(string path);

        public string ReadMetadata(string path, bool returnEmptyOnFail)
        {
            string value = ReadTextResource(path);
            if (value == null && !returnEmptyOnFail)
            {
                throw new System.Exception();
            }
            return value;
        }

        public string ReadTextResource(string path)
        {
            IList<byte> bytes = ReadBytes("res/" + path);
            if (bytes == null) return null;
            bool hasBom = bytes.Count >= 3 && bytes[0] == 239 && bytes[1] == 187 && bytes[2] == 191;
            StringBuilder output = new StringBuilder(bytes.Count);
            output.Append(bytes.Skip<byte>(hasBom ? 3 : 0).Select<byte, char>(b => (char)b).ToArray<char>());
            return output.ToString();
        }

        public UniversalBitmap ReadImageResource(string path)
        {
            IList<byte> data = ReadBytes("res/" + path);
            if (data == null)
            {
                return null;
            }
            return new UniversalBitmap(data.ToArray());
        }

        public byte[] ReadSoundResource(string path)
        {
            List<byte> bytes = ReadBytes("res/" + path);
            return bytes == null ? null : bytes.ToArray();
        }

        public byte[] ReadFontResource(string path)
        {
            List<byte> bytes = ReadBytes("res/" + path);
            return bytes == null ? null : bytes.ToArray();
        }
    }
}
