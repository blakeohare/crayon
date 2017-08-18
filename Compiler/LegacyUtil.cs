using Common;

namespace Crayon
{
    internal static class LegacyUtil
    {
        public static string ReadInterpreterFileInternally(string path)
        {
            return Util.ReadAssemblyFileText(typeof(Interpreter.InterpreterAssembly).Assembly, path);
        }
    }
}
