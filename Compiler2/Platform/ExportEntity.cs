using Common;
using System.Collections.Generic;

namespace Platform
{
    public class ExportEntity
    {
        private Dictionary<string, string> values = new Dictionary<string, string>();

        public string Type { get; set; }
        public object Value { get; set; }
        public string StringValue { get { return this.Value == null ? null : this.Value.ToString(); } }
        public Dictionary<string, string> Values { get { return this.values; } }
        public FileOutput FileOutput { get; set; }
    }
}
