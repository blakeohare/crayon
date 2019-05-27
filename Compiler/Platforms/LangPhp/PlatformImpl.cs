using Common;
using Platform;
using System.Collections.Generic;

namespace LangPhp
{
    public class PlatformImpl : AbstractPlatform
    {
        public PlatformImpl() : base("PHP") { }

        public override string Name { get { return "lang-php"; } }
        public override string InheritsFrom { get { return null; } }
        public override string NL { get { return "\n"; } }

        public override void ExportProject(Dictionary<string, FileOutput> output, IList<LibraryForExport> libraries, ResourceDatabase resourceDatabase, Options options)
        {
            throw new System.NotImplementedException();
        }

        public override void ExportStandaloneVm(Dictionary<string, FileOutput> output, IList<LibraryForExport> everyLibrary)
        {
            throw new System.NotImplementedException();
        }

        public override Dictionary<string, string> GenerateReplacementDictionary(Options options, ResourceDatabase resDb)
        {
            return new Dictionary<string, string>();
        }

        public override IDictionary<string, object> GetConstantFlags()
        {
            return new Dictionary<string, object>();
        }
    }
}
