using System.Collections.Generic;

namespace Parser.ParseTree
{
    public class FileScope
    {
        public string Name { get; private set; }
        internal HashSet<ImportStatement> Imports { get; private set; }
        public CompilationScope CompilationScope { get; private set; }

        internal FileScopedEntityLookup FileScopeEntityLookup { get; private set; }

        public FileScope(string filename, CompilationScope scope)
        {
            this.Name = filename;
            this.Imports = new HashSet<ImportStatement>();
            this.FileScopeEntityLookup = new FileScopedEntityLookup().SetFileScope(this);
            this.CompilationScope = scope;
        }
    }
}
