using System.Collections.Generic;
using System.Linq;

namespace CommonUtil.Collections
{
    public static class DictionaryUtil
    {
        public static void MergeDictionaryInto<K, V>(Dictionary<K, V> newDict, Dictionary<K, V> mergeIntoThis)
        {
            foreach (KeyValuePair<K, V> kvp in newDict)
            {
                mergeIntoThis[kvp.Key] = kvp.Value;
            }
        }

        public static Dictionary<K, V> MergeDictionaries<K, V>(params Dictionary<K, V>[] dictionaries)
        {
            if (dictionaries.Length == 0) return new Dictionary<K, V>();
            if (dictionaries.Length == 1) return new Dictionary<K, V>(dictionaries[0]);
            if (dictionaries.Length == 2)
            {
                // Super common.
                if (dictionaries[0].Count == 0) return new Dictionary<K, V>(dictionaries[1]);
                if (dictionaries[1].Count == 0) return new Dictionary<K, V>(dictionaries[0]);
            }

            Dictionary<K, V> output = new Dictionary<K, V>(dictionaries[0]);
            for (int i = 0; i < dictionaries.Length; ++i)
            {
                Dictionary<K, V> dict = dictionaries[i];
                foreach (K k in dict.Keys)
                {
                    output[k] = dict[k];
                }
            }
            return output;
        }

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
