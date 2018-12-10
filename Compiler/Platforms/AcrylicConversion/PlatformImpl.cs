using Common;
using Parser;
using Platform;
using System;
using System.Collections.Generic;

namespace AcrylicConversion
{
    public class PlatformImpl : AbstractPlatform
    {
        public PlatformImpl() : base("acrylic") { }

        public override string Name { get { return "acrylic-conversion"; } }

        public override string InheritsFrom { get { return null; } }

        public override string NL { get { return "\n"; } }

        public override void TranspileCode(
            Dictionary<string, FileOutput> output,
            object parserContextObj) // object is used because this method is virtual and I don't want a Parser reference in the Platform project.
        {
            ParserContext parserContext = (ParserContext)parserContextObj;
            throw new NotImplementedException();
        }

        public override void ExportProject(Dictionary<string, FileOutput> output, IList<LibraryForExport> libraries, ResourceDatabase resourceDatabase, Options options)
        {
            throw new NotImplementedException();
        }

        public override void ExportStandaloneVm(Dictionary<string, FileOutput> output, IList<LibraryForExport> everyLibrary)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<string, string> GenerateReplacementDictionary(Options options, ResourceDatabase resDb)
        {
            throw new NotImplementedException();
        }

        public override IDictionary<string, object> GetConstantFlags()
        {
            throw new NotImplementedException();
        }
    }
}
