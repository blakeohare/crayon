using System;
using System.Collections.Generic;
using Common;

namespace LangPython
{
    public class Platform : IPlatform
    {
        public string Name { get { return "lang-python"; } }
        public string InheritsFrom {  get { return null; } }

        public Dictionary<string, FileOutput> Export(
            Dictionary<string, object[]> executablesPerCompilationUnit,
            object[] structDefinitions)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, bool> ConstantFlags
        {
            get
            {
                return new Dictionary<string, bool>()
                {
                    { "PLATFORM_SUPPORTS_LIST_CLEAR", false },
                    { "STRONGLY_TYPED", false },
                    { "IS_ARRAY_SAME_AS_LIST", true },
                    { "IS_PYTHON", true },
                    { "IS_CHAR_A_NUMBER", false },
                    { "INT_IS_FLOOR", false },
                    { "IS_THREAD_BLOCKING_ALLOWED", true },
                };
            }
        }
    }
}
