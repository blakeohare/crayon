using CommonUtil;
using System.Collections.Generic;

namespace Common
{
    public static class ConstantReplacer
    {
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
                    if (replacements.ContainsKey(key))
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
