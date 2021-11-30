using Common;
using Platform;
using System;
using System.Collections.Generic;
using Wax;

namespace LangCSharp
{
    public class PlatformImpl : AbstractPlatform
    {
        public override string Name { get { return "lang-csharp"; } }
        public override string InheritsFrom { get { return null; } }
        public override string NL { get { return "\r\n"; } }

        public PlatformImpl()
            : base("CSHARP")
        { }

        public override IDictionary<string, object> GetConstantFlags()
        {
            return new Dictionary<string, object>();
        }

        public override void ExportStandaloneVm(Dictionary<string, FileOutput> output)
        {
            throw new NotImplementedException();
        }

        public override void ExportProject(
            Dictionary<string, FileOutput> output,
            BuildData buildData,
            ExportProperties exportProperties)
        {
            throw new InvalidOperationException("This platform does not support direct export.");
        }

        public override Dictionary<string, string> GenerateReplacementDictionary(
            ExportProperties exportProperties,
            BuildData buildData)
        {
            Dictionary<string, string> replacements = AbstractPlatform.GenerateGeneralReplacementsDictionary(exportProperties);
            replacements["PROJECT_GUID"] = "project guid goes here.";
            return replacements;
        }
    }
}
