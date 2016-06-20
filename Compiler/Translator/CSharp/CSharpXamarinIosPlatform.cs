using System;
using System.Collections.Generic;

namespace Crayon.Translator.CSharp
{
	internal class CSharpXamarinIosPlatform : CSharpPlatform
	{
		public CSharpXamarinIosPlatform() : base(
			new CSharpXamarinIosSystemFunctionTranslator(),
			new CSharpXamarinIosOpenGlTranslator())
		{ }

		public override string PlatformShortId { get { return "csharp-ios"; } }

		public override void AddPlatformSpecificSystemLibraries(HashSet<string> systemLibraries)
		{
			// Nope
		}

		public override void ApplyPlatformSpecificReplacements(Dictionary<string, string> replacements)
		{
			// Nope
		}

		public override void PlatformSpecificFiles(string projectId, List<string> compileTargets, Dictionary<string, FileOutput> files, Dictionary<string, string> replacements, SpriteSheetBuilder spriteSheet)
		{
			files[projectId + ".sln"] = new FileOutput()
			{
				Type = FileOutputType.Text,
				TextContent = Constants.DoReplacements(
					Util.ReadResourceFileInternally("csharp-ios/SolutionFile.sln.txt"),
					replacements),
			};

			foreach (string csFile in new string[] {
				"AppDelegate",
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
						Util.ReadResourceFileInternally("csharp-ios/" + csFile + ".txt"),
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
						Util.ReadResourceFileInternally("csharp-ios/" + parts[0]),
						replacements),
				};
			}

			files[projectId + "/Resources/ByteCode.txt"] = new FileOutput()
			{
				Type = FileOutputType.Text,
				TextContent = this.Context.ByteCodeString
			};

			files[projectId + "/" + projectId + ".csproj"] = new FileOutput()
			{
				Type = FileOutputType.Text,
				TextContent = Constants.DoReplacements(
					Util.ReadResourceFileInternally("csharp-ios/ProjectFile.csproj.txt"),
					replacements),
			};
		}
	}
}
