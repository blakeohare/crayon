using Common;
using CommonUtil;
using CommonUtil.Images;
using CommonUtil.Random;
using Platform;
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
            : base("CSHARP")
        { }

        public override IDictionary<string, object> GetConstantFlags()
        {
            return new Dictionary<string, object>()
            {
                { "HAS_DEBUGGER", true },
            };
        }

        public override Dictionary<string, string> GenerateReplacementDictionary(
            Options options,
            Build.ResourceDatabase resDb)
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

            return CommonUtil.Collections.DictionaryUtil.MergeDictionaries(
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
            IList<LibraryForExport> everyLibrary)
        {
            Dictionary<string, string> libraryProjectNameToGuid = new Dictionary<string, string>();

            TemplateReader templateReader = new TemplateReader(new PkgAwareFileUtil(), this);

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
                { "CURRENT_YEAR", CommonUtil.DateTime.Time.GetCurrentYear() + "" },
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
            foreach (LibraryForExport library in everyLibrary.Where(lib => lib.HasNativeCode))
            {
                string libBaseDir = "Libs/" + library.Name + "/";
                List<LangCSharp.DllFile> dlls = new List<LangCSharp.DllFile>();
                HashSet<string> dotNetRefs = new HashSet<string>();
                this.GetLibraryCode(templateReader, libBaseDir, library, dlls, dotNetRefs, output);

                string name = library.Name;
                string projectGuid = IdGenerator.GenerateCSharpGuid(library.Name + "|" + library.Version, "library-project");
                replacements["PROJECT_GUID"] = projectGuid;
                replacements["ASSEMBLY_GUID"] = IdGenerator.GenerateCSharpGuid(library.Name + "|" + library.Version, "library-assembly");
                replacements["PROJECT_TITLE"] = library.Name;
                replacements["LIBRARY_NAME"] = library.Name;
                replacements["DLL_REFERENCES"] = GetFrameworkReferencesCsProjCode(dotNetRefs);

                LangCSharp.DllReferenceHelper.AddDllReferencesToProjectBasedReplacements(replacements, dlls);

                libraryProjectNameToGuid[name] = projectGuid;

                this.CopyResourceAsText(output, libBaseDir + library.Name + ".sln", "ResourcesLib/Solution.sln", replacements);
                this.CopyResourceAsText(output, libBaseDir + library.Name + ".csproj", "ResourcesLib/ProjectFile.csproj", replacements);
                this.CopyResourceAsText(output, libBaseDir + "Properties/AssemblyInfo.cs", "ResourcesLib/AssemblyInfo.cs", replacements);

                foreach (LangCSharp.DllFile dll in dlls)
                {
                    output[libBaseDir + dll.HintPath] = dll.FileOutput;
                }
            }
            replacements["DLL_REFERENCES"] = dllReferencesOriginal;
            replacements["DLLS_COPIED"] = dllsCopiedOriginal;
            replacements["EMBEDDED_RESOURCES"] = embeddedResources;
            replacements["PROJECT_GUID"] = runtimeProjectGuid;
            replacements["ASSEMBLY_GUID"] = runtimeAssemblyGuid;

            this.CopyTemplatedFiles(baseDir, output, replacements, true);
            this.ExportInterpreter(templateReader, baseDir, output);
            this.ExportProjectFiles(baseDir, output, replacements, libraryProjectNameToGuid, true);
            this.CopyResourceAsBinary(output, baseDir + "icon.ico", "ResourcesVm/icon.ico");

            TODO.MoveCbxParserIntoTranslatedPastelCode();
            this.CopyResourceAsText(output, baseDir + "CbxDecoder.cs", "ResourcesVm/CbxDecoder.cs", replacements);
        }

        private void GetLibraryCode(
            TemplateReader templateReader,
            string baseDir,
            LibraryForExport library,
            List<LangCSharp.DllFile> dllsOut,
            HashSet<string> dotNetRefs,
            Dictionary<string, FileOutput> filesOut)
        {
            string libraryName = library.Name;
            TemplateSet libTemplates = templateReader.GetLibraryTemplates(library);
            List<string> libraryLines = new List<string>();

            string libraryDir = baseDir + "Libraries/" + libraryName;

            foreach (string structKey in libTemplates.GetPaths("gen/structs/"))
            {
                string structFileName = structKey.Substring(structKey.LastIndexOf('/') + 1);
                string structName = System.IO.Path.GetFileNameWithoutExtension(structFileName);
                filesOut[libraryDir + "/" + structName + ".cs"] = new FileOutput()
                {
                    Type = FileOutputType.Text,
                    TextContent = libTemplates.GetText(structKey),
                };
            }

            foreach (string helperFile in libTemplates.GetPaths("source/"))
            {
                filesOut[libraryDir + "/" + helperFile.Substring("source/".Length)] = new FileOutput()
                {
                    Type = FileOutputType.Text,
                    TextContent = libTemplates.GetText(helperFile),
                };
            }

            filesOut[libraryDir + "/LibraryWrapper.cs"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = libTemplates.GetText("gen/LibraryWrapper.cs"),
            };

            foreach (ExportEntity dllFile in library.ExportEntities["DOTNET_DLL"])
            {
                dllsOut.Add(new LangCSharp.DllFile(dllFile));
            }

            foreach (ExportEntity dotNetRef in library.ExportEntities["DOTNET_REF"])
            {
                dotNetRefs.Add(dotNetRef.StringValue);
            }
        }

        private static string GetFrameworkReferencesCsProjCode(ICollection<string> dotNetReferences)
        {
            return StringUtil.JoinLines(
                dotNetReferences
                    .OrderBy(v => v.ToLowerInvariant())
                    .Select(dotNetLib => "    <Reference Include=\"" + dotNetLib + "\" />")
                    .ToArray());
        }

        public override void ExportProject(
            Dictionary<string, FileOutput> output,
            string byteCode,
            IList<LibraryForExport> libraries,
            Build.ResourceDatabase resourceDatabase,
            Options options)
        {
            TemplateReader templateReader = new TemplateReader(new PkgAwareFileUtil(), this);

            Dictionary<string, string> replacements = this.GenerateReplacementDictionary(options, resourceDatabase);
            string projectId = options.GetString(ExportOptionKey.PROJECT_ID);
            string baseDir = projectId + "/";

            this.CopyTemplatedFiles(baseDir, output, replacements, false);

            List<LangCSharp.DllFile> dlls = new List<LangCSharp.DllFile>();

            HashSet<string> dotNetRefs = new HashSet<string>();

            foreach (LibraryForExport library in libraries.Where(lib => lib.HasNativeCode))
            {
                this.GetLibraryCode(templateReader, baseDir, library, dlls, dotNetRefs, output);
            }

            LangCSharp.DllReferenceHelper.AddDllReferencesToProjectBasedReplacements(replacements, dlls);

            replacements["DLL_REFERENCES"] += GetFrameworkReferencesCsProjCode(dotNetRefs);

            this.ExportInterpreter(templateReader, baseDir, output);

            output[baseDir + "Resources/ByteCode.txt"] = new FileOutput() { Type = FileOutputType.Text, TextContent = byteCode };
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
                string[] iconPaths = options.GetStringArray(ExportOptionKey.ICON_PATH);
                IconGenerator iconGen = new IconGenerator();
                foreach (string path in iconPaths)
                {
                    iconGen.AddImage(new Bitmap(path.Trim()));
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
            this.CopyResourceAsText(output, baseDir + "Vm/CoreFunctions.cs", "Resources/CoreFunctions.cs", replacements);
            this.CopyResourceAsText(output, baseDir + "Vm/TranslationHelper.cs", "Resources/TranslationHelper.cs", replacements);
            this.CopyResourceAsText(output, baseDir + "Vm/Library.cs", "Resources/Library.cs", replacements);
            this.CopyResourceAsText(output, baseDir + "Vm/LibraryRegistry.cs", resourceDir + "/LibraryRegistry.cs", replacements);
            this.CopyResourceAsText(output, baseDir + "Vm/UniversalBitmap.cs", "Resources/UniversalBitmap.cs", replacements);
            this.CopyResourceAsText(output, baseDir + "Vm/UniversalFont.cs", "Resources/UniversalFont.cs", replacements);

            // Required project files
            this.CopyResourceAsText(output, baseDir + "Properties/AssemblyInfo.cs", "Resources/AssemblyInfo.cs", replacements);
            this.CopyResourceAsText(output, baseDir + "Program.cs", resourceDir + "/Program.cs", replacements);

            // CSharpOpenTK specific stuff
            this.CopyResourceAsText(output, baseDir + "Vm/PlatformTranslationHelper.cs", "Resources/PlatformTranslationHelper.cs", replacements);
            this.CopyResourceAsText(output, baseDir + "ResourceReader.cs", resourceDir + "/ResourceReader.cs", replacements);

            string debuggerResource = isStandaloneVm ? "ResourcesVm/Debugger.cs" : "Resources/DummyDebugger.cs";
            this.CopyResourceAsText(output, baseDir + "Debugger.cs", debuggerResource, replacements);
        }

        private void ExportInterpreter(
            TemplateReader templateReader,
            string baseDir,
            Dictionary<string, FileOutput> output)
        {
            TemplateSet vmTemplates = templateReader.GetVmTemplates();

            foreach (string structKey in vmTemplates.GetPaths("structs/"))
            {
                string structFileName = structKey.Substring(structKey.LastIndexOf('/') + 1);
                string structName = System.IO.Path.GetFileNameWithoutExtension(structFileName);
                output[baseDir + "Structs/" + structName + ".cs"] = new FileOutput()
                {
                    Type = FileOutputType.Text,
                    TextContent = vmTemplates.GetText(structKey),
                };
            }

            output[baseDir + "Vm/CrayonWrapper.cs"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = vmTemplates.GetText("CrayonWrapper.cs"),
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
                string[] projects = libraryProjectNameToGuid.Keys.OrderBy(s => s.ToLowerInvariant()).ToArray();
                List<string> inclusions = new List<string>();
                List<string> configs = new List<string>();
                foreach (string projectName in projects)
                {
                    string guid = libraryProjectNameToGuid[projectName].ToUpperInvariant();
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

            this.CopyResourceAsText(output, projectId + ".sln", "Resources/SolutionFile.sln", replacements);
            this.CopyResourceAsText(output, projectId + "OSX.sln", "Resources/SolutionFileOsx.sln", replacements);
            string projectFileResource = (isStandaloneVm ? "ResourcesVm" : "Resources") + "/ProjectFile.csproj";
            this.CopyResourceAsText(output, baseDir + "Interpreter.csproj", projectFileResource, replacements);
        }
    }
}
