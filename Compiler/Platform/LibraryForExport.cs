using Common;
using System.Collections.Generic;

namespace Platform
{
    public class LibraryForExport
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public bool HasPastelCode { get { return this.ManifestFunctionDEPRECATED != null; } }
        public Pastel.PastelContext PastelContext { get; set; }
        public Pastel.Nodes.FunctionDefinition ManifestFunctionDEPRECATED { get; set; }
        public Pastel.Nodes.FunctionDefinition[] FunctionsDEPRECATED { get; set; }
        public Pastel.Nodes.StructDefinition[] StructsDEPRECATED { get; set; }
        public string[] FunctionRegisteredNamesOrNulls { get; set; }
        public Multimap<string, ExportEntity> ExportEntities { get; set; }
        public string[] DotNetLibs { get; set; }
        public Dictionary<string, string> LibProjectNamesAndGuids { get; set; }
    }
}
