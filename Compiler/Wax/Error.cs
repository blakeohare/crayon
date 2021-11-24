using System.Collections.Generic;

namespace Wax
{
    public class Error : JsonBasedObject
    {
        public Error() : base() { }

        internal Error(IDictionary<string, object> data) : base(data) { }

        public int Line { get { return this.GetInteger("line"); } set { this.SetInteger("line", value); } }
        public int Column { get { return this.GetInteger("col"); } set { this.SetInteger("col", value); } }
        public string FileName { get { return this.GetString("file"); } set { this.SetString("file", value); } }
        public string Message { get { return this.GetString("msg"); } set { this.SetString("msg", value); } }

        public bool HasLineInfo { get { return this.Line != 0; } }
    }
}
