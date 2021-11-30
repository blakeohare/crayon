using System.Collections.Generic;

namespace Wax
{
    public class Command : JsonBasedObject
    {
        public Command() : base() { }
        public Command(IDictionary<string, object> data) : base(data) { }

        public string DefaultProjectId { get { return this.GetString("defaultProjectId"); } set { this.SetString("defaultProjectId", value); } }
        public string DefaultProjectLocale { get { return this.GetString("defaultProjectLocale"); } set { this.SetString("defaultProjectLocale", value); } }
        public string DefaultProjectType { get { return this.GetString("defaultProjectType"); } set { this.SetString("defaultProjectType", value); } }
        public string BuildFilePath { get { return this.GetString("buildFilePath"); } set { this.SetString("buildFilePath", value); } }
        public string BuildTarget { get { return this.GetString("buildTarget"); } set { this.SetString("buildTarget", value); } }
        public bool IsEmpty { get { return this.GetBoolean("isEmpty"); } set { this.SetBoolean("isEmpty", value); } }
        public string CbxExportPath { get { return this.GetString("cbxExportPath"); } set { this.SetString("cbxExportPath", value); } }
        public bool ShowVersion { get { return this.GetBoolean("showVersion"); } set { this.SetBoolean("showVersion", value); } }
        public bool ShowDependencyTree { get { return this.GetBoolean("showDependencyTree"); } set { this.SetBoolean("showDependencyTree", value); } }
        public bool IsDirectCbxRun { get { return this.GetBoolean("isDirectCbxRun"); } set { this.SetBoolean("isDirectCbxRun", value); } }
        public bool IsErrorCheckOnly { get { return this.GetBoolean("isErrorCheckOnly"); } set { this.SetBoolean("isErrorCheckOnly", value); } }
        public bool IsJsonOutput { get { return this.GetBoolean("isJsonOutput"); } set { this.SetBoolean("isJsonOutput", value); } }
        public bool UseOutputPrefixes { get { return this.GetBoolean("useOutputPrefixes"); } set { this.SetBoolean("useOutputPrefixes", value); } }
        public bool ResourceErrorsShowRelativeDir { get { return this.GetBoolean("resourceErrorsShowRelativeDir"); } set { this.SetBoolean("resourceErrorsShowRelativeDir", value); } }
        public string OutputDirectoryOverride { get { return this.GetString("outputDirectoryOverride"); } set { this.SetString("outputDirectoryOverride", value); } }
        public string[] DirectRunArgs { get { return this.GetStrings("directRunArgs"); } set { this.SetStrings("directRunArgs", value); } }
        public bool DirectRunShowLibStack { get { return this.GetBoolean("directRunShowLibStack"); } set { this.SetBoolean("directRunShowLibStack", value); } }
        public bool IsCSharpToAcrylicTranspiler { get { return this.GetBoolean("isCSharpToAcrylicTranspiler"); } set { this.SetBoolean("isCSharpToAcrylicTranspiler", value); } }
        public string ApkExportPath { get { return this.GetString("apkExportPath"); } set { this.SetString("apkExportPath", value); } }
        public bool ErrorsAsExceptions { get { return this.GetBoolean("errorsAsExceptions"); } set { this.SetBoolean("errorsAsExceptions", value); } }

        public bool HasBuildFile { get { return this.BuildFilePath != null; } }
        public bool HasTarget { get { return this.BuildTarget != null; } }
        public bool HasOutputDirectoryOverride { get { return this.OutputDirectoryOverride != null; } }
        public bool IsGenerateDefaultProject { get { return this.DefaultProjectId != null; } }
        public bool IsCbxExport { get { return this.CbxExportPath != null; } }
    }
}
