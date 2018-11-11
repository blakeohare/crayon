using Common;
using Parser;
using Pastel;
using System.Collections.Generic;
using System.Linq;

namespace Exporter
{
    public static class PastelRunner
    {
        private class LibraryPastelCodeLoader : IInlineImportCodeLoader
        {
            private string rootDir;
            public LibraryPastelCodeLoader(AssemblyMetadata libraryMetadata)
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

        public static void Run(IInlineImportCodeLoader vmCodeLoader, Platform.IPlatformProvider platformProvider)
        {
            AssemblyMetadata[] assemblyMetadataList = new AssemblyFinder().AssemblyFlatList
                .Where(asm => asm.IsMoreThanJustEmbedCode)
                .ToArray();

            Platform.AbstractPlatform[] platforms = new string[] {
                "csharp-app",
                "java-app",
                "javascript-app",
                "python-app",
            }.Select(name => platformProvider.GetPlatform(name)).ToArray();

            foreach (Platform.AbstractPlatform platform in platforms)
            {
                string vmSourceOutput = System.IO.Path.GetFullPath(FileUtil.JoinPath(VmCodeLoader.GetCrayonSourceDirectory(), "..", "gen", platform.Name));

                Dictionary<string, object> constants = platform.GetFlattenedConstantFlags(false);
                VmGenerator.AddTypeEnumsToConstants(constants);

                PastelContext vmContext = new PastelContext(platform.Language, vmCodeLoader);
                foreach (string key in constants.Keys)
                {
                    vmContext.SetConstant(key, constants[key]);
                }

                vmContext.CompileFile("main.pst");
                vmContext.FinalizeCompilation();

                Dictionary<string, string> vmGeneratedFiles = GetGeneratedFiles(vmContext);

                SaveTemplateFiles(vmGeneratedFiles, platform.Language, vmSourceOutput);

                foreach (AssemblyMetadata metadata in assemblyMetadataList)
                {
                    try
                    {
                        Dictionary<string, PastelContext> compilation = new Dictionary<string, PastelContext>();

                        LibraryExporter libExporter = LibraryExporter.Get(metadata, platform);
                        CompileLibraryFiles(libExporter, platform, compilation, vmContext, constants);
                        PastelContext pastelContext = compilation.Values.FirstOrDefault();

                        Dictionary<string, string> templatesForLibrary = GetGeneratedFiles(pastelContext);
                        string outputDirectory = System.IO.Path.GetFullPath(System.IO.Path.Combine(metadata.GetPastelCodeDirectory(), "..", "resources", platform.Name));
                        SaveTemplateFiles(templatesForLibrary, platform.Language, outputDirectory);
                    }
                    catch (ExtensionMethodNotImplementedException emie)
                    {
                        System.Console.WriteLine("Skipping " + metadata.CanonicalKey + " | " + platform.Name + " because: " + emie.Message);
                    }
                }
            }
        }

        private static void SaveTemplateFiles(Dictionary<string, string> templates, string language, string directory)
        {
            FileUtil.EnsureFolderExists(directory);
            string dir = directory + "/";
            string ext;
            switch (language)
            {
                case "C": ext = ".c"; break;
                case "CSHARP": ext = ".cs"; break;
                case "JAVASCRIPT": ext = ".js"; break;
                case "PYTHON": ext = ".py"; break;
                case "JAVA": ext = ".java"; break;
                default: ext = ".txt"; break;
            }

            foreach (string key in templates.Keys)
            {
                string[] parts = key.Split(':');
                switch (parts[0])
                {
                    case "func_decl":
                        FileUtil.WriteFileText(dir + "FunctionDeclarations" + ext, templates[key]);
                        break;
                    case "func_impl":
                        FileUtil.WriteFileText(dir + "Functions" + ext, templates[key]);
                        break;
                    case "struct_decl":
                        FileUtil.WriteFileText(dir + "StructHeader_" + parts[1] + ext, templates[key]);
                        break;
                    case "struct_def":
                        FileUtil.WriteFileText(dir + "Struct_" + parts[1] + ext, templates[key]);
                        break;
                    default:
                        throw new System.Exception();
                }
            }
        }

        private static Dictionary<string, string> GetGeneratedFiles(PastelContext context)
        {
            Dictionary<string, string> output = new Dictionary<string, string>();
            if (context.UsesFunctionDeclarations)
            {
                output["func_decl"] = context.GetCodeForFunctionDeclarations();
            }
            output["func_impl"] = context.GetCodeForFunctions();
            if (context.UsesStructDefinitions)
            {
                Dictionary<string, string> structDefinitions = context.GetCodeForStructs();
                string[] structNames = structDefinitions.Keys.OrderBy(k => k.ToLower()).ToArray();

                foreach (string structName in structNames)
                {
                    output["struct_def:" + structName] = structDefinitions[structName];
                }

                if (context.UsesStructDeclarations)
                {
                    Dictionary<string, string> structDeclarations = structNames.ToDictionary(k => context.GetCodeForStructDeclaration(k));

                    foreach (string structName in structNames)
                    {
                        output["struct_decl:" + structName] = structDeclarations[structName];
                    }
                }
            }
            return output;
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

            string pastelCodeDir = library.Metadata.GetPastelCodeDirectory();
            string entryPoint = FileUtil.JoinPath(pastelCodeDir, "main.pst");
            string filename = "LIB:" + library.Metadata.ID + "/pastel/main.pst";
            context.CompileCode(filename, FileUtil.ReadFileText(entryPoint));

            context.FinalizeCompilation();
        }
    }
}
