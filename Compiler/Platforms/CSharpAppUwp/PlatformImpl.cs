using Common;
using Pastel;
using Platform;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpAppUwp
{
    public class PlatformImpl : AbstractPlatform
    {
        public override string Name { get { return "csharp-app-uwp"; } }
        public override string InheritsFrom { get { return "lang-csharp"; } }
        public override string NL { get { return "\r\n"; } }

        public PlatformImpl()
            : base(Language.CSHARP)
        { }

        public override IDictionary<string, object> GetConstantFlags()
        {
            return new Dictionary<string, object>();
        }

        public override Dictionary<string, string> GenerateReplacementDictionary(Options options, ResourceDatabase resDb)
        {
            // TODO: this is mostly duplicated in csharp-app
            List<string> embeddedResources = new List<string>()
            {
                "<EmbeddedResource Include=\"Resources\\ByteCode.txt\"/>",
                "<EmbeddedResource Include=\"Resources\\ResourceManifest.txt\"/>",
            };

            if (resDb.ImageSheetManifestFile != null)
            {
                embeddedResources.Add("<EmbeddedResource Include=\"Resources\\ImageSheetManifest.txt\"/>");
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

        public override void ExportStandaloneVm(
            Dictionary<string, FileOutput> output,
            TemplateStorage templates,
            IList<LibraryForExport> everyLibrary)
        {
            throw new NotImplementedException();
        }

        public override void ExportProject(
            Dictionary<string, FileOutput> output,
            TemplateStorage templates,
            IList<LibraryForExport> libraries,
            ResourceDatabase resourceDatabase,
            Options options)
        {
            Dictionary<string, string> replacements = this.GenerateReplacementDictionary(options, resourceDatabase);

            string projectId = replacements["PROJECT_ID"];
            string baseDir = projectId + "/";

            this.CopyResourceAsText(output, projectId + ".sln", "Resources/SolutionFile.txt", replacements);
            this.CopyResourceAsText(output, baseDir + projectId + ".csproj", "Resources/ProjectFile.txt", replacements);

            this.CopyResourceAsText(output, baseDir + "App.xaml", "Resources/AppXaml.txt", replacements);
            this.CopyResourceAsText(output, baseDir + "App.xaml.cs", "Resources/AppXamlCs.txt", replacements);
            this.CopyResourceAsText(output, baseDir + "MainPage.xaml", "Resources/MainPageXaml.txt", replacements);
            this.CopyResourceAsText(output, baseDir + "MainPage.xaml.cs", "Resources/MainPageXamlCs.txt", replacements);
            this.CopyResourceAsText(output, baseDir + "Package.appxmanifest", "Resources/PackageAppxmanifest.txt", replacements);

            this.CopyResourceAsText(output, baseDir + "Properties/AssemblyInfo.cs", "Resources/AssemblyInfoCs.txt", replacements);
            this.CopyResourceAsText(output, baseDir + "Properties/Default.rd.xml", "Resources/DefaultRdXml.txt", replacements);

            this.CopyResourceAsText(output, baseDir + "Vm/TranslationHelper.cs", "Resources/TranslationHelper.txt", replacements);
            this.CopyResourceAsText(output, baseDir + "Vm/LibraryRegistry.cs", "Resources/LibraryRegistry.txt", replacements);
            this.CopyResourceAsText(output, baseDir + "Vm/Library.cs", "Resources/Library.txt", replacements);

            this.CopyResourceAsBinary(output, baseDir + "Assets/LockScreenLogo.scale-200.png", "Resources/Assets/lockscreenlogoscale200.png");
            this.CopyResourceAsBinary(output, baseDir + "Assets/SplashScreen.scale-200.png", "Resources/Assets/splashscreenscale200.png");
            this.CopyResourceAsBinary(output, baseDir + "Assets/Square150x150Logo.scale-200.png", "Resources/Assets/square150x150logoscale200.png");
            this.CopyResourceAsBinary(output, baseDir + "Assets/Square44x44Logo.scale-200.png", "Resources/Assets/square44x44logo.png");
            this.CopyResourceAsBinary(output, baseDir + "Assets/Square44x44Logo.targetsize-24_altform-unplated.png", "Resources/Assets/square44x44logotargetsize24altformunplated.png");
            this.CopyResourceAsBinary(output, baseDir + "Assets/StoreLogo.png", "Resources/Assets/storelogo.png");
            this.CopyResourceAsBinary(output, baseDir + "Assets/Wide310x150Logo.scale-200.png", "Resources/Assets/wide310x150logoscale200.png");

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

            CSharpApp.PlatformImpl.ExportInterpreter(this, templates, baseDir, output);
        }

        public override string WrapStructCode(string structCode)
        {
            return string.Join("\r\n", new string[] {
                "using System;",
                "using System.Collections.Generic;",
                "",
                "namespace Interpreter.Structs",
                "{",
                IndentCodeWithSpaces(structCode.Trim(), 4),
                "}",
                ""
            });
        }
    }
}
