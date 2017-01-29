using System;
using System.Collections.Generic;
using Common;

namespace GamePythonPygame
{
    public class Platform : IPlatform
    {
        public string Name { get { return "game-python-pygame"; } }
        public string InheritsFrom { get { return "lang-python"; } }

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
                return new Dictionary<string, bool>();
            }
        }
    }
}
