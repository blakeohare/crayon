namespace Interpreter.Libraries.NoriAlpha
{
    public class NoriHelper
    {
        public static object ShowFrame(string uiData)
        {
            throw new System.NotImplementedException();
        }

        public static bool CloseFrame(object nativeFrameHandle)
        {
            throw new System.NotImplementedException();
        }

        public static bool FlushUpdatesToFrame(object nativeFrameHandle, string uiData)
        {
            throw new System.NotImplementedException();
        }

        private static char[] HEX = "0123456789ABCDEF".ToCharArray();
        public static string EscapeStringHex(string original)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            int len = original.Length;
            char c;
            int a, b;
            for (int i = 0; i < len; ++i)
            {
                c = original[i];
                a = ((int)c >> 4) & 255;
                b = ((int)c & 15);
                sb.Append(HEX[a]);
                sb.Append(HEX[b]);
            }
            return sb.ToString();
        }
    }
}
