using System;
using System.Collections.Generic;
using System.Text;

namespace Build
{
    internal static class BoolParser
    {
        public static bool FlexibleParse(string value)
        {
            if (value == null) return false;

            switch (value.ToLowerInvariant())
            {
                case "true":
                case "t":
                case "1":
                case "yes":
                case "y":
                    return true;
                default:
                    return false;
            }
        }
    }
}
