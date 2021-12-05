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
            if (fileNames != null)
            {
                for (int i = 0; i < files.Length; i++)
                {
                    rawFileMetadata[fileNames[i]] = files[i];
                }
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

        public string ReadBinaryResource(string path, bool isText, bool useBase64, List<Interpreter.Structs.Value> bytesOut, Interpreter.Structs.Value[] integers)
        {
            byte[] bytes;
            if (isText)
            {
                string text = this.ReadTextResource(path);
                bytes = System.Text.Encoding.UTF8.GetBytes(text);
            } else
            {
                bytes = this.ReadBytes("res/" + path).ToArray();
            }

            if (useBase64) return System.Convert.ToBase64String(bytes);

            int len = bytes.Length;
            for (int i = 0; i < len; i++)
            {
                bytesOut.Add(integers[bytes[i]]);
            }
            return null;
        }

        public string ReadTextResource(string path)
        {
            List<byte> byteList = ReadBytes("res/" + path);
            if (byteList == null) return null;
            bool hasBom = byteList.Count >= 3 && byteList[0] == 239 && byteList[1] == 187 && byteList[2] == 191;
            byte[] byteArray = (hasBom ? byteList.Skip(3) : byteList).ToArray();
            try
            {
                return System.Text.Encoding.UTF8.GetString(byteArray);
            }
            catch (System.Text.DecoderFallbackException)
            {
                return null;
            }
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
