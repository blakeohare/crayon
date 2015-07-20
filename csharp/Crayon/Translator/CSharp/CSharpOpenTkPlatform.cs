using System.Collections.Generic;

namespace Crayon.Translator.CSharp
{
	class CSharpOpenTkPlatform : CSharpPlatform
	{
		public CSharpOpenTkPlatform()
			: base(new CSharpOpenTkSystemFunctionTranslator())
		{ }

		public override bool IsOpenGlBased { get { return true; } }
		public override bool SupportsGamePad { get { return true; } }

		public override void ApplyPlatformSpecificReplacements(Dictionary<string, string> replacements)
		{
			replacements["EXTRA_DLLS"] = string.Join("\r\n", new string[] {
				"    <Reference Include=\"OpenTK, Version=1.0.0.0, Culture=neutral, PublicKeyToken=bad199fe84eb3df4, processorArchitecture=MSIL\">",
				"      <SpecificVersion>False</SpecificVersion>",
				"      <HintPath>.\\OpenTK.dll</HintPath>",
				"    </Reference>",
				"    <Reference Include=\"SdlDotNet, Version=6.1.0.0, Culture=neutral, PublicKeyToken=26ad4f7e10c61408, processorArchitecture=MSIL\">",
				"      <SpecificVersion>False</SpecificVersion>",
				"      <HintPath>.\\SdlDotNet.dll</HintPath>",
				"    </Reference>",
				"    <Reference Include=\"Tao.Sdl, Version=1.2.13.0, Culture=neutral, PublicKeyToken=9c7a200e36c0094e, processorArchitecture=MSIL\">",
				"      <SpecificVersion>False</SpecificVersion>",
				"      <HintPath>.\\Tao.Sdl.dll</HintPath>",
				"    </Reference>"
			});

			replacements["COPY_FILES"] = string.Join("\r\n", new string[] {
				"    <None Include=\"DependencyLicenses.txt\">",
				"      <CopyToOutputDirectory>Always</CopyToOutputDirectory>",
				"    </None>",
			});
		}

		public override void AddPlatformSpecificSystemLibraries(HashSet<string> systemLibraries) { }

		public override void PlatformSpecificFiles(
			string projectId,
			List<string> compileTargets,
			Dictionary<string, FileOutput> files,
			Dictionary<string, string> replacements)
		{
			compileTargets.Add("GameWindow.cs");
			files[projectId + "/GameWindow.cs"] = new FileOutput()
			{
				Type = FileOutputType.Text,
				TextContent = Constants.DoReplacements(
					Util.ReadFileInternally("Translator/CSharp/Project/GameWindowOpenTK.txt"),
					replacements)
			};

			compileTargets.Add("GlUtil.cs");
			files[projectId + "/GlUtil.cs"] = new FileOutput()
			{
				Type = FileOutputType.Text,
				TextContent = Constants.DoReplacements(
					Util.ReadFileInternally("Translator/CSharp/Project/GlUtil.txt"),
					replacements)
			};

			// TODO: Do conditional check to see if any sound is used anywhere. If not, exclude the SdlDotNet/Tao.Sdl binaries.
			foreach (string binary in new string[] { "OpenTK", "SdlDotNet", "Tao.Sdl" })
			{
				files[projectId + "/" + binary + ".dll"] = new FileOutput()
				{
					Type = FileOutputType.Binary,
					BinaryContent = Util.ReadBytesInternally("Translator/CSharp/Binaries/" + binary + ".dll")
				};
			}

			files[projectId + "/DependencyLicenses.txt"] = new FileOutput()
			{
				Type = FileOutputType.Text,
				TextContent = Util.ReadFileInternally("Translator/CSharp/Project/License.txt")
			};
		}
	}
}
