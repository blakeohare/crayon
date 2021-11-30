using System;
using System.Collections.Generic;
using System.Linq;

namespace CommonUtil.Random
{
    public static class IdGenerator
    {
        private static readonly string HEX_DIGITS_32 = "hhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhh";
        private static readonly string CSHARP_UPPER = "HHHHHHHH-HHHH-HHHH-HHHH-HHHHHHHHHHHH";
        private static readonly string XCODE_PROJ = "HHHHHHHHHHHHHHHHHHHHHHHH";
        private static readonly string TEMP_DIR = "crayon-HHHHHHHHHHHHHHHH";

        private static readonly string HEX_UPPER = "0123456789ABCDEF";
        private static readonly string HEX_LOWER = "0123456789abcdef";

        public static string Generate32HexDigits(string seed, string salt)
        {
            return GenerateGuid(seed, salt + "-random-hex", HEX_DIGITS_32.ToCharArray());
        }

        public static string GenerateTempDirName(string seed, string salt)
        {
            return GenerateGuid(seed, salt + "-tmp-dir", TEMP_DIR.ToCharArray());
        }

        public static string GenerateCSharpGuid(string seed, string salt)
        {
            return GenerateGuid(seed, salt + "-cs", CSHARP_UPPER.ToCharArray());
        }

        public static string GenerateXProjUuid(string seed, string salt)
        {
            return GenerateGuid(seed, salt + "-xc", XCODE_PROJ.ToCharArray());
        }

        private static readonly System.Random random = new System.Random((int)(System.DateTime.UtcNow.Ticks % Int32.MaxValue));

        public static string GetRandomSeed()
        {
            return System.DateTime.UtcNow.Ticks + "," + random.NextDouble();
        }

        private static string GenerateGuid(string seed, string salt, char[] format)
        {
            int seedSuffix = 0;
            Stack<byte> bytes = new Stack<byte>();
            for (int i = 0; i < format.Length; ++i)
            {
                switch (format[i])
                {
                    case 'H':
                    case 'h':
                        if (bytes.Count == 0)
                        {
                            byte[] seedBytes = (seed + salt + seedSuffix++).ToCharArray().Select<char, byte>(c => (byte)c).ToArray();
                            bytes = new Stack<byte>(System.Security.Cryptography.SHA1.Create().ComputeHash(seedBytes));
                        }
                        format[i] = (format[i] == 'h' ? HEX_LOWER : HEX_UPPER)[bytes.Pop() & 15];
                        break;

                    default:
                        break;
                }
            }

            return new String(format);
        }
    }
}
