using System.Collections.Generic;

namespace Common
{
    public static class StringTokenUtil
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
    }
}
