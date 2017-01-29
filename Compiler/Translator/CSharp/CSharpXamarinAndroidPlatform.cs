using System.Collections.Generic;
using Common;

namespace Crayon.Translator.CSharp
{
    class CSharpXamarinAndroidPlatform : CSharpPlatform
    {
        public CSharpXamarinAndroidPlatform() : base(
            new CSharpXamarinAndroidSystemFunctionTranslator())
        { }

        public override string PlatformShortId { get { return "game-csharp-android"; } }

        private List<string> audioResourcePathsRelativeToProjectRoot = new List<string>();

        public override void PlatformSpecificFiles(
            string projectId,
            Dictionary<string, FileOutput> files,
            Dictionary<string, string> replacements,
            ResourceDatabase resourceDatabase,
            string iconFilePath,
            BuildContext buildContext)
        {
            files[projectId + ".sln"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = Constants.DoReplacements(false,
                    LegacyUtil.ReadResourceFileInternally("game-csharp-android/SolutionFile.sln.txt"),
                    replacements)
            };

            List<string> additionalAndroidAssets = new List<string>();

            if (resourceDatabase.ImageSheetManifestFile != null)
            {
                foreach (string tileName in resourceDatabase.ImageSheetFiles.Keys)
                {
                    additionalAndroidAssets.Add("    <AndroidAsset Include=\"Assets\\ImageSheets\\" + tileName + "\" />");
                    files[projectId + "/Assets/ImageSheets/" + tileName] = resourceDatabase.ImageSheetFiles[tileName];
                }

                files[projectId + "/Assets/Text/imageSheetManifest.txt"] = resourceDatabase.ImageSheetManifestFile;
                additionalAndroidAssets.Add("    <AndroidAsset Include=\"Assets\\Text\\imageSheetManifest.txt\" />");
            }

            additionalAndroidAssets.Add("    <AndroidAsset Include=\"Assets\\Text\\resourceManifest.txt\" />");
            files[projectId + "/Assets/Text/resourceManifest.txt"] = resourceDatabase.ResourceManifestFile;

            /*
                There's another layer of file resource indirection here since Android makes a distinction between
                "Assets" and "Resources" and resources are assigned a magic ID# that starts at 0x7f040000 and increments
                through the files in alphabetical order.
            */
            int androidResourceId = 0x7f040000;
            List<string> androidResourcesForProjectFile = new List<string>();
            List<string> androidResourceLookupFile = new List<string>();
            foreach (string audioResourcePath in this.audioResourcePathsRelativeToProjectRoot)
            {
                string num = androidResourceId.ToString();
                string extension = FileUtil.GetCanonicalExtension(audioResourcePath);

                while (num.Length < 11)
                {
                    num = "0" + num;
                }

                string resFilename = "res" + num + "." + extension;

                files[projectId + "/Resources/raw/" + resFilename] = new FileOutput()
                {
                    Type = FileOutputType.Copy,
                    RelativeInputPath = audioResourcePath,
                };

                androidResourceLookupFile.Add(androidResourceId + "," + audioResourcePath.Replace('\\', '/'));

                androidResourcesForProjectFile.Add("    <AndroidResource Include=\"Resources\\raw\\" + resFilename + "\" />");

                androidResourceId++;
            }

            files[projectId + "/Assets/Text/resourceLookup.txt"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = string.Join("\n", androidResourceLookupFile)
            };
            additionalAndroidAssets.Add("    <AndroidAsset Include=\"Assets\\Text\\resourceLookup.txt\" />");
            
            foreach (FileOutput textFile in resourceDatabase.TextResources)
            {
                files[projectId + "/Assets/Text/" + textFile.CanonicalFileName] = textFile;
                additionalAndroidAssets.Add("    <AndroidAsset Include=\"Assets\\Text\\" + textFile.CanonicalFileName + "\" />");
            }
            
            replacements["ADDITIONAL_ANDROID_ASSETS"] = string.Join("\r\n", additionalAndroidAssets);
            replacements["ANDROID_RAW_RESOURCES"] = "\r\n" + string.Join("\r\n", androidResourcesForProjectFile);
            string orientationString;
            switch (MobileOrientationUtil.Parse(buildContext.Orientation)) {
                case MobileOrientation.AUTO: orientationString = "FullSensor"; break;
                case MobileOrientation.LANDSCAPE: orientationString = "Landscape"; break;
                case MobileOrientation.LANDSCAPES: orientationString = "SensorLandscape"; break;
                case MobileOrientation.PORTRAIT: orientationString = "Portrait"; break;
                case MobileOrientation.PORTRAITS: orientationString = "SensorPortrait"; break;
                default: throw new System.Exception(); // Need to define value here.
            }
            replacements["XAMARIN_ANDROID_ORIENTATION"] = orientationString;

            files[projectId + "/Resources/drawable/Icon.png"] = new FileOutput()
            {
                Type = FileOutputType.Binary,
                BinaryContent = LegacyUtil.ReadResourceBytesInternally("game-csharp-android/Icon.png"),
            };

            files[projectId + "/" + projectId + ".csproj"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = Constants.DoReplacements(false,
                    LegacyUtil.ReadResourceFileInternally("game-csharp-android/ProjectFile.csproj.txt"),
                    replacements)
            };

            // TODO: if not really used, can this be removed from the project?
            files[projectId + "/Resources/layout/Main.axml"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = Constants.DoReplacements(false,
                    LegacyUtil.ReadResourceFileInternally("game-csharp-android/Main.axml.txt"),
                    replacements),
            };

            files[projectId + "/Resources/values/strings.xml"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = Constants.DoReplacements(false,
                    LegacyUtil.ReadResourceFileInternally("game-csharp-android/Strings.xml.txt"),
                    replacements),
            };

            files[projectId + "/Resources/Resource.Designer.cs"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = Constants.DoReplacements(false,
                    LegacyUtil.ReadResourceFileInternally("game-csharp-android/ResourceDesigner.txt"),
                    replacements),
            };

            files[projectId + "/Assets/Text/ByteCode.txt"] = resourceDatabase.ByteCodeFile;

            foreach (string filename in new string[] {
                "AssemblyInfo",
                "CsxaAudioHelper",
                "CsxaGlRenderer",
                "CsxaTranslationHelper",
                "GlView1",
                "MainActivity",
                "ResourceReader",
            })
            {
                string target = projectId + "/" + (filename == "AssemblyInfo" ? "Properties/" : "") + filename + ".cs";
                files[target] = new FileOutput()
                {
                    Type = FileOutputType.Text,
                    TextContent = Constants.DoReplacements(false,
                        LegacyUtil.ReadResourceFileInternally("game-csharp-android/" + filename + ".txt"),
                        replacements)
                };
            }
        }
    }
}
