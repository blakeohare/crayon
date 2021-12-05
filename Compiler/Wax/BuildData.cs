using System.Collections.Generic;

namespace Wax
{
    public class BuildData : JsonBasedObject
    {
        public BuildData() : base() { }
        public BuildData(IDictionary<string, object> data) : base(data) { }

        public CbxBundle CbxBundle { get { return this.GetObjectAsType<CbxBundle>("cbxBundle"); } set { this.SetObject("cbxBundle", value); } }
        public ExportProperties ExportProperties { get { return this.GetObjectAsType<ExportProperties>("exportProperties"); } set { this.SetObject("exportProperties", value); } }
        public string DependencyTreeJson { get { return this.GetString("depTreeJson"); } set { this.SetString("depTreeJson", value); } }
        public string ProjectID { get { return this.GetString("projectId"); } set { this.SetString("projectId", value); } }

        // TODO: move me to ExportProperties
        public bool UsesU3 { get { return this.GetBoolean("usesU3"); } set { this.SetBoolean("usesU3", value); } }

        public Error[] Errors
        {
            get { return this.GetObjectsAsType<Error>("errors"); }
            set { this.SetObjects("errors", value); }
        }
        public bool HasErrors { get { return this.HasObjects("errors"); } }
    }
}
