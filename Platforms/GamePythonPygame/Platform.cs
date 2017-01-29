using System;
using System.Collections.Generic;
using Common;

namespace GamePythonPygame
{
    public class Platform : IPlatform
    {
        public string Name { get { return "game-python-pygame"; } }
        public string InheritsFrom { get { return "lang-python"; } }

        public Dictionary<string, FileOutput> Export()
        {
            throw new NotImplementedException();
        }
    }
}
