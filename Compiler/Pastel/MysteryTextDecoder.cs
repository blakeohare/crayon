using System.Text;

namespace Pastel
{
    // Semi-efficiently auto-detect the text encoding from some bytes and convert to a string.
    public static class MysteryTextDecoder
    {
        public static string DecodeArbitraryBytesAsAppropriatelyAsPossible(byte[] bytes)
        {
            // Empty files are empty strings.
            if (bytes.Length == 0) return "";

            string value = null;

            if (bytes.Length >= 2 && bytes[0] == 254 && bytes[1] == 255)
            {
                // UTF16 Big Endian with Byte Order Mark
                value = DecodeAsUtf16(2, bytes, true);
                if (value != null) return value;
            }

            if (bytes.Length >= 2 && bytes[0] == 255 && bytes[1] == 254)
            {
                // UTF16 Little Endian with Byte Order Mark
                value = DecodeAsUtf16(2, bytes, false);
                if (value != null) return value;
            }

            // A text file is unlikely to have '\0' characters in it.
            if (bytes.Length >= 2 && bytes[0] == 0)
            {
                // UTF16 Big Endian with no Byte Order Mark
                value = DecodeAsUtf16(0, bytes, true);
                if (value != null) return value;
            }

            if (bytes.Length >= 2 && bytes[1] == 0)
            {
                // UTF16 Little Endian with no Byte Order Mark
                value = DecodeAsUtf16(0, bytes, false);
                if (value != null) return value;
            }

            if (bytes.Length >= 3 && bytes[0] == 239 && bytes[1] == 187 && bytes[2] == 191)
            {
                // A clearly provided UTF8 Byte Order Mark
                value = DecodeAsUtf8AndFailSilently(3, bytes);
                if (value != null) return value;
            }

            // Try UTF8 without a Byte Order Mark
            value = DecodeAsUtf8AndFailSilently(0, bytes);
            if (value != null) return value;

            // If all else fails, fall back to ANSI.
            return DecodeAsAnsi(bytes);
        }

        private static string DecodeAsUtf16(int offset, byte[] bytes, bool isBigEndian)
        {
            StringBuilder sb = new StringBuilder();
            int length = bytes.Length;
            if (length % 2 == 1) return null;
            int b1, b2, b3, b4;
            int b1Offset = isBigEndian ? 0 : 1;
            int b2Offset = isBigEndian ? 1 : 0;
            int c;
            for (int i = offset; i < length; i += 2)
            {
                b1 = bytes[i | b1Offset];
                b2 = bytes[i | b2Offset];

                if (b1 > 0xd8 && b1 < 0xdf)
                {
                    if (i + 2 >= length) return null;
                    if ((b1 & 0xfa) != 0xd8) return null;

                    i += 2;
                    b3 = bytes[i | b1Offset];
                    b4 = bytes[i | b2Offset];

                    if ((b3 & 0xfa) != 0xd9) return null;

                    c = ((b1 & 0x03) << 18) |
                        (b2 << 10) |
                        ((b3 & 0x03) << 8) |
                        b4;
                    c += 0x10000;

                    sb.Append(char.ConvertFromUtf32(c));
                }
                else
                {
                    sb.Append(char.ConvertFromUtf32(b2));
                }
            }
            return sb.ToString();
        }

        private static string DecodeAsUtf8AndFailSilently(int offset, byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();

            int length = bytes.Length;
            byte b, b2, b3, b4;
            int c;
            for (int i = offset; i < length; ++i)
            {
                b = bytes[i];
                if ((b & 0x80) == 0)
                {
                    sb.Append((char)b);
                }
                else if ((b & 0xe0) == 0xc0) // == 110xxxxx?
                {
                    if (i + 1 >= length) return null;
                    b2 = bytes[++i];
                    if ((b2 & 0xc0) != 0x80) return null;
                    c = (0x1f & b);
                    c = (c << 6) | (b2 & 0x3f);
                    sb.Append(char.ConvertFromUtf32(c));
                }
                else if ((b & 0xf0) == 0xe0) // == 1110xxxx?
                {
                    if (i + 2 >= length) return null;
                    b2 = bytes[++i];
                    b3 = bytes[++i];
                    if ((b2 & 0xc0) != 0x80) return null;
                    if ((b3 & 0xc0) != 0x80) return null;
                    c = 0x0f & b;
                    c = (c << 6) | (b2 & 0x3f);
                    c = (c << 6) | (b3 & 0x3f);
                    sb.Append(char.ConvertFromUtf32(c));
                }
                else if ((b & 0xf8) == 0xf0) // == 11110xxx?
                {
                    if (i + 3 >= length) return null;
                    b2 = bytes[++i];
                    b3 = bytes[++i];
                    b4 = bytes[++i];
                    if ((b2 & 0xc0) != 0x80) return null;
                    if ((b3 & 0xc0) != 0x80) return null;
                    if ((b4 & 0xc0) != 0x80) return null;
                    c = 0x0f & b;
                    c = (c << 6) | (b2 & 0x3f);
                    c = (c << 6) | (b3 & 0x3f);
                    c = (c << 6) | (b4 & 0x3f);
                    sb.Append(char.ConvertFromUtf32(c));
                }
                else
                {
                    return null;
                }
            }

            return sb.ToString();
        }

        private static string DecodeAsAnsi(byte[] bytes)
        {
            return Encoding.GetEncoding("iso-8859-1").GetString(bytes);
        }
    }
}
