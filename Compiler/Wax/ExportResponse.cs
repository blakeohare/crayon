using System;
using System.Collections.Generic;
using System.Text;

namespace Wax
{
    public class ExportResponse : JsonBasedObject
    {
        public ExportResponse() : base() { }
        public ExportResponse(IDictionary<string, object> data) : base(data) { }

        public Error[] Errors
        {
            get { return this.GetObjectsAsType<Error>("errors"); }
            set { this.SetObjects("errors", value); }
        }
        public bool HasErrors { get { return this.HasObjects("errors"); } }
    }
}
