using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon.Translator.CSharp
{
	class CSharpXamarinAndroidPlatform : CSharpPlatform
    {
        public override string GeneratedFilesFolder { get { return "%PROJECT_ID%/Assets/GeneratedFiles"; } }

		public CSharpXamarinAndroidPlatform() : base(
			new CSharpXamarinAndroidSystemFunctionTranslator(),
			new CSharpXamarinAndroidOpenGlTranslator())
		{ }

		public override string PlatformShortId { get { return "csharp-android"; } }
        
        private List<string> audioResourcePathsRelativeToProjectRoot = new List<string>();
        
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
                    Util.ReadResourceFileInternally("csharp-android/SolutionFile.sln.txt"),
                    replacements)
            };
            
            List<string> additionalAndroidAssets = new List<string>();
            /*
            foreach (string spriteSheetImage in spriteSheet.FinalPaths)
            {
                // TODO: need a better system of putting things in predefined destinations, rather than hacking it between states
                // in this fashion.
                string path = spriteSheetImage.Substring("%PROJECT_ID%".Length + 1).Replace('/', '\\');
                additionalAndroidAssets.Add("    <AndroidAsset Include=\"" + path + "\" />\r\n");
            }//*/
            throw new System.NotImplementedException(); // ^

            int androidResourceId = 0x7f040000;
            List<string> androidResourcesForProjectFile = new List<string>();
            List<string> androidResourceLookupFile = new List<string>();
            foreach (string audioResourcePath in this.audioResourcePathsRelativeToProjectRoot)
            {
                string num = androidResourceId.ToString();
                string extension = null;
                if (audioResourcePath.Contains('.'))
                {
                    string[] parts = audioResourcePath.Split('.');
                    extension = parts[parts.Length - 1].ToLower();
                }

                while (num.Length < 6)
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

                androidResourcesForProjectFile.Add("  <AndroidResource Include=\"Resources\\raw\\" + resFilename + "\" />");

                androidResourceId++;
            }

            if(androidResourceLookupFile.Count > 0)
            {
                files[projectId + "/Assets/resourceLookup.txt"] = new FileOutput()
                {
                    Type = FileOutputType.Text,
                    TextContent = string.Join("\n", androidResourceLookupFile)
                };
                additionalAndroidAssets.Add("    <AndroidAsset Include=\"Assets\\resourceLookup.txt\" />\r\n");
            }

            replacements["ADDITIONAL_ANDROID_ASSETS"] = string.Join("\n", additionalAndroidAssets);
            replacements["ANDROID_RAW_RESOURCES"] = string.Join("\n", androidResourcesForProjectFile);

            files[projectId + "/" + projectId + ".csproj"] = new FileOutput()
			{
				Type = FileOutputType.Text,
				TextContent = Constants.DoReplacements(
					Util.ReadResourceFileInternally("csharp-android/ProjectFile.csproj.txt"),
					replacements)
			};
            
            files[projectId + "/Resources/drawable/Icon.png"] = new FileOutput()
			{
				Type = FileOutputType.Binary,
				BinaryContent = Util.ReadResourceBytesInternally("csharp-android/Icon.png"),
			};

			// TODO: if not really used, can this be removed from the project?
			files[projectId + "/Resources/layout/Main.axml"] = new FileOutput()
			{
				Type = FileOutputType.Text,
				TextContent = Constants.DoReplacements(
					Util.ReadResourceFileInternally("csharp-android/Main.axml.txt"),
					replacements),
			};

			files[projectId + "/Resources/values/strings.xml"] = new FileOutput()
			{
				Type = FileOutputType.Text,
				TextContent = Constants.DoReplacements(
					Util.ReadResourceFileInternally("csharp-android/Strings.xml.txt"),
					replacements),
			};

            files[projectId + "/Resources/Resource.Designer.cs"] = new FileOutput()
            {
                Type = FileOutputType.Text,
                TextContent = Constants.DoReplacements(
                    Util.ReadResourceFileInternally("csharp-android/ResourceDesigner.txt"),
                    replacements),
            };

            files[projectId + "/Assets/ByteCode.txt"] = resourceDatabase.ByteCodeFile;

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
                    TextContent = Constants.DoReplacements(
                        Util.ReadResourceFileInternally("csharp-android/" + filename + ".txt"),
                        replacements)
                };
            }
		}
	}
}
