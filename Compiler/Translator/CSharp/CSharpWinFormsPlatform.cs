using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon.Translator.CSharp
{
    class CSharpWinFormsPlatform : CSharpPlatform
    {
        public CSharpWinFormsPlatform() : base(new CSharpWinFormsSystemFunctionTranslator(), false)
        {

        }

        public override string PlatformShortId { get { return "ui-csharp-winforms"; } }

        public override void PlatformSpecificFiles(
            string projectId,
            Dictionary<string, FileOutput> files,
            Dictionary<string, string> replacements,
            ResourceDatabase resourceDatabase)
        {
            List<string> embeddedResources = new List<string>();

            files[projectId + "/Resources/ByteCode.txt"] = resourceDatabase.ByteCodeFile;
            embeddedResources.Add("Resources\\ByteCode.txt");

            files[projectId + "/Resources/ResourceMapping.txt"] = resourceDatabase.ResourceManifestFile;
            embeddedResources.Add("Resources\\ResourceMapping.txt");

            foreach (FileOutput fileOutput in resourceDatabase.TextResources)
            {
                files[projectId + "/Resources/Text/" + fileOutput.CanonicalFileName] = fileOutput;
                embeddedResources.Add("Resources\\Text\\" + fileOutput.CanonicalFileName);
            }

            foreach (FileOutput fileOutput in resourceDatabase.ImageResources)
            {
                if (fileOutput.Type != FileOutputType.Ghost)
                {
                    files[projectId + "/Resources/Images/" + fileOutput.CanonicalFileName] = fileOutput;
                    embeddedResources.Add("Resources\\Images\\" + fileOutput.CanonicalFileName);
                }
            }

            foreach (FileOutput fileOutput in resourceDatabase.AudioResources)
            {
                files[projectId + "/Resources/Audio/" + fileOutput.CanonicalFileName] = fileOutput;
                embeddedResources.Add("Resources\\Audio\\" + fileOutput.CanonicalFileName);
            }

            if (resourceDatabase.SpriteSheetManifestFile != null)
            {
                files[projectId + "/Resources/SpriteSheetManifest.txt"] = resourceDatabase.SpriteSheetManifestFile;
                embeddedResources.Add("Resources\\SpriteSheetManifest.txt");

                foreach (string spriteSheetFile in resourceDatabase.SpriteSheetFiles.Keys)
                {
                    FileOutput fileOutput = resourceDatabase.SpriteSheetFiles[spriteSheetFile];
                    files[projectId + "/Resources/ImageSheets/" + spriteSheetFile] = fileOutput;
                    embeddedResources.Add("Resources\\ImageSheets\\" + spriteSheetFile);
                }
            }

            List<string> embeddedResourceReplacement = new List<string>();
            foreach (string embeddedResource in embeddedResources)
            {
                embeddedResourceReplacement.Add("    <EmbeddedResource Include=\"" + embeddedResource + "\" />");
            }
            replacements["EMBEDDED_RESOURCES"] = string.Join("\r\n", embeddedResourceReplacement);
            
            foreach (string file in new string[]
            {
                "SolutionFile.txt,%%%PROJECT_ID%%%.sln",
                "ProjectFile.txt,%%%PROJECT_ID%%%/%%%PROJECT_ID%%%.csproj",

                "AppConfig.txt,%%%PROJECT_ID%%%/App.config",
                "Form1.txt,%%%PROJECT_ID%%%/Form1.cs",
                "Form1Designer.txt,%%%PROJECT_ID%%%/Form1.Designer.cs",
                "Program.txt,%%%PROJECT_ID%%%/Program.cs",
                "AssemblyInfo.txt,%%%PROJECT_ID%%%/Properties/AssemblyInfo.cs",
                "ResourcesDesigner.txt,%%%PROJECT_ID%%%/Properties/Resources.Designer.cs",
                "ResourcesResx.txt,%%%PROJECT_ID%%%/Properties/Resources.resx",
                "SettingsDesigner.txt,%%%PROJECT_ID%%%/Properties/Settings.Designer.cs",
                "SettingsSettings.txt,%%%PROJECT_ID%%%/Properties/Settings.settings",
                "WinFormsTranslationHelper.txt,%%%PROJECT_ID%%%/WinFormsTranslationHelper.cs",
            })
            {
                string[] parts = file.Split(',');
                string source = "ui-csharp-winforms/" + parts[0];
                string destination = Constants.DoReplacements(parts[1], replacements);
                string content = Constants.DoReplacements(Util.ReadResourceFileInternally(source), replacements);
                files[destination] = new FileOutput()
                {
                    Type = FileOutputType.Text,
                    TextContent = content,
                };
            }

            files[Constants.DoReplacements("%%%PROJECT_ID%%%/ResourceReader.cs", replacements)] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = Constants.DoReplacements(Util.ReadResourceFileInternally("csharp-common/ResourceReader.txt"), replacements),
            };
        }
    }
}
