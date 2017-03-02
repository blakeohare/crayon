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

        public Dictionary<string, FileOutput> GenerateVmSourceCodeForPlatform(
            Platform.AbstractPlatform platform,
            CompilationBundle nullableCompilationBundle,
            ICollection<Library> relevantLibraries,
            VmGenerationMode mode)
        {
            Options options = new Options();
            Dictionary<string, object> constantFlags = platform.GetFlattenedConstantFlags() ?? new Dictionary<string, object>();
            InlineImportCodeLoader codeLoader = new InlineImportCodeLoader();
            this.mode = mode;

            this.AddTypeEnumsToConstants(constantFlags);

            Pastel.PastelCompiler vm = this.GenerateCoreVmParseTree(platform, constantFlags, codeLoader);
            Dictionary<string, Pastel.PastelCompiler> libraries = this.GenerateLibraryParseTree(
                platform,
                constantFlags,
                codeLoader,
                relevantLibraries,
                vm);

            if (mode == VmGenerationMode.EXPORT_SELF_CONTAINED_PROJECT_SOURCE)
            {
                options
                    .SetOption(ExportOptionKey.PROJECT_ID, nullableCompilationBundle.ProjectID)
                    .SetOption(ExportOptionKey.EMBED_BYTE_CODE, nullableCompilationBundle.GuidSeed)
                    .SetOption(ExportOptionKey.EMBED_BYTE_CODE, true)
                    .SetOption(ExportOptionKey.HAS_ICON, nullableCompilationBundle.IconPath != null);

                return platform.ExportProject(
                    vm.Globals.Values.OrderBy(v => v.VariableName.Value).ToArray(),
                    vm.StructDefinitions.Values.OrderBy(s => s.NameToken.Value).ToArray(),
                    vm.FunctionDefinitions.Values.OrderBy(f => f.NameToken.Value).ToArray(),
                    options);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private Pastel.PastelCompiler GenerateCoreVmParseTree(
            Platform.AbstractPlatform platform,
            Dictionary<string, object> constantFlags,
            InlineImportCodeLoader codeLoader)
        {
            Pastel.PastelCompiler compiler = new Pastel.PastelCompiler(false, null, constantFlags, codeLoader, null, null);

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
                Pastel.PastelCompiler compiler = new Pastel.PastelCompiler(
                    true,
                    sharedScope,
                    constantFlags,
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
