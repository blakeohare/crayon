using System;
using System.Collections.Generic;
using Common;

namespace GameCSharpOpenTk
{
    public class Platform : AbstractPlatform
    {
        public override string Name { get { return "game-csharp-opentk-cbx"; } }
        public override string InheritsFrom { get { return "lang-csharp"; } }

        public override Dictionary<string, FileOutput> Export(
            Dictionary<string, object[]> executablesPerCompilationUnit,
            object[] structDefinitions)
        {
            throw new NotImplementedException();
        }

        public override IDictionary<string, object> GetConstantFlags()
        {
            return new Dictionary<string, object>();
        }
    }
}
