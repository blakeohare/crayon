using System;
using System.Text;

namespace Disk
{
    internal static class Util
    {
        private static Random rnd = new Random();

        public static string GenerateId(int length)
        {
            string chars = "0123456789abcdefghijklmnopqrstuvwxyz";
            StringBuilder sb = new StringBuilder();
            while (length --> 0)
            {
                int i = rnd.Next(chars.Length);
                sb.Append(chars[i]);
            }
            return sb.ToString();
        }
    }
}
