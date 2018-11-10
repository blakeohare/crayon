using Common;
using Pastel;
using System.Collections.Generic;

namespace Exporter
{
    public static class PastelRunner
    {
        public static void CompileLibraryFiles(
            LibraryExporter library,
            Platform.AbstractPlatform platform,
            IInlineImportCodeLoader codeLoader,
            Dictionary<string, PastelContext> libraries,
            PastelContext sharedScope,
            Dictionary<string, object> constantFlags)
        {
            PastelContext context = new PastelContext(platform.Language, codeLoader);
            Dictionary<string, string> exFnTranslations = library.GetExtensibleFunctionTranslations(platform);
            List<ExtensibleFunction> libraryFunctions = library.GetPastelExtensibleFunctions();
            Dictionary<string, object> constantsLookup = Util.MergeDictionaries(constantFlags, library.CompileTimeConstants);

            foreach (ExtensibleFunction exFn in libraryFunctions)
            {
                string exFnTranslation = null;
                if (exFnTranslations.ContainsKey(exFn.Name))
                {
                    exFnTranslation = exFnTranslations[exFn.Name];
                }
                else if (exFnTranslations.ContainsKey("$" + exFn.Name))
                {
                    exFnTranslation = exFnTranslations["$" + exFn.Name];
                }

                context.AddExtensibleFunction(exFn, exFnTranslation);
            }
            context.AddDependency(sharedScope);
            foreach (string constKey in constantsLookup.Keys)
            {
                context.SetConstant(constKey, constantsLookup[constKey]);
            }

            libraries[library.Metadata.ID] = context;

            Dictionary<string, string> structCode = library.Metadata.GetStructFilesCode();
            foreach (string structFile in structCode.Keys)
            {
                string filename = "LIB:" + library.Metadata.ID + "/structs/" + structFile;
                context.CompileCode(filename, structCode[structFile]);
            }

            Dictionary<string, string> supplementalCode = library.Metadata.GetSupplementalTranslatedCode();
            foreach (string supplementalFile in supplementalCode.Keys)
            {
                string filename = "LIB:" + library.Metadata.ID + "/supplemental/" + supplementalFile;
                context.CompileCode(filename, supplementalCode[supplementalFile]);
            }

            context.FinalizeCompilation();
        }
    }
}
