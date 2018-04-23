using Common;
using Platform;
using Pastel.Nodes;
using Pastel.Transpilers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpApp
{
    public class PlatformImpl : AbstractPlatform
    {
        public override string Name { get { return "csharp-app"; } }
        public override string InheritsFrom { get { return "lang-csharp"; } }
        public override string NL { get { return "\r\n"; } }

        public PlatformImpl()
        {
            this.Translator = new CSharpTranslator();
        }

        public override IDictionary<string, object> GetConstantFlags()
        {
            return new Dictionary<string, object>();
        }

        public override Dictionary<string, string> GenerateReplacementDictionary(Options options, ResourceDatabase resDb)
        {
            List<string> embeddedResources = new List<string>()
            {
                "<EmbeddedResource Include=\"Resources\\ByteCode.txt\"/>",
                "<EmbeddedResource Include=\"Resources\\ResourceManifest.txt\"/>",
            };

            if (resDb.ImageSheetManifestFile != null)
            {
                embeddedResources.Add("<EmbeddedResource Include=\"Resources\\ImageSheetManifest.txt\"/>");
            }

            if (options.GetBool(ExportOptionKey.HAS_ICON))
            {
                embeddedResources.Add("<EmbeddedResource Include=\"icon.ico\" />");
            }

            foreach (FileOutput imageFile in resDb.ImageResources.Where(img => img.CanonicalFileName != null))
            {
                embeddedResources.Add("<EmbeddedResource Include=\"Resources\\" + imageFile.CanonicalFileName + "\"/>");
            }

            foreach (string imageSheetFileName in resDb.ImageSheetFiles.Keys)
            {
                embeddedResources.Add("<EmbeddedResource Include=\"Resources\\" + imageSheetFileName + "\"/>");
            }

            foreach (FileOutput textFile in resDb.TextResources.Where(img => img.CanonicalFileName != null))
            {
                embeddedResources.Add("<EmbeddedResource Include=\"Resources\\" + textFile.CanonicalFileName + "\"/>");
            }

            foreach (FileOutput audioFile in resDb.AudioResources.Where(file => file.CanonicalFileName != null))
            {
                embeddedResources.Add("<EmbeddedResource Include=\"Resources\\" + audioFile.CanonicalFileName + "\"/>");
            }

            foreach (FileOutput fontFile in resDb.FontResources.Where(file => file.CanonicalFileName != null))
            {
                embeddedResources.Add("<EmbeddedResource Include=\"Resources\\" + fontFile.CanonicalFileName + "\"/>");
            }

            string guidSeed = IdGenerator.GetRandomSeed();

            return Util.MergeDictionaries(
                this.ParentPlatform.GenerateReplacementDictionary(options, resDb),
                new Dictionary<string, string>() {
                    { "PROJECT_GUID", IdGenerator.GenerateCSharpGuid(options.GetStringOrNull(ExportOptionKey.GUID_SEED) ?? guidSeed, "project") },
                    { "ASSEMBLY_GUID", IdGenerator.GenerateCSharpGuid(options.GetStringOrNull(ExportOptionKey.GUID_SEED) ?? guidSeed, "assembly") },
                    { "EMBEDDED_RESOURCES", string.Join("\r\n", embeddedResources).Trim() },
                    { "CSHARP_APP_ICON", options.GetBool(ExportOptionKey.HAS_ICON) ? "<ApplicationIcon>icon.ico</ApplicationIcon>" : "" },
                });
        }

        private Dictionary<string, string> HACK_libraryProjectGuidToPath = new Dictionary<string, string>();

        public override void ExportStandaloneVm(
            Dictionary<string, FileOutput> output,
            Pastel.PastelCompiler compiler,
            Pastel.PastelContext pastelContext,
            IList<LibraryForExport> everyLibrary,
            ILibraryNativeInvocationTranslatorProvider libraryNativeInvocationTranslatorProviderForPlatform)
        {
            Dictionary<string, string> libraryProjectNameToGuid = new Dictionary<string, string>();

            string runtimeProjectGuid = IdGenerator.GenerateCSharpGuid("runtime", "runtime-project");
            string runtimeAssemblyGuid = IdGenerator.GenerateCSharpGuid("runtime", "runtime-assembly");

            Dictionary<string, string> replacements = new Dictionary<string, string>()
            {
                { "PROJECT_ID", "CrayonRuntime" },
                { "PROJECT_GUID", runtimeProjectGuid },
                { "INTERPRETER_PROJECT_GUID", runtimeProjectGuid },
                { "ASSEMBLY_GUID", runtimeAssemblyGuid },
                { "PROJECT_TITLE", "Crayon Runtime" },
                { "COPYRIGHT", "©" },
                { "CURRENT_YEAR", DateTime.Now.Year.ToString() },
                { "DLL_REFERENCES", "" },
                { "CSHARP_APP_ICON", "<ApplicationIcon>icon.ico</ApplicationIcon>" },
                { "EMBEDDED_RESOURCES", "<EmbeddedResource Include=\"icon.ico\" />" },
                { "CSHARP_CONTENT_ICON", "" },
                { "DLLS_COPIED", "" },
            };
            string baseDir = "CrayonRuntime/";

            string dllReferencesOriginal = replacements["DLL_REFERENCES"];
            string dllsCopiedOriginal = replacements["DLLS_COPIED"];
            string embeddedResources = replacements["EMBEDDED_RESOURCES"];
            replacements["EMBEDDED_RESOURCES"] = "";
            foreach (LibraryForExport library in everyLibrary)
            {
                string libBaseDir = "Libs/" + library.Name + "/";
                List<LangCSharp.DllFile> dlls = new List<LangCSharp.DllFile>();
                if (this.GetLibraryCode(libBaseDir, library, dlls, output, libraryNativeInvocationTranslatorProviderForPlatform))
                {
                    string name = library.Name;
                    string projectGuid = IdGenerator.GenerateCSharpGuid(library.Name + "|" + library.Version, "library-project");
                    replacements["PROJECT_GUID"] = projectGuid;
                    replacements["ASSEMBLY_GUID"] = IdGenerator.GenerateCSharpGuid(library.Name + "|" + library.Version, "library-assembly");
                    replacements["PROJECT_TITLE"] = library.Name;
                    replacements["LIBRARY_NAME"] = library.Name;
                    LangCSharp.DllReferenceHelper.AddDllReferencesToProjectBasedReplacements(replacements, dlls, library.LibProjectNamesAndGuids);

                    libraryProjectNameToGuid[name] = projectGuid;

                    replacements["DOT_NET_LIBS"] = Util.JoinLines(
                        library.DotNetLibs.Select(
                            dotNetLib =>
                                "    <Reference Include=\"" + dotNetLib + "\" />")
                        .ToArray());

                    this.CopyResourceAsText(output, libBaseDir + library.Name + ".sln", "ResourcesLib/Solution.txt", replacements);
                    this.CopyResourceAsText(output, libBaseDir + library.Name + ".csproj", "ResourcesLib/ProjectFile.txt", replacements);
                    this.CopyResourceAsText(output, libBaseDir + "Properties/AssemblyInfo.cs", "ResourcesLib/AssemblyInfo.txt", replacements);

                    foreach (LangCSharp.DllFile dll in dlls)
                    {
                        output[libBaseDir + dll.HintPath] = dll.FileOutput;
                    }
                }
            }
            replacements["DLL_REFERENCES"] = dllReferencesOriginal;
            replacements["DLLS_COPIED"] = dllsCopiedOriginal;
            replacements["EMBEDDED_RESOURCES"] = embeddedResources;
            replacements["PROJECT_GUID"] = runtimeProjectGuid;
            replacements["ASSEMBLY_GUID"] = runtimeAssemblyGuid;

            this.CopyTemplatedFiles(baseDir, output, replacements, true);
            this.ExportInterpreter(baseDir, output, compiler);
            this.ExportProjectFiles(baseDir, output, replacements, libraryProjectNameToGuid, true);
            this.CopyResourceAsBinary(output, baseDir + "icon.ico", "ResourcesVm/icon.ico");

            TODO.MoveCbxParserIntoTranslatedPastelCode();
            this.CopyResourceAsText(output, baseDir + "CbxDecoder.cs", "ResourcesVm/CbxDecoder.txt", replacements);
        }

        // Returns true if any export is necessary i.e. bytecode-only libraries will return false.
        private bool GetLibraryCode(
            string baseDir,
            LibraryForExport library,
            List<LangCSharp.DllFile> dllsOut,
            Dictionary<string, FileOutput> filesOut,
            ILibraryNativeInvocationTranslatorProvider libraryNativeInvocationTranslatorProviderForPlatform)
        {
            TranspilerContext ctx = new TranspilerContext();
            string libraryName = library.Name;
            ctx.CurrentLibraryFunctionTranslator = libraryNativeInvocationTranslatorProviderForPlatform.GetTranslator(libraryName);
            List<string> libraryLines = new List<string>();
            if (library.ManifestFunctionDEPRECATED != null)
            {
                string libraryDir = baseDir + "Libraries/" + libraryName;
                this.Translator.GenerateCodeForFunction(ctx, this.Translator, library.ManifestFunctionDEPRECATED);
                libraryLines.Add(ctx.FlushAndClearBuffer());
                foreach (FunctionDefinition funcDef in library.FunctionsDEPRECATED)
                {
                    this.Translator.GenerateCodeForFunction(ctx, this.Translator, funcDef);
                    libraryLines.Add(ctx.FlushAndClearBuffer());
                }

                foreach (StructDefinition structDef in library.StructsDEPRECATED)
                {
                    filesOut[libraryDir + "/Structs/" + structDef.NameToken.Value + ".cs"] = this.GetStructFile(structDef);
                }

                filesOut[libraryDir + "/LibraryWrapper.cs"] = new FileOutput()
                {
                    Type = FileOutputType.Text,
                    TextContent = string.Join(this.NL,
                        "using System;",
                        "using System.Collections.Generic;",
                        "using System.Linq;",
                        "using Interpreter;",
                        "using Interpreter.Structs;",
                        "using Interpreter.Vm;",
                        "",
                        "namespace Interpreter.Libraries." + libraryName,
                        "{",
                        "    public static class LibraryWrapper",
                        "    {",
                        IndentCodeWithSpaces(string.Join(this.NL, libraryLines), 8),
                        "    }",
                        "}",
                        ""),
                };

                foreach (ExportEntity codeFile in library.ExportEntities["COPY_CODE"])
                {
                    string targetPath = codeFile.Values["target"].Replace("%LIBRARY_PATH%", libraryDir);
                    filesOut[targetPath] = codeFile.FileOutput;
                }

                foreach (ExportEntity dllFile in library.ExportEntities["DOTNET_DLL"])
                {
                    dllsOut.Add(new LangCSharp.DllFile(dllFile));
                }

                return true;
            }

            return false;
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
            Dictionary<string, string> replacements = this.GenerateReplacementDictionary(options, resourceDatabase);
            string projectId = options.GetString(ExportOptionKey.PROJECT_ID);
            string baseDir = projectId + "/";

            this.CopyTemplatedFiles(baseDir, output, replacements, false);

            List<LangCSharp.DllFile> dlls = new List<LangCSharp.DllFile>();

            HashSet<string> dotNetLibs = new HashSet<string>();

            foreach (LibraryForExport library in libraries)
            {
                foreach (string dotNetLib in library.DotNetLibs)
                {
                    dotNetLibs.Add(dotNetLib);
                }
                this.GetLibraryCode(baseDir, library, dlls, output, libraryNativeInvocationTranslatorProviderForPlatform);
            }

            LangCSharp.DllReferenceHelper.AddDllReferencesToProjectBasedReplacements(replacements, dlls, new Dictionary<string, string>());

            replacements["DLL_REFERENCES"] += Util.JoinLines(
                dotNetLibs
                    .OrderBy(v => v.ToLower())
                    .Select(
                        dotNetLib =>
                            "    <Reference Include=\"" + dotNetLib + "\" />")
                    .ToArray());

            this.ExportInterpreter(baseDir, output, compiler);

            output[baseDir + "Resources/ByteCode.txt"] = resourceDatabase.ByteCodeFile;
            output[baseDir + "Resources/ResourceManifest.txt"] = resourceDatabase.ResourceManifestFile;
            if (resourceDatabase.ImageSheetManifestFile != null)
            {
                output[baseDir + "Resources/ImageSheetManifest.txt"] = resourceDatabase.ImageSheetManifestFile;
            }

            foreach (FileOutput imageFile in resourceDatabase.ImageResources.Where(img => img.CanonicalFileName != null))
            {
                output[baseDir + "Resources/" + imageFile.CanonicalFileName] = imageFile;
            }

            foreach (string imageSheetFileName in resourceDatabase.ImageSheetFiles.Keys)
            {
                output[baseDir + "Resources/" + imageSheetFileName] = resourceDatabase.ImageSheetFiles[imageSheetFileName];
            }

            foreach (FileOutput textFile in resourceDatabase.TextResources.Where(img => img.CanonicalFileName != null))
            {
                output[baseDir + "Resources/" + textFile.CanonicalFileName] = textFile;
            }

            foreach (FileOutput audioFile in resourceDatabase.AudioResources.Where(file => file.CanonicalFileName != null))
            {
                output[baseDir + "Resources/" + audioFile.CanonicalFileName] = audioFile;
            }

            foreach (FileOutput fontFile in resourceDatabase.FontResources.Where(file => file.CanonicalFileName != null))
            {
                output[baseDir + "Resources/" + fontFile.CanonicalFileName] = fontFile;
            }

            foreach (LangCSharp.DllFile dll in dlls)
            {
                output[baseDir + dll.HintPath] = dll.FileOutput;
            }

            if (options.GetBool(ExportOptionKey.HAS_ICON))
            {
                string iconPath = options.GetString(ExportOptionKey.ICON_PATH);
                IconGenerator iconGen = new IconGenerator();
                foreach (string path in iconPath.Split(','))
                {
                    iconGen.AddImage(new SystemBitmap(path.Trim()));
                }

                output[baseDir + "icon.ico"] = new FileOutput()
                {
                    Type = FileOutputType.Binary,
                    BinaryContent = iconGen.GenerateIconFile(),
                };
            }

            this.ExportProjectFiles(baseDir, output, replacements, new Dictionary<string, string>(), false);
        }

        private void CopyTemplatedFiles(string baseDir, Dictionary<string, FileOutput> output, Dictionary<string, string> replacements, bool isStandaloneVm)
        {
            string resourceDir = isStandaloneVm ? "ResourcesVm" : "Resources";

            // From LangCSharp
            this.CopyResourceAsText(output, baseDir + "Vm/TranslationHelper.cs", "Resources/TranslationHelper.txt", replacements);
            this.CopyResourceAsText(output, baseDir + "Vm/Library.cs", "Resources/Library.txt", replacements);
            this.CopyResourceAsText(output, baseDir + "Vm/LibraryFunctionPointer.cs", "Resources/LibraryFunctionPointer.txt", replacements);
            this.CopyResourceAsText(output, baseDir + "Vm/LibraryRegistry.cs", resourceDir + "/LibraryRegistry.txt", replacements);
            this.CopyResourceAsText(output, baseDir + "Vm/UniversalBitmap.cs", "Resources/UniversalBitmap.txt", replacements);
            this.CopyResourceAsText(output, baseDir + "Vm/UniversalFont.cs", "Resources/UniversalFont.txt", replacements);

            // Required project files
            this.CopyResourceAsText(output, baseDir + "Properties/AssemblyInfo.cs", "Resources/AssemblyInfo.txt", replacements);
            this.CopyResourceAsText(output, baseDir + "Program.cs", resourceDir + "/Program.txt", replacements);

            // CSharpOpenTK specific stuff
            this.CopyResourceAsText(output, baseDir + "Vm/PlatformTranslationHelper.cs", "Resources/PlatformTranslationHelper.txt", replacements);
            this.CopyResourceAsText(output, baseDir + "CSharpAppTranslationHelper.cs", "Resources/CSharpAppTranslationHelper.txt", replacements);
            this.CopyResourceAsText(output, baseDir + "ResourceReader.cs", resourceDir + "/ResourceReader.txt", replacements);
        }

        private FileOutput GetStructFile(StructDefinition sd)
        {
            TranspilerContext ctx = new TranspilerContext();
            this.Translator.GenerateCodeForStruct(ctx, this.Translator, sd);
            return new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = string.Join("\r\n", new string[] {
                    "using System;",
                    "using System.Collections.Generic;",
                    "",
                    "namespace Interpreter.Structs",
                    "{",
                    IndentCodeWithSpaces(ctx.FlushAndClearBuffer().Trim(), 4),
                    "}",
                    ""
                }),
            };
        }

        private void ExportInterpreter(
            string baseDir,
            Dictionary<string, FileOutput> output,
            Pastel.PastelCompiler compiler)
        {
            IList<VariableDeclaration> globals = compiler.Globals.Values.ToArray();
            IList<StructDefinition> structDefinitions = compiler.StructDefinitions.Values.ToArray();
            IList<FunctionDefinition> functionDefinitions = compiler.FunctionDefinitions.Values.ToArray();
            TranspilerContext ctx = new TranspilerContext();
            foreach (StructDefinition structDefinition in structDefinitions)
            {
                output[baseDir + "Structs/" + structDefinition.NameToken.Value + ".cs"] = this.GetStructFile(structDefinition);
            }

            List<string> coreVmFunctions = new List<string>();
            foreach (FunctionDefinition funcDef in functionDefinitions)
            {
                this.Translator.GenerateCodeForFunction(ctx, this.Translator, funcDef);
                ctx.Append("\r\n\r\n");
            }

            string functionCode = ctx.FlushAndClearBuffer();

            output[baseDir + "Vm/CrayonWrapper.cs"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = string.Join("\r\n", new string[] {
                    "using System;",
                    "using System.Collections.Generic;",
                    "using System.Linq;",
                    "using Interpreter.Structs;",
                    "",
                    "namespace Interpreter.Vm",
                    "{",
                    "    public class CrayonWrapper",
                    "    {",
                    IndentCodeWithSpaces(functionCode, 8),
                    "    }",
                    "}",
                    ""
                }),
            };

            this.Translator.GenerateCodeForGlobalsDefinitions(ctx, this.Translator, globals);
            output[baseDir + "Vm/Globals.cs"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = string.Join("\r\n", new string[]
                {
                    "using System;",
                    "using System.Collections.Generic;",
                    "using Interpreter.Structs;",
                    "",
                    "namespace Interpreter.Vm",
                    "{",
                    ctx.FlushAndClearBuffer(),
                    "}",
                    ""
                }),
            };
        }

        private void ExportProjectFiles(
            string baseDir,
            Dictionary<string, FileOutput> output,
            Dictionary<string, string> replacements,
            Dictionary<string, string> libraryProjectNameToGuid,
            bool isStandaloneVm)
        {
            string projectId = replacements["PROJECT_ID"];
            replacements["LIBRARY_PROJECT_INCLUSIONS"] = "";
            replacements["LIBRARY_PROJECT_CONFIG"] = "";
            if (libraryProjectNameToGuid.Count > 0)
            {
                string[] projects = libraryProjectNameToGuid.Keys.OrderBy(s => s.ToLower()).ToArray();
                List<string> inclusions = new List<string>();
                List<string> configs = new List<string>();
                foreach (string projectName in projects)
                {
                    string guid = libraryProjectNameToGuid[projectName].ToUpper();
                    inclusions.Add(
                        "Project(\"{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}\") = \"" +
                        projectName +
                        "\", \"Libs\\" + projectName + "\\" + projectName + ".csproj\", \"{" +
                        guid +
                        "}\"");
                    inclusions.Add("EndProject");
                    foreach (string releaseConfig in new string[] {
                        "Debug",
                        "Release",
                    })
                    {
                        foreach (string architecture in new string[] {
                            "Any CPU",
                            "x86",
                        })
                        {
                            foreach (string activeCfg in new string[] {
                                "ActiveCfg",
                                "Build.0",
                            })
                            {
                                configs.Add("\t\t{" + guid + "}." + releaseConfig + "|" + architecture + "." + activeCfg + " = " + releaseConfig + "|Any CPU");
                            }
                        }
                    }
                }
                replacements["LIBRARY_PROJECT_INCLUSIONS"] = string.Join("\r\n", inclusions);
                replacements["LIBRARY_PROJECT_CONFIG"] = string.Join("\r\n", configs);
            }

            this.CopyResourceAsText(output, projectId + ".sln", "Resources/SolutionFile.txt", replacements);
            this.CopyResourceAsText(output, projectId + "OSX.sln", "Resources/SolutionFileOsx.txt", replacements);
            string projectFileResource = (isStandaloneVm ? "ResourcesVm" : "Resources") + "/ProjectFile.txt";
            this.CopyResourceAsText(output, baseDir + "Interpreter.csproj", projectFileResource, replacements);
        }
    }
}
