using System.Collections.Generic;

namespace Wax
{
    public class ExportProperties : JsonBasedObject
    {
        public ExportProperties() : base() { }
        public ExportProperties(IDictionary<string, object> data) : base(data) { }

        public string ByteCode { get { return this.GetString("byteCode"); } set { this.SetString("byteCode", value); } }
        public string ProjectID { get { return this.GetString("projectId"); } set { this.SetString("projectId", value); } }
        public string GuidSeed { get { return this.GetString("guidSeed"); } set { this.SetString("guidSeed", value); } }
        private string IconPathsDelim { get { return this.GetString("iconPaths"); } set { this.SetString("iconPaths", value); } }
        public string LaunchScreenPath { get { return this.GetString("launchScreenPath"); } set { this.SetString("launchScreenPath", value); } }
        public string ProjectTitle { get { return this.GetString("title"); } set { this.SetString("title", value); } }
        public string JsFilePrefix { get { return this.GetString("jsFilePrefix"); } set { this.SetString("jsFilePrefix", value); } }
        public bool JsFullPage { get { return this.GetBoolean("jsFullPage"); } set { this.SetBoolean("jsFullPage", value); } }
        public string IosBundlePrefix { get { return this.GetString("iosBundlePrefix"); } set { this.SetString("iosBundlePrefix", value); } }
        public string JavaPackage { get { return this.GetString("javaPackage"); } set { this.SetString("javaPackage", value); } }
        public string Orientations { get { return this.GetString("orientations"); } set { this.SetString("orientations", value); } }
        public string Version { get { return this.GetString("version"); } set { this.SetString("version", value); } }
        public string Description { get { return this.GetString("description"); } set { this.SetString("description", value); } }

        public string[] IconPaths
        {
            get
            {
                string value = this.IconPathsDelim;
                if (value != null) return value.Split(',');
                return null;
            }
            set
            {
                this.IconPathsDelim = string.Join(',', value);
            }
        }
    }
}
