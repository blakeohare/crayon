using System.Collections.Generic;
using System.Linq;

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

        public static Error[] GetErrorList(object jsonData)
        {
            if (jsonData is object[])
            {
                object[] array = (object[])jsonData;
                return array
                    .OfType<Dictionary<string, object>>().Select(d => new Error(d))
                    .Concat(
                        array.OfType<JsonBasedObject>().Select(jbo => new Error(jbo.GetRawData())))
                    .ToArray();
            }

            if (jsonData is Error[])
            {
                return (Error[])jsonData;
            }
 
            if (jsonData is JsonBasedObject[])
            {
                return ((JsonBasedObject[])jsonData).OfType<JsonBasedObject>().Select(jbo => new Error(jbo.GetRawData())).ToArray();
            }

            return new Error[0];
        }
    }
}
