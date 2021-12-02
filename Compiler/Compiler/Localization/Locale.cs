using System;
using System.Collections.Generic;
using System.Linq;

namespace Parser.Localization
{
    public class Locale
    {
        public string ID { get; set; }
        public KeywordsLookup Keywords { get; private set; }
        public StringTable Strings { get; private set; }

        private Dictionary<string, string> keywordsDictionary;
        private static readonly Dictionary<string, Locale> locales = new Dictionary<string, Locale>();

        public static Locale Get(string id)
        {
            Locale output;
            if (!locales.TryGetValue(id.ToLowerInvariant(), out output))
            {
                output = new Locale(id.ToLowerInvariant());
                locales[id.ToLowerInvariant()] = output;
            }
            return output;
        }

        public override string ToString()
        {
            return "Locale: " + this.ID;
        }

        private Locale(string name)
        {
            this.ID = name;
            bool invalid = false;
            if (name.Length > 100)
            {
                for (int i = 0; i < name.Length; ++i)
                {
                    char c = name[i];
                    if ((c < 'a' || c > 'z') &&
                        (c < 'A' || c > 'Z'))
                    {
                        invalid = true;
                        break;
                    }
                }
            }
            string keywordsRaw = ResourceReader.GetFile(name.ToLowerInvariant() + "/keywords.txt");
            if (keywordsRaw == null)
            {
                invalid = true;
            }

            if (invalid)
            {
                throw new InvalidOperationException("Unknown locale: '" + name + "'");
            }

            Dictionary<string, string> keywords = new Dictionary<string, string>();
            foreach (string keywordRow in keywordsRaw.Trim().Split('\n'))
            {
                string row = keywordRow.Trim();
                if (row.Length > 0)
                {
                    string[] parts = keywordRow.Split(':');
                    if (parts.Length != 2)
                    {
                        throw new Exception("Unknown line in keywords: " + keywordRow);
                    }

                    keywords.Add(parts[0].Trim(), parts[1].Trim());
                }
            }
            this.keywordsDictionary = keywords;

            this.Keywords = new KeywordsLookup()
            {
                Lookup = new HashSet<string>(keywords.Values),
                ValidIdentifierNames = new HashSet<string>(
                    new string[] {
                        "FIELD_ENUM_LENGTH",
                        "FIELD_ENUM_MAX",
                        "FIELD_ENUM_VALUES",
                        "MAIN_FUNCTION",
                    }.Select(k => keywords[k])),

                ABSTRACT = keywords["ABSTRACT"],
                BASE = keywords["BASE"],
                BREAK = keywords["BREAK"],
                CASE = keywords["CASE"],
                CATCH = keywords["CATCH"],
                CLASS = keywords["CLASS"],
                CONST = keywords["CONST"],
                CONSTRUCTOR = keywords["CONSTRUCTOR"],
                CONTINUE = keywords["CONTINUE"],
                DEFAULT = keywords["DEFAULT"],
                DO = keywords["DO"],
                DO_WHILE_END = keywords["DO_WHILE_END"],
                ELSE = keywords["ELSE"],
                ENUM = keywords["ENUM"],
                FALSE = keywords["FALSE"],
                FIELD = keywords["FIELD"],
                FIELD_ENUM_LENGTH = keywords["FIELD_ENUM_LENGTH"],
                FIELD_ENUM_MAX = keywords["FIELD_ENUM_MAX"],
                FIELD_ENUM_VALUES = keywords["FIELD_ENUM_VALUES"],
                FINAL = keywords["FINAL"],
                FINALLY = keywords["FINALLY"],
                FOR = keywords["FOR"],
                FUNCTION = keywords["FUNCTION"],
                IF = keywords["IF"],
                IMPORT = keywords["IMPORT"],
                INTERFACE = keywords["INTERFACE"],
                IS = keywords["IS"],
                MAIN_FUNCTION = keywords["MAIN_FUNCTION"],
                NAMESPACE = keywords["NAMESPACE"],
                NEW = keywords["NEW"],
                NULL = keywords["NULL"],
                PRIVATE = keywords["PRIVATE"],
                RETURN = keywords["RETURN"],
                STATIC = keywords["STATIC"],
                SWITCH = keywords["SWITCH"],
                THIS = keywords["THIS"],
                THROW = keywords["THROW"],
                TRUE = keywords["TRUE"],
                TRY = keywords["TRY"],
                WHILE = keywords["WHILE"],
            };

            this.Strings = new StringTable(name);
        }

        public string[] GetKeywordsList()
        {
            return this.keywordsDictionary.Values.ToArray();
        }

        public class KeywordsLookup
        {
            internal HashSet<string> ValidIdentifierNames { get; set; }
            internal HashSet<string> Lookup { get; set; }
            public bool IsValidVariable(string value)
            {
                return !this.Lookup.Contains(value) || this.ValidIdentifierNames.Contains(value);
            }

            public string ABSTRACT { get; set; }
            public string BASE { get; set; }
            public string BREAK { get; set; }
            public string CASE { get; set; }
            public string CATCH { get; set; }
            public string CLASS { get; set; }
            public string CONST { get; set; }
            public string CONSTRUCTOR { get; set; }
            public string CONTINUE { get; set; }
            public string DEFAULT { get; set; }
            public string DO { get; set; }
            public string DO_WHILE_END { get; set; }
            public string ELSE { get; set; }
            public string ENUM { get; set; }
            public string FALSE { get; set; }
            public string FIELD { get; set; }
            public string FIELD_ENUM_LENGTH { get; set; }
            public string FIELD_ENUM_MAX { get; set; }
            public string FIELD_ENUM_VALUES { get; set; }
            public string FINAL { get; set; }
            public string FINALLY { get; set; }
            public string FOR { get; set; }
            public string FUNCTION { get; set; }
            public string IF { get; set; }
            public string IMPORT { get; set; }
            public string INTERFACE { get; set; }
            public string IS { get; set; }
            public string MAIN_FUNCTION { get; set; }
            public string NAMESPACE { get; set; }
            public string NEW { get; set; }
            public string NULL { get; set; }
            public string PRIVATE { get; set; }
            public string RETURN { get; set; }
            public string STATIC { get; set; }
            public string SWITCH { get; set; }
            public string THIS { get; set; }
            public string THROW { get; set; }
            public string TRUE { get; set; }
            public string TRY { get; set; }
            public string WHILE { get; set; }
        }
    }
}
