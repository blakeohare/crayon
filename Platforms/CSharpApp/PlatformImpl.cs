using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Platform;
using Pastel.Nodes;
using System.Text;

namespace GameCSharpOpenTk
{
    public class PlatformImpl : AbstractPlatform
    {
        public override string Name { get { return "csharp-app"; } }
        public override string InheritsFrom { get { return "lang-csharp"; } }
        public override string NL { get { return "\r\n"; } }

        public PlatformImpl()
        {
            this.Translator = new CSharpAppTranslator(this);
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
                "<EmbeddedResource Include=\"Resources\\ImageSheetManifest.txt\"/>",
            };

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

            return Util.MergeDictionaries(
                this.ParentPlatform.GenerateReplacementDictionary(options, resDb),
                new Dictionary<string, string>() {
                    { "PROJECT_GUID", CSharpHelper.GenerateGuid(options.GetStringOrNull(ExportOptionKey.GUID_SEED), "project") },
                    { "ASSEMBLY_GUID", CSharpHelper.GenerateGuid(options.GetStringOrNull(ExportOptionKey.GUID_SEED), "assembly") },
                    { "EMBEDDED_RESOURCES", string.Join("\r\n", embeddedResources).Trim() },
                    { "CSHARP_APP_ICON", options.GetBool(ExportOptionKey.HAS_ICON) ? "<ApplicationIcon>icon.ico</ApplicationIcon>" : "" },
                });
        }
        
        private Dictionary<string, string> HACK_libraryProjectGuidToPath = new Dictionary<string, string>();

        public override Dictionary<string, FileOutput> ExportStandaloneVm(
            IList<VariableDeclaration> globals,
            IList<StructDefinition> structDefinitions,
            IList<FunctionDefinition> functionDefinitions,
            IList<LibraryForExport> everyLibrary,
            ILibraryNativeInvocationTranslatorProvider libraryNativeInvocationTranslatorProviderForPlatform)
        {
            Dictionary<string, string> libraryProjectNameToGuid = new Dictionary<string, string>();

            Dictionary<string, FileOutput> output = new Dictionary<string, FileOutput>();
            Dictionary<string, string> replacements = new Dictionary<string, string>()
            {
                { "PROJECT_ID", "CrayonRuntime" },
                { "PROJECT_GUID", CSharpHelper.GenerateGuid("runtime", "runtime-project") },
                { "ASSEMBLY_GUID", CSharpHelper.GenerateGuid("runtime", "runtime-assembly") },
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

            foreach (LibraryForExport library in everyLibrary)
            {
                string libBaseDir = "Libs/" + library.Name + "/";
                List<LangCSharp.DllFile> dlls = new List<LangCSharp.DllFile>();
                if (this.GetLibraryCode(libBaseDir, library, dlls, output, libraryNativeInvocationTranslatorProviderForPlatform))
                {
                    string name = library.Name;
                    string projectGuid = CSharpHelper.GenerateGuid(library.Name + "|" + library.Version, "library-project");
                    replacements["PROJECT_GUID"] = projectGuid;
                    replacements["ASSEMBLY_GUID"] = CSharpHelper.GenerateGuid(library.Name + "|" + library.Version, "library-assembly");
                    replacements["PROJECT_TITLE"] = library.Name;
                    replacements["LIBRARY_NAME"] = library.Name;

                    libraryProjectNameToGuid[name] = projectGuid;

                    this.CopyResourceAsText(output, libBaseDir + library.Name + ".sln", "ResourcesLib/Solution.txt", replacements);
                    this.CopyResourceAsText(output, libBaseDir + library.Name + ".csproj", "ResourcesLib/ProjectFile.txt", replacements);
                    this.CopyResourceAsText(output, libBaseDir + "Properties/AssemblyInfo.cs", "ResourcesLib/AssemblyInfo.txt", replacements);
                }
            }

            this.CopyTemplatedFiles(baseDir, output, replacements, true);
            this.ExportInterpreter(baseDir, output, globals, structDefinitions, functionDefinitions);
            this.ExportProjectFiles(baseDir, output, replacements, libraryProjectNameToGuid);
            this.CopyResourceAsBinary(output, baseDir + "icon.ico", "ResourcesVm/icon.ico");

            TODO.MoveCbxParserIntoTranslatedPastelCode();
            this.CopyResourceAsText(output, baseDir + "CbxDecoder.cs", "ResourcesVm/CbxDecoder.txt", replacements);

            return output;
        }

        // Returns true if any export is necessary i.e. bytecode-only libraries will return false.
        private bool GetLibraryCode(string baseDir, LibraryForExport library, List<LangCSharp.DllFile> dllsOut, Dictionary<string, FileOutput> filesOut,
            ILibraryNativeInvocationTranslatorProvider libraryNativeInvocationTranslatorProviderForPlatform)
        {
            string libraryName = library.Name;
            this.Translator.CurrentLibraryFunctionTranslator = libraryNativeInvocationTranslatorProviderForPlatform.GetTranslator(libraryName);
            List<string> libraryLines = new List<string>();
            if (library.ManifestFunction != null)
            {
                string libraryDir = baseDir + "Libraries/" + libraryName;
                libraryLines.Add(this.GenerateCodeForFunction(this.Translator, library.ManifestFunction));
                foreach (FunctionDefinition funcDef in library.Functions)
                {
                    libraryLines.Add(this.GenerateCodeForFunction(this.Translator, funcDef));
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
                        this.IndentCodeWithSpaces(string.Join(this.NL, libraryLines), 8),
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

        public override Dictionary<string, FileOutput> ExportProject(
            IList<VariableDeclaration> globals,
            IList<StructDefinition> structDefinitions,
            IList<FunctionDefinition> functionDefinitions,
            IList<LibraryForExport> libraries,
            ResourceDatabase resourceDatabase,
            Options options,
            ILibraryNativeInvocationTranslatorProvider libraryNativeInvocationTranslatorProviderForPlatform)
        {
            Dictionary<string, string> replacements = this.GenerateReplacementDictionary(options, resourceDatabase);
            Dictionary<string, FileOutput> output = new Dictionary<string, FileOutput>();
            string projectId = options.GetString(ExportOptionKey.PROJECT_ID);
            string baseDir = projectId + "/";

            this.CopyTemplatedFiles(baseDir, output, replacements, false);

            List<LangCSharp.DllFile> dlls = new List<LangCSharp.DllFile>();
            
            foreach (LibraryForExport library in libraries)
            {
                this.GetLibraryCode(baseDir, library, dlls, output, libraryNativeInvocationTranslatorProviderForPlatform);
            }

            LangCSharp.DllReferenceHelper.AddDllReferencesToProjectBasedReplacements(replacements, dlls);

            this.ExportInterpreter(baseDir, output, globals, structDefinitions, functionDefinitions);

            output[baseDir + "Resources/ByteCode.txt"] = resourceDatabase.ByteCodeFile;
            output[baseDir + "Resources/ResourceManifest.txt"] = resourceDatabase.ResourceManifestFile;
            output[baseDir + "Resources/ImageSheetManifest.txt"] = resourceDatabase.ImageSheetManifestFile;

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

            foreach (LangCSharp.DllFile dll in dlls)
            {
                output[baseDir + dll.HintPath] = dll.FileOutput;
            }

            this.ExportProjectFiles(baseDir, output, replacements, new Dictionary<string, string>());

            return output;
        }

        private void CopyTemplatedFiles(string baseDir, Dictionary<string, FileOutput> output, Dictionary<string, string> replacements, bool isStandaloneVm)
        {
            string resourceDir = isStandaloneVm ? "ResourcesVm" : "Resources";

            // From LangCSharp
            this.CopyResourceAsText(output, baseDir + "Vm/TranslationHelper.cs", "Resources/TranslationHelper.txt", replacements);
            this.CopyResourceAsText(output, baseDir + "Vm/Library.cs", "Resources/Library.txt", replacements);
            this.CopyResourceAsText(output, baseDir + "Vm/LibraryFunctionPointer.cs", "Resources/LibraryFunctionPointer.txt", replacements);
            this.CopyResourceAsText(output, baseDir + "Vm/LibraryRegistry.cs", resourceDir + "/LibraryRegistry.txt", replacements);

            // Required project files
            this.CopyResourceAsText(output, baseDir + "Properties/AssemblyInfo.cs", "Resources/AssemblyInfo.txt", replacements);
            this.CopyResourceAsText(output, baseDir + "Program.cs", resourceDir + "/Program.txt", replacements);

            // CSharpOpenTK specific stuff
            this.CopyResourceAsText(output, baseDir + "CSharpAppTranslationHelper.cs", "Resources/CSharpAppTranslationHelper.txt", replacements);
            this.CopyResourceAsText(output, baseDir + "ResourceReader.cs", resourceDir + "/ResourceReader.txt", replacements);
        }

        private void ExportInterpreter(
            string baseDir,
            Dictionary<string, FileOutput> output,
            IList<VariableDeclaration> globals,
            IList<StructDefinition> structDefinitions,
            IList<FunctionDefinition> functionDefinitions)
        {
            foreach (StructDefinition structDefinition in structDefinitions)
            {
                output[baseDir + "Structs/" + structDefinition.NameToken.Value + ".cs"] = new FileOutput()
                {
                    Type = FileOutputType.Text,
                    TextContent = string.Join("\r\n", new string[] {
                        "using System;",
                        "using System.Collections.Generic;",
                        "",
                        "namespace Interpreter.Structs",
                        "{",
                        this.IndentCodeWithSpaces(this.GenerateCodeForStruct(structDefinition).Trim(), 4),
                        "}",
                        ""
                    }),
                };
            }

            List<string> coreVmFunctions = new List<string>();
            foreach (FunctionDefinition funcDef in functionDefinitions)
            {
                coreVmFunctions.Add(this.GenerateCodeForFunction(this.Translator, funcDef));
            }

            string functionCode = string.Join("\r\n\r\n", coreVmFunctions);

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
                    this.IndentCodeWithSpaces(functionCode, 8),
                    "    }",
                    "}",
                    ""
                }),
            };

            StringBuilder globalsCode = new StringBuilder();
            this.Translator.TranslateExecutables(globalsCode, globals.Cast<Executable>().ToArray());

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
                    this.GenerateCodeForGlobalsDefinitions(this.Translator, globals),
                    "}",
                    ""
                }),
            };
        }

        private void ExportProjectFiles(
            string baseDir,
            Dictionary<string, FileOutput> output,
            Dictionary<string, string> replacements,
            Dictionary<string, string> libraryProjectNameToGuid)
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
                    configs.Add("\t{" + guid + "}.Debug|Any CPU.ActiveCfg = Debug|Any CPU");
                    configs.Add("\t{" + guid + "}.Debug|Any CPU.Build.0 = Debug|Any CPU");
                    configs.Add("\t{" + guid + "}.Debug|x86.ActiveCfg = Debug|Any CPU");
                    configs.Add("\t{" + guid + "}.Debug|x86.Build.0 = Debug|Any CPU");
                    configs.Add("\t{" + guid + "}.Release|Any CPU.ActiveCfg = Debug|Any CPU");
                    configs.Add("\t{" + guid + "}.Release|Any CPU.Build.0 = Debug|Any CPU");
                    configs.Add("\t{" + guid + "}.Release|x86.ActiveCfg = Debug|Any CPU");
                    configs.Add("\t{" + guid + "}.Release|x86.Build.0 = Debug|Any CPU");
                }
                replacements["LIBRARY_PROJECT_INCLUSIONS"] = string.Join("\r\n", inclusions);
                replacements["LIBRARY_PROJECT_CONFIG"] = string.Join("\r\n", configs);
            }
            
            this.CopyResourceAsText(output, projectId + ".sln", "Resources/SolutionFile.txt", replacements);
            this.CopyResourceAsText(output, baseDir + "Interpreter.csproj", "Resources/ProjectFile.txt", replacements);
        }

        public override string GenerateCodeForGlobalsDefinitions(AbstractTranslator translator, IList<VariableDeclaration> globals)
        {
            return this.ParentPlatform.GenerateCodeForGlobalsDefinitions(this.Translator, globals);
        }

        public override string GenerateCodeForFunction(AbstractTranslator translator, FunctionDefinition funcDef)
        {
            return this.ParentPlatform.GenerateCodeForFunction(this.Translator, funcDef);
        }

        public override string GenerateCodeForStruct(StructDefinition structDef)
        {
            return this.ParentPlatform.GenerateCodeForStruct(structDef);
        }
    }
}
