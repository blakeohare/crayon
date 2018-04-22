using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pastel
{
    internal static class PastelUtil
    {
        public static string ConvertCharToCharConstantCode(char value)
        {
            switch (value)
            {
                case '\n': return "'\\n'";
                case '\r': return "'\\r'";
                case '\0': return "'\\0'";
                case '\t': return "'\\t'";
                case '\\': return "'\\\\'";
                case '\'': return "'\\''";
                default: return "'" + value + "'";
            }
        }
    }
}
