using Common;
using Parser;
using Pastel;
using Pastel.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Exporter
{
    public enum VmGenerationMode
    {
        EXPORT_SELF_CONTAINED_PROJECT_SOURCE,
        EXPORT_VM_AND_LIBRARIES,
    }

    public class VmGenerator
    {
        private static readonly string[] INTERPRETER_BASE_FILES = {
            "BinaryOpsUtil.pst",
            "ByteCodeLoader.pst",
            "Constants.pst",
            "Globals.pst",
            "Interpreter.pst",
            "MetadataInitializer.pst",
            "PrimitiveMethods.pst",
            "Reflection.pst",
            "ResourceManager.pst",
            "Runner.pst",
            "Structs.pst",
            "TypesUtil.pst",
            "ValueUtil.pst",
        };

        private void AddTypeEnumsToConstants(Dictionary<string, object> constantFlags)
        {
            foreach (Types type in Enum.GetValues(typeof(Types)))
            {
                constantFlags["TYPE_ID_" + type.ToString()] = (int)type;
            }
        }

        private List<Platform.LibraryForExport> GetLibrariesForExport(
            Platform.AbstractPlatform platform,
            Dictionary<string, LibraryMetadata> librariesById,
            Dictionary<string, object> constantFlags,
            IInlineImportCodeLoader codeLoader,
            Pastel.PastelCompiler vm)
        {
            using (new PerformanceSection("VmGenerator.GetLibrariesForExport"))
            {
                Dictionary<string, Pastel.PastelCompiler> libraryCompilation = this.GenerateLibraryParseTree(
                platform,
                constantFlags,
                codeLoader,
                librariesById.Values,
                vm);

                List<Platform.LibraryForExport> libraries = new List<Platform.LibraryForExport>();
                Dictionary<string, LibraryExporter> libraryByName = new Dictionary<string, LibraryExporter>();
                foreach (string libraryId in libraryCompilation.Keys.OrderBy(s => s))
                {
                    LibraryExporter library = LibraryExporter.Get(librariesById[libraryId], platform);
                    libraryByName[library.Metadata.ID] = library;
                    Platform.LibraryForExport libraryForExport = this.CreateLibraryForExport(
                        library.Metadata.ID,
                        library.Metadata.Version,
                        libraryCompilation[library.Metadata.ID],
                        library.Resources);
                    libraries.Add(libraryForExport);
                }

                // Now that all libraries are read and initialized, go through and resolve all deferred DLL's that required all libraries to be loaded.
                foreach (Platform.LibraryForExport lfe in libraries)
                {
                    foreach (Platform.ExportEntity ee in lfe.ExportEntities.GetValueEnumerator())
                    {
                        if (ee.DeferredFileOutputBytesLibraryName != null)
                        {
                            LibraryExporter sourceLibrary;
                            if (!libraryByName.TryGetValue(ee.DeferredFileOutputBytesLibraryName, out sourceLibrary))
                            {
                                throw new InvalidOperationException("The library '" + lfe.Name + "' makes reference to another library '" + ee.DeferredFileOutputBytesLibraryName + "' which could not be found.");
                            }

                            string resourcePath = "resources/" + ee.DeferredFileOutputBytesLibraryPath;
                            byte[] dllFile = sourceLibrary.Metadata.ReadFileBytes(resourcePath);
                            if (dllFile == null)
                            {
                                throw new InvalidOperationException("Could not find file: '" + resourcePath + "' in library '" + sourceLibrary.Metadata.ID + "'");
                            }
                            ee.FileOutput = new FileOutput()
                            {
                                Type = FileOutputType.Binary,
                                BinaryContent = dllFile
                            };
                        }
                    }
                }

                return libraries;
            }
        }

        public void GenerateVmSourceCodeForPlatform(
            Dictionary<string, FileOutput> output,
            Platform.AbstractPlatform platform,
            CompilationBundle nullableCompilationBundle,
            ResourceDatabase resourceDatabase,
            ICollection<LibraryMetadata> relevantLibraries,
            string verifiedAbsoluteOutputPath,
            IInlineImportCodeLoader codeLoader,
            VmGenerationMode mode)
        {
            using (new PerformanceSection("VmGenerator.GenerateVmSourceCodeForPlatform"))
            {
                Options options = new Options();
                Dictionary<string, object> constantFlags = platform.GetFlattenedConstantFlags() ?? new Dictionary<string, object>();

                this.AddTypeEnumsToConstants(constantFlags);
                PastelCompiler vm = this.GenerateCoreVmParseTree(platform, codeLoader, constantFlags);

                Dictionary<string, LibraryMetadata> librariesByID = relevantLibraries.ToDictionary(lib => lib.ID);
                List<Platform.LibraryForExport> libraries = this.GetLibrariesForExport(platform, librariesByID, constantFlags, codeLoader, vm);

                LibraryNativeInvocationTranslatorProvider libTranslationProvider =
                    new LibraryNativeInvocationTranslatorProvider(
                        relevantLibraries.ToDictionary(lib => lib.ID),
                        libraries,
                        platform);

                if (mode == VmGenerationMode.EXPORT_SELF_CONTAINED_PROJECT_SOURCE)
                {
                    options
                        .SetOption(ExportOptionKey.PROJECT_ID, nullableCompilationBundle.ProjectID)
                        .SetOption(ExportOptionKey.DESCRIPTION, nullableCompilationBundle.Description)
                        .SetOption(ExportOptionKey.VERSION, nullableCompilationBundle.Version)
                        .SetOption(ExportOptionKey.EMBED_BYTE_CODE, nullableCompilationBundle.GuidSeed)
                        .SetOption(ExportOptionKey.EMBED_BYTE_CODE, true)
                        .SetOption(ExportOptionKey.DEFAULT_TITLE, nullableCompilationBundle.DefaultTitle)
                        .SetOption(ExportOptionKey.LIBRARIES_USED, libraries.Cast<object>().ToArray())
                        .SetOption(ExportOptionKey.HAS_ICON, nullableCompilationBundle.IconPath != null)
                        .SetOption(ExportOptionKey.HAS_LAUNCHSCREEN, nullableCompilationBundle.LaunchScreenPath != null)
                        .SetOption(ExportOptionKey.IOS_BUNDLE_PREFIX, nullableCompilationBundle.IosBundlePrefix)
                        .SetOption(ExportOptionKey.JAVA_PACKAGE, nullableCompilationBundle.JavaPackage)
                        .SetOption(ExportOptionKey.JS_FILE_PREFIX, nullableCompilationBundle.JsFilePrefix)
                        .SetOption(ExportOptionKey.JS_FULL_PAGE, nullableCompilationBundle.JsFullPage)
                        .SetOption(ExportOptionKey.SUPPORTED_ORIENTATION, nullableCompilationBundle.Orientations);

                    if (options.GetBool(ExportOptionKey.HAS_ICON)) options.SetOption(ExportOptionKey.ICON_PATH, nullableCompilationBundle.IconPath);
                    if (options.GetBool(ExportOptionKey.HAS_LAUNCHSCREEN)) options.SetOption(ExportOptionKey.LAUNCHSCREEN_PATH, nullableCompilationBundle.LaunchScreenPath);

                    platform.GleanInformationFromPreviouslyExportedProject(options, verifiedAbsoluteOutputPath);

                    platform.ExportProject(
                        output,
                        vm,
                        null,
                        libraries,
                        resourceDatabase,
                        options,
                        libTranslationProvider);
                }
                else
                {
                    platform.ExportStandaloneVm(
                        output,
                        vm,
                        null,
                        libraries,
                        libTranslationProvider);
                }
            }
        }

        private Platform.LibraryForExport CreateLibraryForExport(
            string libraryName,
            string libraryVersion,
            Pastel.PastelCompiler compilation,
            LibraryResourceDatabase libResDb)
        {
            using (new PerformanceSection("VmGenerator.CreateLibraryForExport"))
            {
                Multimap<string, Platform.ExportEntity> exportEntities = libResDb.ExportEntities;
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
                string[] dotNetLibs = libResDb.DotNetLibs.OrderBy(s => s.ToLower()).ToArray();

                return new Platform.LibraryForExport()
                {
                    Name = libraryName,
                    Version = libraryVersion,
                    FunctionRegisteredNamesOrNulls = names,
                    Functions = functions,
                    Structs = compilation.StructDefinitions.Values.ToArray(),
                    ManifestFunction = manifestFunction,
                    ExportEntities = exportEntities,
                    DotNetLibs = dotNetLibs,
                    LibProjectNamesAndGuids = libResDb.ProjectReferenceToGuid,
                };
            }
        }

        private Pastel.PastelCompiler GenerateCoreVmParseTree(
            Platform.AbstractPlatform platform,
            IInlineImportCodeLoader codeLoader,
            Dictionary<string, object> constantFlags)
        {
            using (new PerformanceSection("VmGenerator.GenerateCoreVmParseTree"))
            {
                Pastel.PastelCompiler compiler = new Pastel.PastelCompiler(
                new Pastel.PastelCompiler[0],
                constantFlags,
                codeLoader,
                new Pastel.ExtensibleFunction[0]);

                foreach (string file in INTERPRETER_BASE_FILES)
                {
                    string code = codeLoader.LoadCode(file);
                    compiler.CompileBlobOfCode(file, code);
                }
                compiler.Resolve();

                return compiler;
            }
        }

        private Dictionary<string, PastelCompiler> GenerateLibraryParseTree(
            Platform.AbstractPlatform platform,
            Dictionary<string, object> constantFlags,
            IInlineImportCodeLoader codeLoader,
            ICollection<LibraryMetadata> relevantLibraries,
            PastelCompiler sharedScope)
        {
            using (new PerformanceSection("VmGenerator.GenerateLibraryParseTree"))
            {
                Dictionary<string, PastelCompiler> libraries = new Dictionary<string, PastelCompiler>();

                foreach (LibraryMetadata libraryMetadata in relevantLibraries)
                {
                    LibraryExporter library = LibraryExporter.Get(libraryMetadata, platform);

                    Dictionary<string, object> constantsLookup = Util.MergeDictionaries<string, object>(constantFlags, library.CompileTimeConstants);

                    List<ExtensibleFunction> libraryFunctions = library.GetPastelExtensibleFunctions();

                    PastelCompiler compiler = new PastelCompiler(
                        new List<PastelCompiler> { sharedScope },
                        constantsLookup,
                        codeLoader,
                        libraryFunctions);
                    libraries[library.Metadata.ID] = compiler;

                    Dictionary<string, string> supplementalCode = library.Metadata.GetSupplementalTranslatedCode();
                    Dictionary<string, string> translatedCode = library.GetNativeCode();
                    Dictionary<string, string> structCode = library.Metadata.GetStructFilesCode();
                    // need to load from the actual Library instance, which could have come from either CRAYON_HOME or source

                    string registryCode = library.Metadata.GetRegistryCode();
                    if (registryCode == null)
                    {
                        if (supplementalCode.Count > 0 || translatedCode.Count > 0)
                        {
                            throw new InvalidOperationException("The library '" + library.Metadata.ID + "' has translated code but no function_registry.pst file.");
                        }
                    }
                    else
                    {
                        compiler.CompileBlobOfCode("LIB:" + library.Metadata.ID + "/function_registry.pst", registryCode);

                        foreach (string structFile in structCode.Keys)
                        {
                            string code = structCode[structFile];
                            compiler.CompileBlobOfCode("LIB:" + library.Metadata.ID + "/structs/" + structFile, code);
                        }

                        foreach (string supplementalFile in supplementalCode.Keys)
                        {
                            string code = supplementalCode[supplementalFile];
                            compiler.CompileBlobOfCode("LIB:" + library.Metadata.ID + "/supplemental/" + supplementalFile, code);
                        }
                        foreach (string translatedFile in translatedCode.Keys)
                        {
                            string code = translatedCode[translatedFile];
                            compiler.CompileBlobOfCode("LIB:" + library.Metadata.ID + "/translate/" + translatedFile, code);
                        }
                        compiler.Resolve();
                    }
                }

                return libraries;
            }
        }
    }
}
