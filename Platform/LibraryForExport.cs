using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform
{
    public class LibraryForExport
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public Pastel.Nodes.FunctionDefinition ManifestFunction { get; set; }
        public Pastel.Nodes.FunctionDefinition[] Functions { get; set; }
        public string[] FunctionRegisteredNamesOrNulls { get; set; }

    }
}
