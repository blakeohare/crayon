namespace CommonUtil.Core
{
    public static class StringUtil
    {
        public static string JoinLines(params string[] lines)
        {
            return string.Join("\n", lines);
        }

        public static string[] Split(string value, string separator)
        {
            return value.Split(new string[] { separator }, System.StringSplitOptions.None);
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

    }
}
