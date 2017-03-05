using System.Collections.Generic;
using System.Linq;

namespace Common
{
    public static class Util
    {
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
                for (int i = 0; i < 20 && value != 0; ++i)
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

        public static string ReadFileExternally(string path, bool canonicalizeNewlines)
        {
            string contents = TrimBomIfPresent(System.IO.File.ReadAllText(path));

            if (canonicalizeNewlines)
            {
                contents = contents.Replace("\r\n", "\n").Replace('\r', '\n');
            }

            return contents;
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
            // Ick. Drops the encoding. TODO: fix this
            return TrimBomIfPresent(string.Join("", bytes.Select<byte, char>(b => (char)b)));
        }

        private static string TrimBomIfPresent(string text)
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

        public static byte[] ReadAssemblyFileBytes(System.Reflection.Assembly assembly, string path, bool failSilently)
        {
            string canonicalizedPath = path.Replace('/', '.');
#if WINDOWS
            // a rather odd difference...
            canonicalizedPath = canonicalizedPath.Replace('-', '_');
#endif
            string fullPath = assembly.GetName().Name + "." + canonicalizedPath;
            System.IO.Stream stream = assembly.GetManifestResourceStream(fullPath);
            if (stream == null)
            {
                if (failSilently)
                {
                    return null;
                }

                throw new System.Exception(path + " not marked as an embedded resource.");
            }
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

        // TODO: use <K, V>
        public static Dictionary<string, string> FlattenDictionary(Dictionary<string, string> bottom, Dictionary<string, string> top)
        {
            if (bottom.Count == 0) return new Dictionary<string, string>(top);

            Dictionary<string, string> output = new Dictionary<string, string>(bottom);
            foreach (string key in top.Keys)
            {
                output[key] = top[key];
            }
            return output;
        }
        
        public static Dictionary<K, V> MergeDictionaries<K, V>(params Dictionary<K, V>[] dictionaries)
        {
            Dictionary<K, V> output = new Dictionary<K, V>();
            foreach (Dictionary<K, V> dict in dictionaries)
            {
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
    }
}
