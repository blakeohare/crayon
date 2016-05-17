using System.Collections.Generic;

namespace Crayon.Translator.CSharp
{
	class CSharpOpenTkPlatform : CSharpPlatform
	{
		public CSharpOpenTkPlatform()
			: base(new CSharpOpenTkSystemFunctionTranslator(), new CSharpOpenTkOpenGlTranslator())
		{ }

		public override string PlatformShortId { get { return "csotk"; } }

		public override void ApplyPlatformSpecificReplacements(Dictionary<string, string> replacements)
		{
			replacements["PROJECT_FILE_EXTRAS"] = string.Join("\n", 
				"<OutputType>WinExe</OutputType>",
				"    <TargetFrameworkProfile>Client</TargetFrameworkProfile>",
				"    <TargetFrameworkVersion>v4.0</TargetFrameworkVersion>");


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
                // TODO: This is the wrong place for this.
                // But the Project file is going to be target-specific soon anyway, so it doesn't matter too much.
                "    <EmbeddedResource Include=\"ByteCode.txt\" />",

                "    <None Include=\"DependencyLicenses.txt\">",
				"      <CopyToOutputDirectory>Always</CopyToOutputDirectory>",
				"    </None>",
			});
		}

		public override void AddPlatformSpecificSystemLibraries(HashSet<string> systemLibraries) { }

        public override void ApplyPlatformSpecificOverrides(string projectId, Dictionary<string, FileOutput> files)
        {
            FileOutput openGlRenderer = files[projectId + "/OpenGlPipeline.cs"];
            openGlRenderer.TextContent = openGlRenderer.TextContent.Replace(
                // This is quite silly.
                // Ultimately the OpenGL stuff will be pulled out as a static file resource and this will be a non-issue.
                "using System.Linq;",
                "using System.Linq;\r\nusing OpenTK.Graphics.OpenGL;");
        }

		public override void PlatformSpecificFiles(
			string projectId,
			List<string> compileTargets,
			Dictionary<string, FileOutput> files,
			Dictionary<string, string> replacements)
		{
			// TODO: Do conditional check to see if any sound is used anywhere. If not, exclude the SdlDotNet/Tao.Sdl binaries.
			foreach (string binary in new string[] { "OpenTK", "SDL", "SDL_mixer", "SdlDotNet", "Tao.Sdl" })
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

            foreach (string filename in new string[] {
                "GameWindow",
                "GlUtil",
                "Program",
                "AssemblyInfo",
                "ResourceReader",
                "OpenTkTranslationHelper",

                // TODO: only inject this when the Gamepad library is imported
                // It'll require some reworking with the library code.
                "GamepadTranslationHelper",
            }) {
                compileTargets.Add(filename + ".cs");
                string target = projectId + "/" + (filename == "AssemblyInfo" ? "Properties/" : "") + filename + ".cs";
                files[target] = new FileOutput()
                {
                    Type = FileOutputType.Text,
                    TextContent = Constants.DoReplacements(
                        Util.ReadFileInternally("Translator/CSharp/Project/OpenTK/" + filename + ".txt"),
                        replacements)
                };
            }
		}
	}
}
