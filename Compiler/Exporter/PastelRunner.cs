using Common;
using Pastel;
using System.Collections.Generic;

namespace Exporter
{
    public static class PastelRunner
    {
        private class LibraryPastelCodeLoader : IInlineImportCodeLoader
        {
            private string rootDir;
            public LibraryPastelCodeLoader(Parser.AssemblyMetadata libraryMetadata)
            {
                this.rootDir = libraryMetadata.GetPastelCodeDirectory();
            }

            public string LoadCode(string path)
            {
                string fullPath = FileUtil.JoinPath(rootDir, path);
                string content = FileUtil.ReadFileText(fullPath);
                return content;
            }
        }

        public static void Run(IInlineImportCodeLoader vmCodeLoader)
        {
            throw new System.NotImplementedException();
        }

        public static void CompileLibraryFiles(
            LibraryExporter library,
            Platform.AbstractPlatform platform,
            Dictionary<string, PastelContext> libraries,
            PastelContext sharedScope,
            Dictionary<string, object> constantFlags)
        {
            LibraryPastelCodeLoader libCodeLoader = new LibraryPastelCodeLoader(library.Metadata);
            PastelContext context = new PastelContext(platform.Language, libCodeLoader);
            Dictionary<string, string> exFnTranslations = library.GetExtensibleFunctionTranslations(platform);
            List<ExtensibleFunction> libraryFunctions = library.GetPastelExtensibleFunctions();
            Dictionary<string, object> constantsLookup = Util.MergeDictionaries(constantFlags, library.CompileTimeConstants);

            string functionPrefixOfSharedScope = platform.GetInterpreterFunctionInvocationPrefix();

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
            context.AddDependency(sharedScope, functionPrefixOfSharedScope);
            foreach (string constKey in constantsLookup.Keys)
            {
                context.SetConstant(constKey, constantsLookup[constKey]);
            }

            libraries[library.Metadata.ID] = context;

            Dictionary<string, string> structCode = library.Metadata.GetStructFilesCode();
            foreach (string structFile in structCode.Keys)
            {
                // TODO(pastel-split): remove this, once migrated.
                throw new System.Exception("Nope. All struct code should just go directly in {library}/pastel, now.");
                //string filename = "LIB:" + library.Metadata.ID + "/structs/" + structFile;
                //context.CompileCode(filename, structCode[structFile]);
            }

            string pastelCodeDir = library.Metadata.GetPastelCodeDirectory();
            string entryPoint = FileUtil.JoinPath(pastelCodeDir, "main.pst");

            string filename = "LIB:" + library.Metadata.ID + "/pastel/main.pst";
            context.CompileCode(filename, FileUtil.ReadFileText(entryPoint));

            context.FinalizeCompilation();
        }
    }
}
