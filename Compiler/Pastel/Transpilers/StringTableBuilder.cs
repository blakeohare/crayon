using System;
using System.Collections.Generic;

namespace Pastel.Transpilers
{
    public class StringTableBuilder
    {
        public string Prefix { get; private set; }
        private string constantPrefix;
        private Dictionary<string, string> valueToId = new Dictionary<string, string>();
        public List<string> Values { get; private set; }
        public List<string> Names { get; private set; }

        public StringTableBuilder(string prefix)
        {
            this.Prefix = prefix;
            this.constantPrefix = "STRING_CONST_" + prefix + "_";
            this.Values = new List<string>();
            this.Names = new List<string>();
        }

        public string GetId(string value)
        {
            string output;
            if (!valueToId.TryGetValue(value, out output))
            {
                output = this.constantPrefix + this.valueToId.Count;
                this.valueToId[value] = output;
                this.Values.Add(value);
                this.Names.Add(output);
            }
            return output;
        }
    }
}
