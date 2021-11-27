using System.Collections.Generic;
using System.Linq;

namespace Wax
{
    public class CbxBundleView : JsonBasedObject
    {
        public string ByteCode { get { return this.GetString("byteCode"); } set { this.SetString("byteCode", value); } }
        public ResourceDatabase ResourceDB { get { return this.GetObjectAsType<ResourceDatabase>("resources"); } set { this.SetObject("resources", value); } }
        public bool UsesU3 { get { return this.GetBoolean("usesU3"); } set { this.SetBoolean("usesU3", value); } }
        public Error[] Errors
        {
            get { return this.GetObjectsAsType<Error>("errors"); }
            set { this.SetObjects("errors", value); }
        }
        public bool HasErrors { get { return this.HasObjects("errors"); } }

        // TODO: don't double-encode
        public string DependencyTreeJson { get { return this.GetString("depTreeJson"); } set { this.SetString("depTreeJson", value); } }

        public string ExportPlatform { get { return this.GetString("exportPlatform"); } set { this.SetString("exportPlatform", value); } }

        public CbxBundleView(IDictionary<string, object> data) : base(data) { }

        public CbxBundleView(string byteCode, ResourceDatabase resDb, bool usesU3, string exportPlatform, IList<Error> errors)
        {
            this.ByteCode = byteCode;
            this.ResourceDB = resDb;
            this.UsesU3 = usesU3;
            this.Errors = (errors ?? new Error[0]).ToArray();
            if (exportPlatform != null) this.ExportPlatform = exportPlatform;
        }
    }
}
