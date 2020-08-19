using Common;
using System.Collections.Generic;

namespace Exporter
{
    public class ExportRequest
    {
        public string ByteCode { get; set; }
        public string ProjectID { get; set; }
        public string GuidSeed { get; set; }
        public IList<AssemblyResolver.AssemblyMetadata> LibraryAssemblies { get; set; }
        public string[] IconPaths { get; set; }
        public string LaunchScreenPath { get; set; }
        public string ProjectTitle { get; set; }
        public string JsFilePrefix { get; set; }
        public bool JsFullPage { get; set; }
        public string IosBundlePrefix { get; set; }
        public string JavaPackage { get; set; }
        public string OrganizationName { get; set; }
        public NullableInteger WindowWidth { get; set; }
        public NullableInteger WindowHeight { get; set; }
        public string Orientations { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
    }
}
