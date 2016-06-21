using System;
using System.Collections.Generic;

namespace Crayon.Translator.CSharp
{
	internal class CSharpXamarinIosPlatform : CSharpPlatform
	{
		public override string GeneratedFilesFolder { get { return "%PROJECT_ID%/Resources/GeneratedFiles"; } }

		public CSharpXamarinIosPlatform() : base(
			new CSharpXamarinIosSystemFunctionTranslator(),
			new CSharpXamarinIosOpenGlTranslator())
		{ }

		public override string PlatformShortId { get { return "csharp-ios"; } }

		public override void ApplyPlatformSpecificOverrides(string projectId, Dictionary<string, FileOutput> files)
		{
			base.ApplyPlatformSpecificOverrides(projectId, files);

			// Hack
			foreach (string key in files.Keys)
			{
				if (key.StartsWith(projectId + "/GeneratedFiles/"))
				{
					FileOutput file = files[key];
					string newKey = projectId + "/Resources" + key.Substring(projectId.Length);
					files.Remove(key);
					files.Add(newKey, file);
				}
			}
		}

		public override void AddPlatformSpecificSystemLibraries(HashSet<string> systemLibraries)
		{
			// Nope
		}

		public override void ApplyPlatformSpecificReplacements(Dictionary<string, string> replacements)
		{
			// Nope
		}

		private List<string> audioResourcePathsRelativeToProjectRoot = new List<string>();

		// TODO: refactor this up. This is copied and pasted from the Android code.
		protected override List<string> FilterEmbeddedResources(List<string> embeddedResources)
		{
			List<string> filteredEmbeddedResources = new List<string>();
			foreach (string resource in embeddedResources)
			{
				if (resource.ToLower().EndsWith(".ogg"))
				{
					this.audioResourcePathsRelativeToProjectRoot.Add(resource.Substring("Files/".Length));
				}
				else
				{
					filteredEmbeddedResources.Add(resource);
				}
			}
			return filteredEmbeddedResources;
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
