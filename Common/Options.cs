using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    public class Options
    {
        private Dictionary<ExportOptionKey, object> options = new Dictionary<ExportOptionKey, object>();

        public Options() { }

        public Options SetOption(ExportOptionKey key, object value)
        {
            this.options[key] = value;
            return this;
        }

        public string GetStringOrNull(ExportOptionKey key)
        {
            object output;
            if (this.options.TryGetValue(key, out output))
            {
                return output.ToString();
            }
            return null;
        }

        public string GetString(ExportOptionKey key)
        {
            return this.options[key].ToString();
        }
    }
}
