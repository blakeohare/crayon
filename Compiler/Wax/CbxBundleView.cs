using System.Collections.Generic;

namespace Wax
{
    public class CbxBundleView : JsonBasedObject
    {
        public string ByteCode { get { return this.GetString("byteCode"); } set { this.SetString("byteCode", value); } }
        public ResourceDatabase ResourceDB { get { return this.GetObject("resources") as ResourceDatabase; } set { this.SetObject("resources", value); } }

        public CbxBundleView(Dictionary<string, object> data) : base(data) { }

        public CbxBundleView(string byteCode, ResourceDatabase resDb)
        {
            this.ByteCode = byteCode;
            this.ResourceDB = resDb;
        }
    }
}
