namespace Crayon
{
    internal class Command
    {
        public string DefaultProjectId { get; set; }
        public string DefaultProjectLocale { get; set; }
        public string BuildFilePath { get; set; }
        public string BuildTarget { get; set; }
        public string VmPlatform { get; set; }
        public string VmExportDirectory { get; set; }
        public bool ShowPerformanceMarkers { get; set; }
        public bool IsEmpty { get; set; }
        public string CbxExportPath { get; set; }
        public bool ShowVersion { get; set; }
        public bool ShowDependencyTree { get; set; }
        public bool IsDirectCbxRun { get; set; }
        public bool IsErrorCheckOnly { get; set; }
        public bool IsJsonOutput { get; set; }
        public bool UseOutputPrefixes { get; set; }
        public bool ResourceErrorsShowRelativeDir { get; set; }
        public string OutputDirectoryOverride { get; set; }
        public string[] DirectRunArgs { get; set; }
        public bool DirectRunShowLibStack { get; set; }
        public bool IsCSharpToAcrylicTranspiler { get; private set; }

        public bool HasBuildFile { get { return this.BuildFilePath != null; } }
        public bool HasTarget { get { return this.BuildTarget != null; } }
        public bool HasOutputDirectoryOverride { get { return this.OutputDirectoryOverride != null; } }
        public bool IsGenerateDefaultProject { get { return this.DefaultProjectId != null; } }
        public bool IsVmExportCommand { get { return this.VmExportDirectory != null || this.VmPlatform != null; } } // The actual VM exporter will throw the error if one is not specified.
        public bool IsCbxExport { get { return this.CbxExportPath != null; } }
    }
}
