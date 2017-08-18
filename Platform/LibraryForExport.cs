using System.Collections.Generic;
using Common;

namespace Platform
{
    public class LibraryForExport
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public Pastel.Nodes.FunctionDefinition ManifestFunction { get; set; }
        public Pastel.Nodes.FunctionDefinition[] Functions { get; set; }
        public string[] FunctionRegisteredNamesOrNulls { get; set; }
        public Multimap<string, ExportEntity> ExportEntities { get; set; }
        public string[] DotNetLibs { get; set; }
        public Dictionary<string, string> LibProjectNamesAndGuids { get; set; }
    }
}
