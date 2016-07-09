using System.Collections.Generic;

namespace Crayon.Translator.CSharp
{
    class CSharpOpenTkPlatform : CSharpPlatform
    {
        public CSharpOpenTkPlatform()
            : base(new CSharpOpenTkSystemFunctionTranslator(), true)
        { }

        public override string PlatformShortId { get { return "game-csharp-opentk"; } }

        public override void PlatformSpecificFiles(
            string projectId,
            Dictionary<string, FileOutput> files,
            Dictionary<string, string> replacements,
            ResourceDatabase resourceDatabase)
        {
            // TODO: Do conditional check to see if any sound is used anywhere. If not, exclude the SdlDotNet/Tao.Sdl binaries.
            foreach (string binary in new string[] { "OpenTK", "SDL", "SDL_mixer", "SdlDotNet", "Tao.Sdl" })
            {
                files[projectId + "/" + binary + ".dll"] = new FileOutput()
                {
                    Type = FileOutputType.Binary,
                    BinaryContent = Util.ReadResourceBytesInternally("game-csharp-opentk/binaries/" + binary + ".dll")
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
                "SolutionFile.sln.txt,%%%PROJECT_ID%%%.sln",
                "ProjectFile.csproj.txt,%%%PROJECT_ID%%%/%%%PROJECT_ID%%%.csproj",

                "AssemblyInfo.txt,%%%PROJECT_ID%%%/Properties/AssemblyInfo.cs",
                "GamepadTranslationHelper.txt,%%%PROJECT_ID%%%/GamepadTranslationHelper.cs",
                "GameWindow.txt,%%%PROJECT_ID%%%/GameWindow.cs",
                "GlUtil.txt,%%%PROJECT_ID%%%/GlUtil.cs",
                "OpenTkRenderer.txt,%%%PROJECT_ID%%%/OpenTkRenderer.cs",
                "OpenTkTranslationHelper.txt,%%%PROJECT_ID%%%/OpenTkTranslationHelper.cs",
                "Program.txt,%%%PROJECT_ID%%%/Program.cs",
                "ResourceReader.txt,%%%PROJECT_ID%%%/ResourceReader.cs",
            })
            {
                string[] parts = file.Split(',');
                string source = "game-csharp-opentk/" + parts[0];
                string destination = Constants.DoReplacements(parts[1], replacements);
                string content = Constants.DoReplacements(Util.ReadResourceFileInternally(source), replacements);
                files[destination] = new FileOutput()
                {
                    Type = FileOutputType.Text,
                    TextContent = content,
                };
            }
        }
    }
}
