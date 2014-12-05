using System.Collections.Generic;

namespace Crayon.Translator.CSharp
{
	class CSharpOpenTkPlatform : CSharpPlatform
	{
		public CSharpOpenTkPlatform()
			: base(new CSharpOpenTkSystemFunctionTranslator())
		{ }

		public override string OutputFolderName { get { return "csopengl"; } }

		public override void ApplyPlatformSpecificReplacements(Dictionary<string, string> replacements)
		{
			replacements["EXTRA_DLLS"] = string.Join("\r\n", new string[] {
				"<Reference Include=\"OpenTK, Version=1.0.0.0, Culture=neutral, PublicKeyToken=bad199fe84eb3df4, processorArchitecture=MSIL\">",
				"      <SpecificVersion>False</SpecificVersion>",
				"      <HintPath>.\\OpenTK.dll</HintPath>",
				"    </Reference>",
				"    "
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
				TextContent = Util.MassReplacements(
					Util.ReadFileInternally("Translator/CSharp/Project/GameWindowOpenTK.txt"),
					replacements)
			};

			compileTargets.Add("Image.cs");
			files[projectId + "/Image.cs"] = new FileOutput()
			{
				Type = FileOutputType.Text,
				TextContent = Util.MassReplacements(
					Util.ReadFileInternally("Translator/CSharp/Project/ImageOpenTK.txt"),
					replacements)
			};

			files[projectId + "/OpenTK.dll"] = new FileOutput()
			{
				Type = FileOutputType.Binary,
				BinaryContent = Util.ReadBytesInternally("Translator/CSharp/OpenTK/OpenTK.dll")
			};
		}
	}
}
