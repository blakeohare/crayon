using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    public static class GuidHelper
    {
        private static readonly string CSHARP_UPPER = "HHHHHHHH-HHHH-HHHH-HHHH-HHHHHHHHHHHH";
        private static readonly string CSHARP_LOWER = "hhhhhhhh-hhhh-hhhh-hhhh-hhhhhhhhhhhh";
        private static readonly string XCODE_PROJ = "HHHHHHHHHHHHHHHHHHHHHHHH";

        private static readonly string HEX_UPPER = "0123456789ABCDEF";
        private static readonly string HEX_LOWER = "0123456789abcdef";

        public static string GenerateCSharpGuid(string seed, string salt)
        {
            return GenerateGuid(seed, salt + "-cs", CSHARP_UPPER.ToCharArray());
        }

        public static string GenerateXProjUuid(string seed, string salt)
        {
            return GenerateGuid(seed, salt + "-xc", XCODE_PROJ.ToCharArray());
        }

        private static readonly Random random = new Random((int)(DateTime.Now.Ticks % 2000000000));

        public static string GetRandomSeed()
        {
            return DateTime.Now.Ticks.ToString() + "," + random.NextDouble();
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
                }			}

            return new String(format);
        }
    }
}
