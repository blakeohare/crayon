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

        public bool GetBool(ExportOptionKey key)
        {
            object output;
            return this.options.TryGetValue(key, out output) && (bool)output;
        }

        public string GetStringOrNull(ExportOptionKey key)
        {
            object output;
            if (this.options.TryGetValue(key, out output))
            {
                if (output == null) return null;
                return output.ToString();
            }
            return null;
        }

        public string GetStringOrEmpty(ExportOptionKey key)
        {
            return GetString(key, "");
        }

        public string GetString(ExportOptionKey key)
        {
            return this.options[key].ToString();
        }

        public string GetString(ExportOptionKey key, string defaultValue)
        {
            return GetStringOrNull(key) ?? defaultValue;
        }

        public object[] GetArray(ExportOptionKey key)
        {
            object output;
            if (this.options.TryGetValue(key, out output))
            {
                return (object[])output;
            }
            return null;
        }
    }
}
