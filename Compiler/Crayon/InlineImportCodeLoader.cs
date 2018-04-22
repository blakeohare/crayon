using Common;

namespace Crayon
{
    internal class InlineImportCodeLoader : Pastel.IInlineImportCodeLoader
    {
        private static readonly System.Reflection.Assembly INTERPRETER_ASSEMBLY = typeof(Interpreter.InterpreterAssembly).Assembly;

        public string LoadCode(string path)
        {
            return Util.ReadAssemblyFileText(INTERPRETER_ASSEMBLY, path);
        }
    }
}
