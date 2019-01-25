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
            return Common.MysteryTextDecoder.DecodeArbitraryBytesAsAppropriatelyAsPossible(this.data[path]);
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
