using System;
using System.Collections.Generic;
using System.Text;

namespace Wax
{
    public class BuildData : JsonBasedObject
    {
        public BuildData() : base() { }
        public BuildData(IDictionary<string, object> data) : base(data) { }
        
        public CbxBundle CbxBundle { get { return this.GetObjectAsType<CbxBundle>("cbxBundle"); } set { this.SetObject("cbxBundle", value); } }
        public bool UsesU3 { get { return this.GetBoolean("usesU3"); } set { this.SetBoolean("usesU3", value); } }
        public string ExportPlatform { get { return this.GetString("exportPlatform"); } set { this.SetString("exportPlatform", value); } }

        public Error[] Errors
        {
            get { return this.GetObjectsAsType<Error>("errors"); }
            set { this.SetObjects("errors", value); }
        }
        public bool HasErrors { get { return this.HasObjects("errors"); } }
    }
}
