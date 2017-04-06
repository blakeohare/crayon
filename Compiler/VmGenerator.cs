using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Pastel.Nodes;

namespace Crayon
{
    public enum VmGenerationMode
    {
        EXPORT_SELF_CONTAINED_PROJECT_SOURCE,
        EXPORT_VM_AND_LIBRARIES,
    }

    internal class VmGenerator
    {
        private static readonly string[] INTERPRETER_BASE_FILES = new string[] {
            "BinaryOpsUtil.pst",
            "ByteCodeLoader.pst",
            "Constants.pst",
            "Globals.pst",
            "Interpreter.pst",
            "MetadataInitializer.pst",
            "PrimitiveMethods.pst",
            "ResourceManager.pst",
            "Runner.pst",
            "Structs.pst",
            "TypesUtil.pst",
            "ValueUtil.pst",
        };

        private VmGenerationMode mode;

        private void AddTypeEnumsToConstants(Dictionary<string, object> constantFlags)
        {
            foreach (Types type in Enum.GetValues(typeof(Types)))
            {
                constantFlags["TYPE_ID_" + type.ToString()] = (int)type;
            }
        }

        private List<Platform.LibraryForExport> GetLibrariesForExport(
            Platform.AbstractPlatform platform,
            Dictionary<string, Library> librariesByName,
            Dictionary<string, object> constantFlags,
            Pastel.PastelCompiler vm)
        {
            Dictionary<string, Pastel.PastelCompiler> libraryCompilation = this.GenerateLibraryParseTree(
                platform,
                constantFlags,
                new InlineImportCodeLoader(),
                librariesByName.Values,
                vm);

            List<Platform.LibraryForExport> libraries = new List<Platform.LibraryForExport>();
            foreach (string libraryName in libraryCompilation.Keys.OrderBy(s => s))
            {
                Library library = librariesByName[libraryName];
                Platform.LibraryForExport libraryForExport = this.CreateLibraryForExport(
                    library.Name,
                    library.Version,
                    library.Resources.ExportEntities,
                    libraryCompilation[library.Name]);
                libraries.Add(libraryForExport);
            }
            return libraries;
        }

        public Dictionary<string, FileOutput> GenerateVmSourceCodeForPlatform(
            Platform.AbstractPlatform platform,
            CompilationBundle nullableCompilationBundle,
            ResourceDatabase resourceDatabase,
            ICollection<Library> relevantLibraries,
            VmGenerationMode mode)
        {
            Options options = new Options();
            Dictionary<string, object> constantFlags = platform.GetFlattenedConstantFlags() ?? new Dictionary<string, object>();
            this.mode = mode;

            this.AddTypeEnumsToConstants(constantFlags);
            Pastel.PastelCompiler vm = this.GenerateCoreVmParseTree(platform, constantFlags);

            Dictionary<string, Library> librariesByName = relevantLibraries.ToDictionary(lib => lib.Name);
            List<Platform.LibraryForExport> libraries = this.GetLibrariesForExport(platform, librariesByName, constantFlags, vm);

            LibraryNativeInvocationTranslatorProvider libTranslationProvider = 
                new LibraryNativeInvocationTranslatorProvider(
                    relevantLibraries.ToDictionary(lib => lib.Name), 
                    platform);

            if (mode == VmGenerationMode.EXPORT_SELF_CONTAINED_PROJECT_SOURCE)
            {
                options
                    .SetOption(ExportOptionKey.PROJECT_ID, nullableCompilationBundle.ProjectID)
                    .SetOption(ExportOptionKey.EMBED_BYTE_CODE, nullableCompilationBundle.GuidSeed)
                    .SetOption(ExportOptionKey.EMBED_BYTE_CODE, true)
                    .SetOption(ExportOptionKey.DEFAULT_TITLE, nullableCompilationBundle.DefaultTitle)
                    .SetOption(ExportOptionKey.LIBRARIES_USED, libraries.Cast<object>().ToArray())
                    .SetOption(ExportOptionKey.HAS_ICON, nullableCompilationBundle.IconPath != null);

                return platform.ExportProject(
                    vm.Globals.Values.OrderBy(v => v.VariableNameToken.Value).ToArray(),
                    vm.StructDefinitions.Values.OrderBy(s => s.NameToken.Value).ToArray(),
                    vm.FunctionDefinitions.Values.OrderBy(f => f.NameToken.Value).ToArray(),
                    libraries,
                    resourceDatabase,
                    options,
                    libTranslationProvider);
            }
            else
            {
                return platform.ExportStandaloneVm(
                    vm.Globals.Values.OrderBy(v => v.VariableNameToken.Value).ToArray(),
                    vm.StructDefinitions.Values.OrderBy(s => s.NameToken.Value).ToArray(),
                    vm.FunctionDefinitions.Values.OrderBy(f => f.NameToken.Value).ToArray(),
                    libraries);
            }
        }

        private Platform.LibraryForExport CreateLibraryForExport(
            string libraryName,
            string libraryVersion,
            Multimap<string, Platform.ExportEntity> exportEntities,
            Pastel.PastelCompiler compilation)
        { 
            FunctionDefinition manifestFunction = null;
            Dictionary<string, FunctionDefinition> otherFunctions = new Dictionary<string, FunctionDefinition>();
            foreach (FunctionDefinition functionDefinition in compilation.FunctionDefinitions.Values)
            {
                string functionName = functionDefinition.NameToken.Value;
                if (functionName == "lib_manifest_RegisterFunctions")
                {
                    manifestFunction = functionDefinition;
                }
                else
                {
                    otherFunctions[functionName] = functionDefinition;
                }
            }

            string[] names = otherFunctions.Keys.OrderBy(s => s).ToArray();
            FunctionDefinition[] functions = names.Select(n => otherFunctions[n]).ToArray();

            return new Platform.LibraryForExport()
            {
                Name = libraryName,
                Version = libraryVersion,
                FunctionRegisteredNamesOrNulls = names,
                Functions = functions,
                ManifestFunction = manifestFunction,
                ExportEntities = exportEntities,
            };
        }

        private Pastel.PastelCompiler GenerateCoreVmParseTree(
            Platform.AbstractPlatform platform,
            Dictionary<string, object> constantFlags)
        {
            Pastel.PastelCompiler compiler = new Pastel.PastelCompiler(
                false, 
                null, 
                constantFlags, 
                new InlineImportCodeLoader(), 
                null, 
                null);

            foreach (string file in INTERPRETER_BASE_FILES)
            {
                string code = LegacyUtil.ReadInterpreterFileInternally(file);
                compiler.CompileBlobOfCode(file, code);
            }
            compiler.Resolve();

            return compiler;
        }

        private Dictionary<string, Pastel.PastelCompiler> GenerateLibraryParseTree(
            Platform.AbstractPlatform platform,
            Dictionary<string, object> constantFlags,
            InlineImportCodeLoader codeLoader,
            ICollection<Library> relevantLibraries,
            Pastel.PastelCompiler sharedScope)
        {
            Dictionary<string, Pastel.PastelCompiler> libraries = new Dictionary<string, Pastel.PastelCompiler>();

            foreach (Library library in relevantLibraries)
            {
                string libName = library.Name;

                Dictionary<string, object> constantsLookup = Util.MergeDictionaries<string, object>(constantFlags, library.CompileTimeConstants);

                Pastel.PastelCompiler compiler = new Pastel.PastelCompiler(
                    true,
                    sharedScope,
                    constantsLookup,
                    codeLoader,
                    library.GetReturnTypesForNativeMethods(),
                    library.GetArgumentTypesForNativeMethods());
                libraries[libName] = compiler;

                Dictionary<string, string> supplementalCode = library.GetSupplementalTranslatedCode(false);
                Dictionary<string, string> pastelSupplementalCode = library.GetSupplementalTranslatedCode(true);
                Dictionary<string, string> translatedCode = library.GetNativeCode();
                // need to load from the actual Library instance, which could have come from either CRAYON_HOME or source

                string registryCode = library.GetRegistryCode();
                if (registryCode == null)
                {
                    if (supplementalCode.Count > 0 || translatedCode.Count > 0)
                    {
                        throw new InvalidOperationException("The library '" + libName + "' has translated code but no function_registry.pst file.");
                    }
                }
                else
                {
                    compiler.CompileBlobOfCode("LIB:" + libName + "/function_registry.pst", registryCode);

                    foreach (string supplementalFile in supplementalCode.Keys)
                    {
                        string code = supplementalCode[supplementalFile];
                        compiler.CompileBlobOfCode("LIB:" + libName + "/supplemental/" + supplementalFile, code);
                    }
                    foreach (string translatedFile in translatedCode.Keys)
                    {
                        string code = translatedCode[translatedFile];
                        compiler.CompileBlobOfCode("LIB:" + libName + "/translate/" + translatedFile, code);
                    }
                    compiler.Resolve();
                }
            }

            return libraries;
        }
    }
}
