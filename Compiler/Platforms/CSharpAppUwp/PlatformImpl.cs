using Common;
using Pastel;
using Platform;
using System;
using System.Collections.Generic;

namespace CSharpAppUwp
{
    public class PlatformImpl : AbstractPlatform
    {
        public override string Name { get { return "csharp-app-uwp"; } }
        public override string InheritsFrom { get { return "lang-csharp"; } }
        public override string NL { get { return "\r\n"; } }

        public PlatformImpl()
            : base(Language.CSHARP)
        { }

        public override IDictionary<string, object> GetConstantFlags()
        {
            return new Dictionary<string, object>();
        }

        public override Dictionary<string, string> GenerateReplacementDictionary(Options options, ResourceDatabase resDb)
        {
            throw new NotImplementedException();
        }

        public override void ExportStandaloneVm(
            Dictionary<string, FileOutput> output,
            TemplateStorage templates,
            IList<LibraryForExport> everyLibrary)
        {
            throw new NotImplementedException();
        }

        public override void ExportProject(
            Dictionary<string, FileOutput> output,
            TemplateStorage templates,
            IList<LibraryForExport> libraries,
            ResourceDatabase resourceDatabase,
            Options options)
        {
            throw new NotImplementedException();
        }
    }
}
