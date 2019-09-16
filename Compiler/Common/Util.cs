using CommonUtil.Text;
using System.Collections.Generic;
using System.Linq;

namespace Common
{
    public static class Util
    {
        public static string ConvertStringValueToCode(string rawValue)
        {
            return ConvertStringValueToCode(rawValue, false);
        }

        private const char ASCII_MAX = (char)127;
        private static readonly string[] HEX_CHARS = "0 1 2 3 4 5 6 7 8 9 a b c d e f".Split(' ');

        public static string ConvertStringValueToCode(string rawValue, bool includeUnicodeEscape)
        {
            int uValue, d1, d2, d3, d4;
            List<string> output = new List<string>() { "\"" };
            foreach (char c in rawValue)
            {
                if (includeUnicodeEscape && c > ASCII_MAX)
                {
                    uValue = c;
                    output.Add("\\u");
                    d1 = uValue & 15;
                    d2 = (uValue >> 4) & 15;
                    d3 = (uValue >> 8) & 15;
                    d4 = (uValue >> 12) & 15;
                    output.Add(HEX_CHARS[d4]);
                    output.Add(HEX_CHARS[d3]);
                    output.Add(HEX_CHARS[d2]);
                    output.Add(HEX_CHARS[d1]);
                }
                else
                {
                    switch (c)
                    {
                        case '"': output.Add("\\\""); break;
                        case '\n': output.Add("\\n"); break;
                        case '\r': output.Add("\\r"); break;
                        case '\0': output.Add("\\0"); break;
                        case '\t': output.Add("\\t"); break;
                        case '\\': output.Add("\\\\"); break;
                        default: output.Add("" + c); break;
                    }
                }
            }
            output.Add("\"");

            return string.Join("", output);
        }

        public static string ReadAssemblyFileText(System.Reflection.Assembly assembly, string path)
        {
            return ReadAssemblyFileText(assembly, path, false);
        }

        public static string ReadAssemblyFileText(System.Reflection.Assembly assembly, string path, bool failSilently)
        {
            byte[] bytes = Util.ReadAssemblyFileBytes(assembly, path, failSilently);
            if (bytes == null)
            {
                return null;
            }
            return UniversalTextDecoder.Decode(bytes);
        }

        public static byte[] GetIconFileBytesFromImageFile(string filePath)
        {
            // TODO: scaling
#if OSX
            filePath = filePath.Replace('\\', '/');
#endif
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            System.Drawing.Bitmap bmp = (System.Drawing.Bitmap)System.Drawing.Image.FromFile(filePath);
            System.Drawing.Icon.FromHandle(bmp.GetHicon()).Save(ms);
            byte[] bytes = ms.ToArray();
            return bytes;
        }

        public static byte[] ReadAssemblyFileBytes(System.Reflection.Assembly assembly, string path)
        {
            return ReadAssemblyFileBytes(assembly, path, false);
        }

        private static Dictionary<System.Reflection.Assembly, Dictionary<string, string>> caseInsensitiveLookup =
            new Dictionary<System.Reflection.Assembly, Dictionary<string, string>>();

        private static readonly byte[] BUFFER = new byte[1000];

        public static byte[] ReadAssemblyFileBytes(System.Reflection.Assembly assembly, string path, bool failSilently)
        {
            string canonicalizedPath = path.Replace('/', '.');
#if WINDOWS
            // a rather odd difference...
            canonicalizedPath = canonicalizedPath.Replace('-', '_');
#endif
            string assemblyName = assembly.GetName().Name.ToLower();

            Dictionary<string, string> nameLookup;
            if (!caseInsensitiveLookup.TryGetValue(assembly, out nameLookup))
            {
                nameLookup = new Dictionary<string, string>();
                caseInsensitiveLookup[assembly] = nameLookup;
                foreach (string resource in assembly.GetManifestResourceNames())
                {
                    string lookupName = resource.ToLower();
                    if (resource.Contains('_'))
                    {
                        // this is silly, but VS gets confused easily, even when marked as embedded resources.
                        lookupName = lookupName
                            .Replace("_cs.txt", ".cs")
                            .Replace("_csproj.txt", ".csproj")
                            .Replace("_sln.txt", ".sln")
                            .Replace("_java.txt", ".java")
                            .Replace("_js.txt", ".js")
                            .Replace("_php.txt", ".php")
                            .Replace("_py.txt", ".py")
                            .Replace("_xml.txt", ".xml");
                    }

                    nameLookup[lookupName] = resource;
                }
            }

            string fullPath = assembly.GetName().Name + "." + canonicalizedPath;
            if (!nameLookup.ContainsKey(fullPath.ToLower()))
            {
                if (failSilently)
                {
                    return null;
                }

                throw new System.Exception(path + " not marked as an embedded resource.");
            }

            System.IO.Stream stream = assembly.GetManifestResourceStream(nameLookup[fullPath.ToLower()]);
            List<byte> output = new List<byte>();
            int bytesRead = 1;
            while (bytesRead > 0)
            {
                bytesRead = stream.Read(BUFFER, 0, BUFFER.Length);
                if (bytesRead == BUFFER.Length)
                {
                    output.AddRange(BUFFER);
                }
                else
                {
                    for (int i = 0; i < bytesRead; ++i)
                    {
                        output.Add(BUFFER[i]);
                    }
                    bytesRead = 0;
                }
            }

            return output.ToArray();
        }
    }
}
