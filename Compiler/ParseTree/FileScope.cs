using System;
using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
    public class FileScope
    {
        internal HashSet<ImportStatement> Imports { get; private set; }

        public FileScope(string filename)
        {
            this.Imports = new HashSet<ImportStatement>();
        }
    }
}
