using Common;
using CommonUtil.Random;
using Platform;
using System.Collections.Generic;
using System.Linq;
using Wax;

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
            ExportProperties exportProperties,
            BuildData buildData)
        {
            ResourceDatabase resDb = buildData.CbxBundle.ResourceDB;

            List<string> embeddedResources = new List<string>()
            {
                "<EmbeddedResource Include=\"Resources\\ByteCode.txt\"/>",
                "<EmbeddedResource Include=\"Resources\\ResourceManifest.txt\"/>",
            };

            if (resDb.ImageResourceManifestFile != null)
            {
                embeddedResources.Add("<EmbeddedResource Include=\"Resources\\ImageManifest.txt\"/>");
            }

            if (exportProperties.HasIcon)
            {
                embeddedResources.Add("<EmbeddedResource Include=\"icon.ico\" />");
            }

            foreach (FileOutput imageFile in resDb.ImageResources.Where(img => img.CanonicalFileName != null))
            {
                embeddedResources.Add("<EmbeddedResource Include=\"Resources\\" + imageFile.CanonicalFileName + "\"/>");
            }

            foreach (string imageChunk in resDb.ImageResourceFiles.Keys)
            {
                embeddedResources.Add("<EmbeddedResource Include=\"Resources\\" + imageChunk + "\"/>");
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
                this.ParentPlatform.GenerateReplacementDictionary(exportProperties, buildData),
                new Dictionary<string, string>() {
                    { "PROJECT_GUID", IdGenerator.GenerateCSharpGuid(exportProperties.GuidSeed ?? guidSeed, "project") },
                    { "ASSEMBLY_GUID", IdGenerator.GenerateCSharpGuid(exportProperties.GuidSeed ?? guidSeed, "assembly") },
                    { "EMBEDDED_RESOURCES", string.Join("\r\n", embeddedResources).Trim() },
                    { "CSHARP_APP_ICON", exportProperties.HasIcon ? "<ApplicationIcon>icon.ico</ApplicationIcon>" : "" },
                });
        }

        private Dictionary<string, string> HACK_libraryProjectGuidToPath = new Dictionary<string, string>();

        public override void ExportStandaloneVm(Dictionary<string, FileOutput> output)
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
                { "CURRENT_YEAR", System.DateTime.Now.Year + "" },
                { "CSHARP_APP_ICON", "<ApplicationIcon>icon.ico</ApplicationIcon>" },
                { "EMBEDDED_RESOURCES", "<EmbeddedResource Include=\"icon.ico\" />" },
                { "CSHARP_CONTENT_ICON", "" },
            };
            string baseDir = "CrayonRuntime/";

            string embeddedResources = replacements["EMBEDDED_RESOURCES"];
            replacements["EMBEDDED_RESOURCES"] = "";

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

        public override void ExportProject(
            Dictionary<string, FileOutput> output,
            BuildData buildData,
            ExportProperties exportProperties)
        {
            TemplateReader templateReader = new TemplateReader(new PkgAwareFileUtil(), this);
            bool usesU3 = buildData.UsesU3;

            Dictionary<string, string> replacements = this.GenerateReplacementDictionary(exportProperties, buildData);
            string projectId = exportProperties.ProjectID;
            string baseDir = projectId + "/";

            this.CopyTemplatedFiles(baseDir, output, replacements, false);

            this.ExportInterpreter(templateReader, baseDir, output);

            ResourceDatabase resDb = buildData.CbxBundle.ResourceDB;
            output[baseDir + "Resources/ByteCode.txt"] = new FileOutput() { Type = FileOutputType.Text, TextContent = buildData.CbxBundle.ByteCode };
            output[baseDir + "Resources/ResourceManifest.txt"] = buildData.CbxBundle.ResourceDB.ResourceManifestFile;

            if (resDb.ImageResourceManifestFile != null)
            {
                output[baseDir + "Resources/ImageManifest.txt"] = resDb.ImageResourceManifestFile;
            }

            foreach (FileOutput imageFile in resDb.ImageResources.Where(img => img.CanonicalFileName != null))
            {
                output[baseDir + "Resources/" + imageFile.CanonicalFileName] = imageFile;
            }

            foreach (string imageFilePath in resDb.ImageResourceFiles.Keys)
            {
                output[baseDir + "Resources/" + imageFilePath] = resDb.ImageResourceFiles[imageFilePath];
            }

            foreach (FileOutput textFile in resDb.TextResources.Where(img => img.CanonicalFileName != null))
            {
                output[baseDir + "Resources/" + textFile.CanonicalFileName] = textFile;
            }

            foreach (FileOutput audioFile in resDb.AudioResources.Where(file => file.CanonicalFileName != null))
            {
                output[baseDir + "Resources/" + audioFile.CanonicalFileName] = audioFile;
            }

            foreach (FileOutput fontFile in resDb.FontResources.Where(file => file.CanonicalFileName != null))
            {
                output[baseDir + "Resources/" + fontFile.CanonicalFileName] = fontFile;
            }

            if (exportProperties.HasIcon)
            {
                this.GenerateIconFile(output, baseDir + "icon.ico", exportProperties);
            }

            this.ExportProjectFiles(baseDir, output, replacements, new Dictionary<string, string>(), false);
        }

        private void CopyTemplatedFiles(string baseDir, Dictionary<string, FileOutput> output, Dictionary<string, string> replacements, bool isStandaloneVm)
        {
            string resourceDir = isStandaloneVm ? "ResourcesVm" : "Resources";

            // From LangCSharp
            this.CopyResourceAsText(output, baseDir + "Vm/CoreFunctions.cs", "Resources/CoreFunctions.cs", replacements);
            this.CopyResourceAsText(output, baseDir + "Vm/DateTimeHelper.cs", "Resources/DateTimeHelper.cs", replacements);
            this.CopyResourceAsText(output, baseDir + "Vm/DiskHelper.cs", "Resources/DiskHelper.cs", replacements);
            this.CopyResourceAsText(output, baseDir + "Vm/EventLoop.cs", "Resources/EventLoop.cs", replacements);
            this.CopyResourceAsText(output, baseDir + "Vm/ImageUtil.cs", "Resources/ImageUtil.cs", replacements);
            this.CopyResourceAsText(output, baseDir + "Vm/JsonHelper.cs", "Resources/JsonHelper.cs", replacements);
            this.CopyResourceAsText(output, baseDir + "Vm/PixelBuffer.cs", "Resources/PixelBuffer.cs", replacements);
            this.CopyResourceAsText(output, baseDir + "Vm/ProcessHelper.cs", "Resources/ProcessHelper.cs", replacements);
            this.CopyResourceAsText(output, baseDir + "Vm/TextEncodingHelper.cs", "Resources/TextEncodingHelper.cs", replacements);
            this.CopyResourceAsText(output, baseDir + "Vm/TranslationHelper.cs", "Resources/TranslationHelper.cs", replacements);
            this.CopyResourceAsText(output, baseDir + "Vm/UniversalBitmap.cs", "Resources/UniversalBitmap.cs", replacements);

            // Required project files
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

            this.CopyResourceAsText(output, projectId + ".sln", "Resources/SolutionFile.sln", replacements);
            string projectFileResource = "Resources/ProjectFile.csproj";
            this.CopyResourceAsText(output, baseDir + "Interpreter.csproj", projectFileResource, replacements);
        }
    }
}
