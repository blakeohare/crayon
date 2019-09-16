using CommonUtil;
using Parser;
using System;
using System.Collections.Generic;

namespace Exporter
{
    public class Constants
    {
        private static readonly Dictionary<string, string> CONSTANT_REPLACEMENTS;

        static Constants()
        {
            Dictionary<string, string> constants = new Dictionary<string, string>()
            {
#if DEBUG
                { "IS_RELEASE", "false" },
#else
                { "IS_RELEASE", "true" },
#endif
            };

            Dictionary<string, Type> enumReplacementsByPrefix = new Dictionary<string, Type>()
            {
                { "PRIMITIVE_METHOD", typeof(PrimitiveMethods) },
                { "TYPE_ID", typeof(Types) },
            };

            foreach (string key in enumReplacementsByPrefix.Keys)
            {
                foreach (object enumValue in Enum.GetValues(enumReplacementsByPrefix[key]))
                {
                    constants.Add(key + "_" + enumValue.ToString(), ((int)enumValue).ToString());
                }
            }

            CONSTANT_REPLACEMENTS = constants;
        }

        public static string DoReplacements(bool keepPercents, string text, Dictionary<string, string> replacements)
        {
            if (keepPercents) return text;

            if (text.Contains("%%%"))
            {
                string[] parts = StringUtil.Split(text, "%%%");
                bool lastWasReplacement = false;

                List<string> replaced = new List<string>() { parts[0] };
                int i = 1;
                for (; i < parts.Length - 1; ++i)
                {
                    string key = parts[i];
                    if (CONSTANT_REPLACEMENTS.ContainsKey(key))
                    {
                        replaced.Add(CONSTANT_REPLACEMENTS[key]);
                        replaced.Add(parts[++i]);
                        lastWasReplacement = true;
                    }
                    else if (replacements.ContainsKey(key))
                    {
                        replaced.Add(replacements[key]);
                        replaced.Add(parts[++i]);
                        lastWasReplacement = true;
                    }
                    else
                    {
                        replaced.Add("%%%");
                        replaced.Add(key);
                        lastWasReplacement = false;
                    }
                }

                if (!lastWasReplacement)
                {
                    replaced.Add("%%%");
                    replaced.Add(parts[parts.Length - 1]);
                }

                text = string.Join("", replaced);
            }

            return text;
        }
    }
}
