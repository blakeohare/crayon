using Common;
using Platform;
using System;
using System.Collections.Generic;

namespace LangJava
{
    public class PlatformImpl : AbstractPlatform
    {
        public override string Name { get { return "lang-java"; } }
        public override string InheritsFrom { get { return null; } }
        public override string NL { get { return "\n"; } }

        public PlatformImpl()
            : base("JAVA")
        { }

        public override void ExportStandaloneVm(
            Dictionary<string, FileOutput> output,
            IList<LibraryForExport> everyLibrary)
        {
            throw new NotImplementedException();
        }

        public override void ExportProject(
            Dictionary<string, FileOutput> output,
            IList<LibraryForExport> libraries,
            ResourceDatabase resourceDatabase,
            Options options)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<string, string> GenerateReplacementDictionary(Options options, ResourceDatabase resDb)
        {
            // This is repeated in the JavaScriptAppAndroid platform.
            Dictionary<string, string> replacements = new Dictionary<string, string>();
            replacements["PROJECT_ID"] = options.GetString(ExportOptionKey.PROJECT_ID);
            replacements["JAVA_PACKAGE"] = (options.GetStringOrNull(ExportOptionKey.JAVA_PACKAGE) ?? options.GetString(ExportOptionKey.PROJECT_ID)).ToLowerInvariant();
            replacements["PROJECT_TITLE"] = options.GetStringOrNull(ExportOptionKey.PROJECT_TITLE) ?? options.GetString(ExportOptionKey.PROJECT_ID);

            if (replacements["JAVA_PACKAGE"].StartsWith("org.crayonlang.interpreter"))
            {
                throw new InvalidOperationException("Cannot use org.crayonlang.interpreter as the project's package.");
            }

            return replacements;
        }

        public override IDictionary<string, object> GetConstantFlags()
        {
            return new Dictionary<string, object>();
        }
    }
}
