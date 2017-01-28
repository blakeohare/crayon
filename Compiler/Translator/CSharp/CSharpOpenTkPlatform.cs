using System.Collections.Generic;

namespace Crayon.Translator.CSharp
{
    class CSharpOpenTkPlatform : CSharpPlatform
    {
        public CSharpOpenTkPlatform()
            : base(new CSharpOpenTkSystemFunctionTranslator())
        { }

        public override string PlatformShortId { get { return "game-csharp-opentk"; } }

        public override void PlatformSpecificFiles(
            string projectId,
            Dictionary<string, FileOutput> files,
            Dictionary<string, string> replacements,
            ResourceDatabase resourceDatabase, 
            string iconFilePath,
            BuildContext buildContext)
        {
            bool hasIcon = iconFilePath != null;
            if (hasIcon)
            {
                byte[] iconFile = Util.GetIconFileBytesFromImageFile(iconFilePath);
                files[projectId + "/icon.ico"] = new FileOutput()
                {
                    Type = FileOutputType.Binary,
                    BinaryContent = iconFile,
                };
            }

            foreach (string binaryNameInfo in new string[] {
                "OpenTK",
                "SDL",
                "SDL_mixer",
                "SdlDotNet",
                "Tao.Sdl",
                "libogg:-0",
                "libvorbis:-0",
                "libvorbisfile:-3" })
            {
                string[] parts = binaryNameInfo.Split(':');
                string binary = parts[0];
                string resourcePath = binary;
                if (parts.Length == 2)
                {
                    binary += parts[1];
                }
                files[projectId + "/" + binary + ".dll"] = new FileOutput()
                {
                    Type = FileOutputType.Binary,
                    BinaryContent = Util.ReadResourceBytesInternally("game-csharp-opentk/binaries/" + resourcePath + ".dll")
                };
            }

            files[projectId + "/DependencyLicenses.txt"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = Util.ReadResourceFileInternally("game-csharp-opentk/License.txt")
            };

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
                    files[projectId + "/Resources/Images/" + imageSheetFile] = fileOutput;
                    embeddedResources.Add("Resources\\Images\\" + imageSheetFile);
                }
            }

            List<string> embeddedResourceReplacement = new List<string>();
            foreach (string embeddedResource in embeddedResources)
            {
                embeddedResourceReplacement.Add("    <EmbeddedResource Include=\"" + embeddedResource + "\" />");
            }
            replacements["EMBEDDED_RESOURCES"] = string.Join("\r\n", embeddedResourceReplacement);
            replacements["CSHARP_APP_ICON"] = hasIcon
                ? "<ApplicationIcon>icon.ico</ApplicationIcon>"
                : "";
            replacements["CSHARP_CONTENT_ICON"] = hasIcon
                ? "<EmbeddedResource Include=\"icon.ico\" />"
                : "";

            foreach (string file in new string[]
            {
                "SolutionFile.sln.txt,%%%PROJECT_ID%%%.sln",
                "ProjectFile.csproj.txt,%%%PROJECT_ID%%%/%%%PROJECT_ID%%%.csproj",

                "AssemblyInfo.txt,%%%PROJECT_ID%%%/Properties/AssemblyInfo.cs",
                "GamepadTranslationHelper.txt,%%%PROJECT_ID%%%/GamepadTranslationHelper.cs",
                "GameWindow.txt,%%%PROJECT_ID%%%/GameWindow.cs",
                "GlUtil.txt,%%%PROJECT_ID%%%/GlUtil.cs",
                "OpenTkRenderer.txt,%%%PROJECT_ID%%%/OpenTkRenderer.cs",
                "OpenTkTranslationHelper.txt,%%%PROJECT_ID%%%/OpenTkTranslationHelper.cs",
                "Program.txt,%%%PROJECT_ID%%%/Program.cs",
            })
            {
                string[] parts = file.Split(',');
                string source = "game-csharp-opentk/" + parts[0];
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
