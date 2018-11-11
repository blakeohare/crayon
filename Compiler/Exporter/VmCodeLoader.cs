using Common;

namespace Exporter
{
    public class VmCodeLoader : Pastel.IInlineImportCodeLoader
    {
        private static string crayonSourceDirectory = null;

        private string interpreterSource;

        public VmCodeLoader() { }

        public static string GetCrayonSourceDirectory()
        {
            if (crayonSourceDirectory != null) return crayonSourceDirectory;

            string currentDirectory = FileUtil.GetCurrentDirectory();
            while (!string.IsNullOrEmpty(currentDirectory))
            {
                string crayonSlnPath = System.IO.Path.Combine(currentDirectory, "Compiler", "CrayonWindows.sln");
                if (System.IO.File.Exists(crayonSlnPath))
                {
                    crayonSourceDirectory = System.IO.Path.GetFullPath(System.IO.Path.Combine(currentDirectory, "Interpreter", "source"));
                    return crayonSourceDirectory;
                }
                else
                {
                    currentDirectory = System.IO.Path.GetDirectoryName(currentDirectory);
                }
            }
            return null;
        }

        private void Initialize()
        {
            this.interpreterSource = GetCrayonSourceDirectory();

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
            if (this.interpreterSource == null)
            {
                this.Initialize();
            }

            string fullPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(this.interpreterSource, path));
            string code = System.IO.File.ReadAllText(fullPath);
            return code;
        }
    }
}
