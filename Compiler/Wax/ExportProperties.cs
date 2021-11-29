using System.Collections.Generic;

namespace Wax
{
    public class ExportProperties : JsonBasedObject
    {
        public ExportProperties() : base() { }
        public ExportProperties(IDictionary<string, object> data) : base(data) { }

        public string ProjectID { get { return this.GetString("projectId"); } set { this.SetString("projectId", value); } }
        public string GuidSeed { get { return this.GetString("guidSeed"); } set { this.SetString("guidSeed", value); } }
        public string[] IconPaths { get { return this.GetStrings("iconPaths"); } set { this.SetStrings("iconPaths", value); } }
        public string LaunchScreenPath { get { return this.GetString("launchScreenPath"); } set { this.SetString("launchScreenPath", value); } }
        public string ProjectTitle { get { return this.GetString("title"); } set { this.SetString("title", value); } }
        public string JsFilePrefix { get { return this.GetString("jsFilePrefix"); } set { this.SetString("jsFilePrefix", value); } }
        public bool JsFullPage { get { return this.GetBoolean("jsFullPage"); } set { this.SetBoolean("jsFullPage", value); } }
        public string IosBundlePrefix { get { return this.GetString("iosBundlePrefix"); } set { this.SetString("iosBundlePrefix", value); } }
        public string JavaPackage { get { return this.GetString("javaPackage"); } set { this.SetString("javaPackage", value); } }
        public string Orientations { get { return this.GetString("orientations"); } set { this.SetString("orientations", value); } }
        public string Version { get { return this.GetString("version"); } set { this.SetString("version", value); } }
        public string Description { get { return this.GetString("description"); } set { this.SetString("description", value); } }

        public string ExportPlatform { get { return this.GetString("exportPlatform"); } set { this.SetString("exportPlatform", value); } }
        public string ProjectDirectory { get { return this.GetString("projectDirectory"); } set { this.SetString("projectDirectory", value); } }
        public string OutputDirectory { get { return this.GetString("outputDirectory"); } set { this.SetString("outputDirectory", value); } }

        public bool IsAndroid { get { return this.ExportPlatform.ToLowerInvariant() == "javascript-app-android"; } }
    }
}
