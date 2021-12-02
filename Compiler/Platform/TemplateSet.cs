using System.Collections.Generic;
using System.Linq;

namespace Platform
{
    public class TemplateSet
    {
        private Dictionary<string, byte[]> data;

        public TemplateSet(Dictionary<string, byte[]> data)
        {
            this.data = data;
        }

        public string GetText(string path)
        {
            byte[] byteData;
            if (!this.data.TryGetValue(path, out byteData))
            {
                throw new System.InvalidOperationException(path + " was not available in the template set. Files: " + string.Join(", ", this.data.Keys));
            }
            return Wax.Util.UniversalTextDecoder.Decode(byteData);
        }

        public byte[] GetBytes(string path)
        {
            return this.data[path];
        }

        public string[] GetPaths(string prefix)
        {
            return GetPaths(prefix, null);
        }

        public string[] GetPaths(string prefix, string suffix)
        {
            return this.data.Keys
                .Where(k => k.StartsWith(prefix) && (suffix == null || k.EndsWith(suffix)))
                .OrderBy(k => k)
                .ToArray();
        }
    }
}
