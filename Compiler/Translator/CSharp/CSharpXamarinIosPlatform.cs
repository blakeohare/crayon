using System.Collections.Generic;

namespace Crayon.Translator.CSharp
{
    internal class CSharpXamarinIosPlatform : CSharpPlatform
    {
        public CSharpXamarinIosPlatform()
            : base(new CSharpXamarinIosSystemFunctionTranslator(), true)
        { }

        public override string PlatformShortId { get { return "game-csharp-ios"; } }

        public override void PlatformSpecificFiles(
            string projectId,
            Dictionary<string, FileOutput> files,
            Dictionary<string, string> replacements,
            ResourceDatabase resourceDatabase)
        {
            files[projectId + ".sln"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = Constants.DoReplacements(
                    Util.ReadResourceFileInternally("game-csharp-ios/SolutionFile.sln.txt"),
                    replacements),
            };

            foreach (string csFile in new string[] {
                "AppDelegate",
				"CryImage",
                "CsxiTranslationHelper",
                "GameViewController",
                "Graphics2dRenderer",
                "Main",
                "ResourceReader",
            })
            {
                files[projectId + "/" + csFile + ".cs"] = new FileOutput()
                {
                    Type = FileOutputType.Text,
                    TextContent = Constants.DoReplacements(
                        Util.ReadResourceFileInternally("game-csharp-ios/" + csFile + ".txt"),
                        replacements),
                };
            }

            foreach (string copyFile in new string[] {
                "AssetsXcassetsAppIconsAppIconSetContentsJson.txt|Assets.xcassets/AppIcons.appiconset/Contents.json",
                "AssetsXcassetsContentsJson.txt|Assets.xcassets/Contents.json",
                "EntitlementsPlist.txt|Entitlements.plist",
                "InfoPlist.txt|Info.plist",
                "LaunchScreenStoryboard.txt|LaunchScreen.storyboard",
                "MainStoryboard.txt|Main.storyboard",
            })
            {
                string[] parts = copyFile.Split('|');
                files[projectId + "/" + parts[1]] = new FileOutput()
                {
                    Type = FileOutputType.Text,
                    TextContent = Constants.DoReplacements(
                        Util.ReadResourceFileInternally("game-csharp-ios/" + parts[0]),
                        replacements),
                };
            }

            files[projectId + "/Resources/ByteCode.txt"] = resourceDatabase.ByteCodeFile;
            files[projectId + "/Resources/ResourceManifest.txt"] = resourceDatabase.ResourceManifestFile;

            List<string> bundleResources = new List<string>();
            foreach (FileOutput textFile in resourceDatabase.TextResources)
            {
                files[projectId + "/Resources/Text/" + textFile.CanonicalFileName] = textFile;
                bundleResources.Add("    <BundleResource Include=\"Resources\\Text\\" + textFile.CanonicalFileName + "\" />\r\n");
            }

            if (resourceDatabase.ImageSheetManifestFile != null)
            {
                files[projectId + "/Resources/ImageSheetManifest.txt"] = resourceDatabase.ImageSheetManifestFile;
                bundleResources.Add("    <BundleResource Include=\"Resources\\ImageSheetManifest.txt\" />\r\n");

                foreach (string tileName in resourceDatabase.ImageSheetFiles.Keys)
                {
                    FileOutput tile = resourceDatabase.ImageSheetFiles[tileName];
                    files[projectId + "/Resources/Images/" + tileName] = tile;
                    bundleResources.Add("    <BundleResource Include=\"Resources\\Images\\" + tileName + "\" />\r\n");
                }
            }

            replacements["IOS_BUNDLE_RESOURCES"] = string.Join("", bundleResources);

            files[projectId + "/" + projectId + ".csproj"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = Constants.DoReplacements(
                    Util.ReadResourceFileInternally("game-csharp-ios/ProjectFile.csproj.txt"),
                    replacements),
            };
        }
    }
}
