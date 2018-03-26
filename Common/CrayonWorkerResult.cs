using System.Collections.Generic;

namespace Common
{
    public class CrayonWorkerResult
    {
        private Dictionary<string, bool> booleanFields = new Dictionary<string, bool>();

        public bool GetField(string name)
        {
            return this.booleanFields.ContainsKey(name) ? this.booleanFields[name] : false;
        }

        public CrayonWorkerResult SetField(string name, bool value)
        {
            this.booleanFields[name] = value;
            return this;
        }

        public object Value { get; set; }

        public object[] ParallelValue { get; set; }
    }
}
