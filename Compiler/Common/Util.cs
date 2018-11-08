using System.Collections.Generic;
using System.Linq;

namespace Common
{
    public static class Util
    {
        public static string[] StringSplit(string value, string separator)
        {
            return value.Split(new string[] { separator }, System.StringSplitOptions.None);
        }

        public static bool StringToBool(string value)
        {
            if (value == null) return false;

            switch (value.ToLower())
            {
                case "true":
                case "t":
                case "1":
                case "yes":
                case "y":
                case "yup":
                case "aye":
                case "youknowit":
                case "youbetcha":
                    return true;
                default:
                    return false;
            }
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

            if (!output.Contains('.'))
            {
                output += ".0";
            }
            return output;
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

        public static string ConvertByteArrayToBase64(byte[] bytes)
        {
            return System.Convert.ToBase64String(bytes, System.Base64FormattingOptions.None);
        }

        public static string ConvertStringToAlphanumerics(string value)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (char c in value)
            {
                if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9'))
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
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
            return MysteryTextDecoder.DecodeArbitraryBytesAsAppropriatelyAsPossible(bytes);
        }

        public static string TrimBomIfPresent(string text)
        {
            return (text.Length >= 3 && text[0] == 239 && text[1] == 187 && text[2] == 191)
                ? text.Substring(3)
                : text;
        }

        private static readonly byte[] BUFFER = new byte[1000];

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

        public static void MergeDictionaryInto<K, V>(Dictionary<K, V> newDict, Dictionary<K, V> mergeIntoThis)
        {
            foreach (KeyValuePair<K, V> kvp in newDict)
            {
                mergeIntoThis[kvp.Key] = kvp.Value;
            }
        }

        public static Dictionary<K, V> MergeDictionaries<K, V>(params Dictionary<K, V>[] dictionaries)
        {
            if (dictionaries.Length == 0) return new Dictionary<K, V>();
            if (dictionaries.Length == 1) return new Dictionary<K, V>(dictionaries[0]);
            if (dictionaries.Length == 2)
            {
                // Super common.
                if (dictionaries[0].Count == 0) return new Dictionary<K, V>(dictionaries[1]);
                if (dictionaries[1].Count == 0) return new Dictionary<K, V>(dictionaries[0]);
            }

            Dictionary<K, V> output = new Dictionary<K, V>(dictionaries[0]);
            for (int i = 0; i < dictionaries.Length; ++i)
            {
                Dictionary<K, V> dict = dictionaries[i];
                foreach (K k in dict.Keys)
                {
                    output[k] = dict[k];
                }
            }
            return output;
        }

        private static readonly System.IFormatProvider EN_US =
            System.Globalization.CultureInfo.GetCultureInfo("en-us");
        private static readonly System.Globalization.NumberStyles DOUBLE_FLAG =
            (System.Globalization.NumberStyles)(
            (int)System.Globalization.NumberStyles.AllowDecimalPoint |
            (int)System.Globalization.NumberStyles.AllowLeadingSign |
            (int)System.Globalization.NumberStyles.AllowLeadingWhite |
            (int)System.Globalization.NumberStyles.AllowTrailingWhite |
            (int)System.Globalization.NumberStyles.Float |
            (int)System.Globalization.NumberStyles.Integer);
        public static bool ParseDouble(string value, out double output)
        {
            // Parsing text data should use local info, but this is for parsing code.
            // As this is not supposed to be localized yet, only allow US decimals.
            return double.TryParse(value, DOUBLE_FLAG, EN_US, out output);
        }

        public static string MultiplyString(string str, int count)
        {
            switch (count)
            {
                case 0: return "";
                case 1: return str;
                case 2: return str + str;
                default:
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    while (count-- > 0)
                    {
                        sb.Append(str);
                    }
                    return sb.ToString();
            }
        }

        public static string JoinLines(params string[] lines)
        {
            return string.Join("\n", lines);
        }

        public static int? ParseIntWithErrorNullOkay(string value, string error)
        {
            if (value == null) return null;
            int output;
            if (int.TryParse(value, out output))
            {
                return output;
            }
            throw new System.InvalidOperationException(error);
        }

        public static Dictionary<string, string> MakeReplacementStringsJsonSafe(Dictionary<string, string> values)
        {
            Dictionary<string, string> output = new Dictionary<string, string>();
            foreach (string key in values.Keys)
            {
                output[key] = values[key].Replace("\"", "\\\"");
            }
            return output;
        }

        // A check for 100% absolute floating-point equality is sometimes needed.
        public static bool FloatEqualsNoEpislon(double a, double b)
        {
#pragma warning disable RECS0018
            return a == b;
#pragma warning restore RECS0018
        }
    }
}
