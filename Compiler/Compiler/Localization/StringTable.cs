using CommonUtil;
using CommonUtil.Resources;
using System.Collections.Generic;

namespace Parser.Localization
{
    public class StringTable
    {
        private Dictionary<string, string> lookup = new Dictionary<string, string>();
        public StringTable(string locale)
        {
            string stringTable = ResourceReader.GetFile(locale + "/strings.txt");

            foreach (string line in stringTable.Split('\n'))
            {
                string trimmedLine = line.Trim();
                if (trimmedLine.Length > 0)
                {
                    string[] parts = StringUtil.SplitOnce(line, ":");
                    string key = parts[0].Trim();
                    string value = parts[1].Trim();
                    lookup[key] = value;
                }
            }
        }

        public string Get(string key, params string[] values)
        {
            string value = this.lookup[key];
            return string.Format(value, values);
        }
    }
}
