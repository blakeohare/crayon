using Common;
using Pastel.Transpilers;
using Platform;
using System;
using System.Collections.Generic;

namespace LangCSharp
{
    public class PlatformImpl : AbstractPlatform
    {
        public override string Name { get { return "lang-csharp"; } }
        public override string InheritsFrom { get { return null; } }
        public override string NL { get { return "\r\n"; } }

        public PlatformImpl() : base()
        {
        }

        public override IDictionary<string, object> GetConstantFlags()
        {
            return new Dictionary<string, object>()
                {
                    { "IS_ASYNC", true },
                    { "PLATFORM_SUPPORTS_LIST_CLEAR", true },
                    { "STRONGLY_TYPED", true },
                    { "IS_ARRAY_SAME_AS_LIST", false },
                    { "IS_PYTHON", false },
                    { "IS_CHAR_A_NUMBER", true },
                    { "INT_IS_FLOOR", false },
                    { "IS_THREAD_BLOCKING_ALLOWED", true },
                    { "HAS_INCREMENT", true },
                };
        }

        public override void ExportStandaloneVm(
            Dictionary<string, FileOutput> output,
            Pastel.PastelCompiler compiler,
            Pastel.PastelContext pastelContext,
            IList<LibraryForExport> everyLibrary,
            ILibraryNativeInvocationTranslatorProvider libraryNativeInvocationTranslatorProviderForPlatform)
        {
            throw new NotImplementedException();
        }

        public override void ExportProject(
            Dictionary<string, FileOutput> output,
            Pastel.PastelCompiler compiler,
            Pastel.PastelContext pastelContext,
            IList<LibraryForExport> libraries,
            ResourceDatabase resourceDatabase,
            Options options,
            ILibraryNativeInvocationTranslatorProvider libraryNativeInvocationTranslatorProviderForPlatform)
        {
            throw new InvalidOperationException("This platform does not support direct export.");
        }

        public override Dictionary<string, string> GenerateReplacementDictionary(Options options, ResourceDatabase resDb)
        {
            Dictionary<string, string> replacements = AbstractPlatform.GenerateGeneralReplacementsDictionary(options);
            replacements["PROJECT_GUID"] = "project guid goes here.";
            return replacements;
        }
    }
}
