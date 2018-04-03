namespace Common
{
    public static class Utf8Base64
    {
        public static string ToBase64(string value)
        {
            byte[] utf8Bytes = System.Text.Encoding.UTF8.GetBytes(value);
            string encodedValue = System.Convert.ToBase64String(utf8Bytes);
            return encodedValue;
        }

        public static string FromBase64(string encodedValue)
        {
            byte[] utf8Bytes = System.Convert.FromBase64String(encodedValue);
            string value = System.Text.Encoding.UTF8.GetString(utf8Bytes);
            return value;
        }
    }
}
