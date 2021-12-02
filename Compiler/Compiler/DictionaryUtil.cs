using System.Collections.Generic;
using System.Linq;

namespace Build
{
    public static class DictionaryUtil
    {
        public static Dictionary<string, string> FlattenedDictionaryToDictionary(IList<string> items)
        {
            Dictionary<string, string> output = new Dictionary<string, string>();
            for (int i = 0; i < items.Count; i += 2)
            {
                output[items[i]] = items[i + 1];
            }
            return output;
        }

        public static string[] DictionaryToFlattenedDictionary(IDictionary<string, string> dict)
        {
            List<string> output = new List<string>();
            foreach (string key in dict.Keys.OrderBy(k => k))
            {
                output.Add(key);
                output.Add(dict[key]);
            }
            return output.ToArray();
        }
    }
}
