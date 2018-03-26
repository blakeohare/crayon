namespace Crayon
{
    public class ExportCommand
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
        public bool ShowLibraryDepTree { get; set; }
        public bool IsDirectCbxRun { get; set; }
        public string[] DirectRunArgs { get; set; }

        public bool HasBuildFile { get { return this.BuildFilePath != null; } }
        public bool HasTarget { get { return this.BuildTarget != null; } }
        public bool IsGenerateDefaultProject { get { return this.DefaultProjectId != null; } }
        public bool IsVmExportCommand { get { return this.VmExportDirectory != null || this.VmPlatform != null; } } // The actual VM exporter will throw the error if one is not specified.
        public bool IsCbxExport { get { return this.CbxExportPath != null; } }

        public ExecutionType IdentifyUseCase()
        {
            if (this.IsGenerateDefaultProject) return ExecutionType.GENERATE_DEFAULT_PROJECT;
            if (this.IsEmpty) return ExecutionType.SHOW_USAGE;
            if (this.IsVmExportCommand) return ExecutionType.EXPORT_VM_STANDALONE;
            if (this.HasTarget) return ExecutionType.EXPORT_VM_BUNDLE;
            if (this.IsCbxExport) return ExecutionType.EXPORT_CBX;
            return ExecutionType.RUN_CBX;
        }
    }
}
