using System;
using System.Linq;
using System.Text;

namespace Common
{
    public static class CSharpHelper
    {
        public static string GenerateGuid(string optionalSeed, string salt)
        {
            string seed = optionalSeed ?? (DateTime.Now.Ticks.ToString() + "," + new Random().NextDouble());

            byte[] seedBytes = (seed + salt).ToCharArray().Select<char, byte>(c => (byte)c).ToArray();
            byte[] hash = System.Security.Cryptography.SHA1.Create().ComputeHash(seedBytes);


            StringBuilder output = new StringBuilder();
            for (int i = 0; i < 16; ++i)
            {
                if (i == 4 || i == 6 || i == 8 || i == 10)
                {
                    output.Append("-");
                }
                output.Append("0123456789ABCDEF"[hash[i] & 15]);
                output.Append("0123456789ABCDEF"[hash[i] >> 4]);
            }
            return output.ToString();
        }
    }
}
