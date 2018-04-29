using System.Collections.Generic;
using System.Linq;

namespace Platform
{
    public class TemplateStorage
    {
        private Dictionary<string, string> codeByKey = new Dictionary<string, string>();
        private Dictionary<string, string> nameByKey = new Dictionary<string, string>();

        public string GetCode(string key)
        {
            return this.codeByKey[key];
        }

        public string GetName(string key)
        {
            return this.nameByKey[key];
        }

        public void AddPastelTemplate(string key, string value)
        {
            this.AddPastelTemplate(key, null, value);
        }

        public void AddPastelTemplate(string key, string name, string value)
        {
            this.codeByKey[key] = value;
            if (name != null)
            {
                this.nameByKey[key] = name;
            }
        }

        public IList<string> GetTemplateKeysWithPrefix(string prefix)
        {
            return this.codeByKey.Keys.Where(s => s.StartsWith(prefix)).ToArray();
        }
    }
}
