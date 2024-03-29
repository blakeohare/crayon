﻿using System.Collections.Generic;

namespace Platform
{
    public static class MiscUtil
    {
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

    }
}
