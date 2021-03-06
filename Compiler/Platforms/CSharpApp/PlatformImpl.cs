﻿using Common;
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

            if (resDb.ImageResourceManifestFile != null)
            {
                embeddedResources.Add("<EmbeddedResource Include=\"Resources\\ImageManifest.txt\"/>");
            }

            if (options.GetBool(ExportOptionKey.HAS_ICON))
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
                this.ParentPlatform.GenerateReplacementDictionary(options, resDb),
                new Dictionary<string, string>() {
                    { "PROJECT_GUID", IdGenerator.GenerateCSharpGuid(options.GetStringOrNull(ExportOptionKey.GUID_SEED) ?? guidSeed, "project") },
                    { "ASSEMBLY_GUID", IdGenerator.GenerateCSharpGuid(options.GetStringOrNull(ExportOptionKey.GUID_SEED) ?? guidSeed, "assembly") },
                    { "EMBEDDED_RESOURCES", string.Join("\r\n", embeddedResources).Trim() },
                    { "CSHARP_APP_ICON", options.GetBool(ExportOptionKey.HAS_ICON) ? "<ApplicationIcon>icon.ico</ApplicationIcon>" : "" },
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
                { "CURRENT_YEAR", CommonUtil.DateTime.Time.GetCurrentYear() + "" },
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
            string byteCode,
            Build.ResourceDatabase resourceDatabase,
            Options options)
        {
            TemplateReader templateReader = new TemplateReader(new PkgAwareFileUtil(), this);
            bool usesU3 = options.GetBool(ExportOptionKey.USES_U3);

            Dictionary<string, string> replacements = this.GenerateReplacementDictionary(options, resourceDatabase);
            string projectId = options.GetString(ExportOptionKey.PROJECT_ID);
            string baseDir = projectId + "/";

            this.CopyTemplatedFiles(baseDir, output, replacements, false);

            this.ExportInterpreter(templateReader, baseDir, output);

            output[baseDir + "Resources/ByteCode.txt"] = new FileOutput() { Type = FileOutputType.Text, TextContent = byteCode };
            output[baseDir + "Resources/ResourceManifest.txt"] = resourceDatabase.ResourceManifestFile;

            if (resourceDatabase.ImageResourceManifestFile != null)
            {
                output[baseDir + "Resources/ImageManifest.txt"] = resourceDatabase.ImageResourceManifestFile;
            }

            foreach (FileOutput imageFile in resourceDatabase.ImageResources.Where(img => img.CanonicalFileName != null))
            {
                output[baseDir + "Resources/" + imageFile.CanonicalFileName] = imageFile;
            }

            foreach (string imageFilePath in resourceDatabase.ImageResourceFiles.Keys)
            {
                output[baseDir + "Resources/" + imageFilePath] = resourceDatabase.ImageResourceFiles[imageFilePath];
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

            if (options.GetBool(ExportOptionKey.HAS_ICON))
            {
                this.GenerateIconFile(output, baseDir + "icon.ico", options);
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
