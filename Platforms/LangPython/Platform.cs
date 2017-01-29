using System;
using System.Collections.Generic;
using Common;

namespace LangPython
{
    public class Platform : IPlatform
    {
        public string Name { get { return "lang-python"; } }
        public string InheritsFrom {  get { return null; } }

        public Dictionary<string, FileOutput> Export()
        {
            throw new NotImplementedException();
        }
    }
}
