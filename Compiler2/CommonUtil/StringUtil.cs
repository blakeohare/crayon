namespace CommonUtil
{
    public static class StringUtil
    {
        public static string JoinLines(params string[] lines)
        {
            return string.Join("\n", lines);
        }

        private static readonly string[] SEP = new string[1];

        public static string[] SplitOnce(string value, string separator)
        {
            SEP[0] = separator;
            return value.Split(SEP, 2, System.StringSplitOptions.None);
        }

        public static string[] Split(string value, string separator)
        {
            SEP[0] = separator;
            return value.Split(SEP, System.StringSplitOptions.None);
        }

        public static string[] SplitRemoveEmpty(string value, string separator)
        {
            SEP[0] = separator;
            return value.Split(SEP, System.StringSplitOptions.RemoveEmptyEntries);
        }

        public static string Multiply(string str, int count)
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

        public static string TrimBomIfPresent(string text)
        {
            return (text.Length >= 3 && text[0] == 239 && text[1] == 187 && text[2] == 191)
                ? text.Substring(3)
                : text;
        }

        public static string FilterStringToAlphanumerics(string value)
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

        public static byte[] ToUtf8Bytes(string value)
        {
            return System.Text.Encoding.UTF8.GetBytes(value);
        }

        public static string FromUtf8Bytes(byte[] bytes, int startIndex, int length)
        {
            return System.Text.Encoding.UTF8.GetString(bytes, startIndex, length);
        }
    }
}
