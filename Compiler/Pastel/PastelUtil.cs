using System.Collections.Generic;

namespace Pastel
{
    internal static class PastelUtil
    {
        public static string ConvertCharToCharConstantCode(char value)
        {
            switch (value)
            {
                case '\n': return "'\\n'";
                case '\r': return "'\\r'";
                case '\0': return "'\\0'";
                case '\t': return "'\\t'";
                case '\\': return "'\\\\'";
                case '\'': return "'\\''";
                default: return "'" + value + "'";
            }
        }

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

        public static string ConvertStringTokenToValue(string tokenValue)
        {
            List<string> output = new List<string>();
            for (int i = 1; i < tokenValue.Length - 1; ++i)
            {
                char c = tokenValue[i];
                if (c == '\\')
                {
                    c = tokenValue[++i];
                    switch (c)
                    {
                        case '\\': output.Add("\\"); break;
                        case 'n': output.Add("\n"); break;
                        case 'r': output.Add("\r"); break;
                        case 't': output.Add("\t"); break;
                        case '\'': output.Add("'"); break;
                        case '"': output.Add("\""); break;
                        case '0': output.Add("\0"); break;
                        default: return null;
                    }
                }
                else
                {
                    output.Add("" + c);
                }
            }
            return string.Join("", output);
        }

        /// <summary>
        /// Override C#'s default float to string behavior of not display the decimal portion if it's a whole number.
        /// </summary>
        public static string FloatToString(double value)
        {
            string output = value.ToString();
            if (output.Contains("E-"))
            {
                output = "0.";
                if (value < 0)
                {
                    value = -value;
                    output = "-" + output;
                }
                value *= 15;
#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                for (int i = 0; i < 20 && value != 0; ++i)
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator
                {
                    output += (int)(value % 10);
                    value = value % 1;
                    value *= 10;
                }
            }

            if (!output.Contains("."))
            {
                output += ".0";
            }
            return output;
        }

        public static string ReadAssemblyFileText(System.Reflection.Assembly assembly, string path)
        {
            return ReadAssemblyFileText(assembly, path, false);
        }

        public static string ReadAssemblyFileText(System.Reflection.Assembly assembly, string path, bool failSilently)
        {
            byte[] bytes = PastelUtil.ReadAssemblyFileBytes(assembly, path, failSilently);
            if (bytes == null)
            {
                return null;
            }
            return MysteryTextDecoder.DecodeArbitraryBytesAsAppropriatelyAsPossible(bytes);
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
                    if (resource.Contains("_"))
                    {
                        // this is silly, but VS gets confused easily, even when marked as embedded resources.
                        lookupName = lookupName
                            .Replace("_cs.txt", ".cs")
                            .Replace("_csproj.txt", ".csproj")
                            .Replace("_sln.txt", ".sln")
                            .Replace("_java.txt", ".java")
                            .Replace("_js.txt", ".js")
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

        public static void IndentLines(string indention, List<string> lines)
        {
            int length = lines.Count;
            for (int i = 0; i < length; ++i)
            {
                lines[i] = (indention + lines[i]).TrimEnd();
            }
        }
    }
}
