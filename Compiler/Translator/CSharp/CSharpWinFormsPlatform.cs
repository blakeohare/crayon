using System.Collections.Generic;
using Common;

namespace Crayon.Translator.CSharp
{
    class CSharpWinFormsPlatform : CSharpPlatform
    {
        public CSharpWinFormsPlatform() : base(new CSharpWinFormsSystemFunctionTranslator())
        {

        }

        public override string PlatformShortId { get { return "ui-csharp-winforms"; } }

        public override void PlatformSpecificFiles(
            string projectId,
            Dictionary<string, FileOutput> files,
            Dictionary<string, string> replacements,
            ResourceDatabase resourceDatabase,
            string iconFilePath,
            BuildContext buildContext)
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

            if (resourceDatabase.ImageSheetManifestFile != null)
            {
                files[projectId + "/Resources/ImageSheetManifest.txt"] = resourceDatabase.ImageSheetManifestFile;
                embeddedResources.Add("Resources\\ImageSheetManifest.txt");

                foreach (string imageSheetFile in resourceDatabase.ImageSheetFiles.Keys)
                {
                    FileOutput fileOutput = resourceDatabase.ImageSheetFiles[imageSheetFile];
                    files[projectId + "/Resources/ImageSheets/" + imageSheetFile] = fileOutput;
                    embeddedResources.Add("Resources\\ImageSheets\\" + imageSheetFile);
                }
            }

            files[projectId + "/DefaultIcon.png"] = new FileOutput()
            {
                Type = FileOutputType.Binary,
                BinaryContent = Util.ReadResourceBytesInternally("ui-csharp-winforms/DefaultIcon.png"),
            };

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
                "NoriRenderer.txt,%%%PROJECT_ID%%%/NoriRenderer.cs",
            })
            {
                string[] parts = file.Split(',');
                string source = "ui-csharp-winforms/" + parts[0];
                string destination = Constants.DoReplacements(false, parts[1], replacements);
                string content = Constants.DoReplacements(false, Util.ReadResourceFileInternally(source), replacements);
                files[destination] = new FileOutput()
                {
                    Type = FileOutputType.Text,
                    TextContent = content,
                };
            }

            files[Constants.DoReplacements(false, "%%%PROJECT_ID%%%/ResourceReader.cs", replacements)] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = Constants.DoReplacements(false, Util.ReadResourceFileInternally("csharp-common/ResourceReader.txt"), replacements),
            };
        }
    }
}
