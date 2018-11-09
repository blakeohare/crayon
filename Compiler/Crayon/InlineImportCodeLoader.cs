using Common;

namespace Crayon
{
    internal class InlineImportCodeLoader : Pastel.IInlineImportCodeLoader
    {
        private string interpreterSource;

        public InlineImportCodeLoader()
        {
            string currentDirectory = FileUtil.GetCurrentDirectory();
            while (!string.IsNullOrEmpty(currentDirectory))
            {
                string crayonSlnPath = System.IO.Path.Combine(currentDirectory, "Compiler", "CrayonWindows.sln");
                if (System.IO.File.Exists(crayonSlnPath))
                {
                    this.interpreterSource = System.IO.Path.GetFullPath(System.IO.Path.Combine(currentDirectory, "Interpreter", "source"));
                    break;
                }
                else
                {
                    currentDirectory = System.IO.Path.GetDirectoryName(currentDirectory);
                }
            }

            if (this.interpreterSource == null)
            {
                string crayonHome = System.Environment.GetEnvironmentVariable("CRAYON_HOME");
                if (!string.IsNullOrEmpty(crayonHome))
                {
                    string crayonHomeSource = System.IO.Path.GetFullPath(System.IO.Path.Combine(crayonHome, "vmsrc"));
                    if (System.IO.File.Exists(System.IO.Path.Combine(crayonHomeSource, "main.pst")))
                    {
                        this.interpreterSource = crayonHomeSource;
                    }
                }
            }

            if (this.interpreterSource == null)
            {
                throw new System.InvalidOperationException("The VM source code was not present.");
            }
        }

        public string LoadCode(string path)
        {
            string fullPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(this.interpreterSource, path));
            string code = System.IO.File.ReadAllText(fullPath);
            return code;
        }
    }
}
