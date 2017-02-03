using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;

namespace Crayon
{
    internal static class LegacyUtil
    {
        public static byte[] ReadResourceBytesInternally(string path)
        {
            return Util.ReadAssemblyFileBytes(typeof(Resources.ResourceAssembly).Assembly, path);
        }
        
        public static string ReadResourceFileInternally(string path)
        {
            return Util.ReadAssemblyFileText(typeof(Resources.ResourceAssembly).Assembly, path);
        }

        public static string ReadInterpreterFileInternally(string path)
        {
            return Util.ReadAssemblyFileText(typeof(Interpreter.InterpreterAssembly).Assembly, path);
        }
    }
}
