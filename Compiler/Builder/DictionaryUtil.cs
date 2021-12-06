using System.Collections.Generic;

namespace Builder
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
    }
}
