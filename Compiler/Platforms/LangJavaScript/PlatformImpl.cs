using Platform;
using System;
using System.Collections.Generic;
using Wax;

namespace LangJavaScript
{
    public class PlatformImpl : AbstractPlatform
    {
        public override string Name { get { return "lang-javascript"; } }
        public override string NL { get { return "\n"; } }

        public PlatformImpl()
            : base("JAVASCRIPT")
        { }

        public override void ExportProject(
            Dictionary<string, FileOutput> output,
            BuildData buildData,
            ExportProperties exportProperties)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<string, string> GenerateReplacementDictionary(
            ExportProperties exportProperties,
            BuildData buildData)
        {
            return AbstractPlatform.GenerateGeneralReplacementsDictionary(exportProperties);
        }

        public override IDictionary<string, object> GetConstantFlags()
        {
            return new Dictionary<string, object>();
        }
    }
}
