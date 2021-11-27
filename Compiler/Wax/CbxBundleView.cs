using System.Collections.Generic;
using System.Linq;

namespace Wax
{
    public class CbxBundleView : JsonBasedObject
    {
        public string ByteCode { get { return this.GetString("byteCode"); } set { this.SetString("byteCode", value); } }
        public ResourceDatabase ResourceDB { get { return this.GetObject("resources") as ResourceDatabase; } set { this.SetObject("resources", value); } }
        public bool UsesU3 { get { return this.GetBoolean("usesU3"); } set { this.SetBoolean("usesU3", value); } }
        public Error[] Errors
        {
            get { return (this.GetObjects("errors") ?? new Error[0]).Select(obj => new Error(obj.RawData)).ToArray(); }
            set { this.SetObjects("errors", value); }
        }
        public bool HasErrors { get { return this.HasObjects("errors"); } }

        // TODO: don't double-encode
        public string DependencyTreeJson {  get { return this.GetString("depTreeJson"); } set { this.SetString("depTreeJson", value); } }

        public CbxBundleView(Dictionary<string, object> data) : base(data) { }

        public CbxBundleView(string byteCode, ResourceDatabase resDb, bool usesU3, IList<Error> errors)
        {
            this.ByteCode = byteCode;
            this.ResourceDB = resDb;
            this.UsesU3 = usesU3;
            this.Errors = (errors ?? new Error[0]).ToArray();
        }
    }
}
