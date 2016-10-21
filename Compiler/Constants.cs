using System;
using System.Collections.Generic;
using System.Linq;

namespace Crayon
{
    public class Constants
    {
        private static readonly Dictionary<string, string> CONSTANT_REPLACEMENTS;
        
        static Constants()
        {
            Dictionary<string, string> constants = new Dictionary<string, string>()
            {
                { "OPEN_GL_ELLIPSE_POINT_COUNT", "60" },
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
        
        public static string DoReplacements(string text, Dictionary<string, string> replacements)
        {
            if (text.Contains("%%%"))
            {
                string[] parts = text.Split(new string[] { "%%%" }, StringSplitOptions.None);
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

                return string.Join("", replaced);
            }

            // Hackity hack hack hack. Oh well.
            // TODO: in the resolver, add variable references to these arguments and then return a normal function invocations.
            // That would be the mildly cleaner way of doing this.
            return text.Replace(
                "$_exception(",
                "generateException(stack, pc, valueStackSize, ec, ");
        }
    }
}
