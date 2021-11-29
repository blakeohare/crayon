﻿using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Interpreter
{
    internal static class ResourceReader
    {
        public static string ReadMetadata(string path, bool returnEmptyOnFail)
        {
            string value = ReadTextResource(path);
            if (value == null && !returnEmptyOnFail)
            {
                throw new System.Exception();
            }
            return value;
        }

        public static string ReadTextResource(string path)
        {
            IList<byte> bytes = ReadBytes("Resources/" + path);
            if (bytes == null) return null;
            bool hasBom = bytes.Count >= 3 && bytes[0] == 239 && bytes[1] == 187 && bytes[2] == 191;
            StringBuilder output = new StringBuilder(bytes.Count);
            output.Append(bytes.Skip<byte>(hasBom ? 3 : 0).Select<byte, char>(b => (char)b).ToArray<char>());
            return output.ToString();
        }

        public static UniversalBitmap ReadImageResource(string path)
        {
            IList<byte> data = ReadBytes("Resources/" + path);
            if (data == null)
            {
                return null;
            }
            return new UniversalBitmap(data.ToArray());
        }

        public static byte[] ReadSoundResource(string path)
        {
            List<byte> bytes = ReadBytes("Resources/" + path);
            return bytes == null ? null : bytes.ToArray();
        }

        public static UniversalBitmap ReadIconResource(string path)
        {
            return ReadImageResource("icons/" + path);
        }

        public static byte[] ReadFontResource(string path)
        {
            List<byte> bytes = ReadBytes("Resources/" + path);
            return bytes == null ? null : bytes.ToArray();
        }

        // C# does not like it when you access resources from different threads.
        private static object resMutex = new object();

        public static System.IO.Stream GetResourceStream(string path)
        {
            // TODO: this needs to change
            System.Reflection.Assembly assembly = typeof(ResourceReader).Assembly;
            string assemblyPrefix = assembly.GetManifestResourceNames()[0].Split('.')[0];
            path = assemblyPrefix + "." + path.Replace('\\', '.').Replace('/', '.').TrimStart('.');
            return assembly.GetManifestResourceStream(path);
        }

        public static List<byte> ReadBytes(string path)
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
}
