using System.Collections.Generic;

namespace Wax
{
    public class CbxBundle : JsonBasedObject
    {
        public CbxBundle() : base() { }
        public CbxBundle(IDictionary<string, object> data) : base(data) { }

        public string ByteCode { get { return this.GetString("byteCode"); } set { this.SetString("byteCode", value); } }
        public ResourceDatabase ResourceDB { get { return this.GetObjectAsType<ResourceDatabase>("resources"); } set { this.SetObject("resources", value); } }
    }
}
