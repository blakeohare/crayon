using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon.Translator.CSharp
{
	class CSharpWinFormsPlatform : CSharpPlatform
	{
		public CSharpWinFormsPlatform()
			: base(new CSharpWinFormsSystemFunctionTranslator())
		{ }

		public override string OutputFolderName { get { return "cswinforms"; } }

		public override bool IsOpenGlBased { get { return false; } }
		public override bool SupportsGamePad { get { return false; } }

		public override void ApplyPlatformSpecificReplacements(Dictionary<string, string> replacements)
		{

		}

		public override void AddPlatformSpecificSystemLibraries(HashSet<string> systemLibraries)
		{
			systemLibraries.Add("WindowsBase");
			systemLibraries.Add("System.Windows.Forms");
		}

		public override void PlatformSpecificFiles(string projectId, List<string> compileTargets, Dictionary<string, FileOutput> files, Dictionary<string, string> replacements)
		{
			compileTargets.Add("GameWindow.cs");
			files[projectId + "/GameWindow.cs"] = new FileOutput()
			{
				Type = FileOutputType.Text,
				TextContent = Util.MassReplacements(
					Util.ReadFileInternally("Translator/CSharp/Project/GameWindowWinForms.txt"),
					replacements)
			};

			compileTargets.Add("Renderer.cs");
			files[projectId + "/Renderer.cs"] = new FileOutput()
			{
				Type = FileOutputType.Text,
				TextContent = Util.MassReplacements(
					Util.ReadFileInternally("Translator/CSharp/Project/RendererWinForms.txt"),
					replacements)
			};

			compileTargets.Add("Image.cs");
			files[projectId + "/Image.cs"] = new FileOutput()
			{
				Type = FileOutputType.Text,
				TextContent = Util.MassReplacements(
					Util.ReadFileInternally("Translator/CSharp/Project/ImageWinForms.txt"),
					replacements)
			};
		}
	}
}
