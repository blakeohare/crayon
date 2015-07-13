using System.Collections.Generic;

namespace Crayon.Translator.Python
{
	class PythonPlatform : AbstractPlatform
	{
		public PythonPlatform()
			: base(false, new PythonTranslator(), new PythonSystemFunctionTranslator())
		{ }

		public override bool IsAsync { get { return false; } }
		public override bool RemoveBreaksFromSwitch { get { return true; } }
		public override bool SupportsListClear { get { return false; } }
		public override bool IsStronglyTyped { get { return false; } }
		public override bool UseFixedListArgConstruction { get { return false; } }
		public override bool IntIsFloor { get { return false; } }
		public override bool ImagesLoadInstantly { get { return true; } }
		public override bool ScreenBlocksExecution { get { return false; } }
		public override bool IsOpenGlBased { get { return false; } }
		public override bool SupportsGamePad { get { return true; } }

		public override string OutputFolderName { get { return "pygame"; } }
		public override string GeneratedFilesFolder { get { return "_generated_files"; } }

		public override Dictionary<string, FileOutput> Package(
			BuildContext buildContext,
			string projectId,
			Dictionary<string, ParseTree.Executable[]> finalCode,
			List<string> filesToCopyOver,
			ICollection<ParseTree.StructDefinition> structDefinitions,
			string inputFolder,
			SpriteSheetBuilder spriteSheet)
		{
			Dictionary<string, FileOutput> output = new Dictionary<string, FileOutput>();
			List<string> concatenatedCode = new List<string>();
			Dictionary<string, string> replacements = new Dictionary<string, string>()
			{
				{ "PROJECT_ID", projectId },
			};

			concatenatedCode.Add(this.GetPyGameCode("Header.py", replacements));
			concatenatedCode.Add(this.Translator.NL);
			concatenatedCode.Add(this.GetPyGameCode("AsyncHttpFetcher.py", replacements));
			concatenatedCode.Add(this.Translator.NL);
			this.Translator.TranslateGlobals(concatenatedCode, finalCode);
			concatenatedCode.Add(this.Translator.NL);
			this.Translator.TranslateSwitchLookups(concatenatedCode, finalCode);
			concatenatedCode.Add(this.Translator.NL);
			this.Translator.TranslateFunctions(concatenatedCode, finalCode);
			concatenatedCode.Add(this.Translator.NL);
			concatenatedCode.Add(this.GetPyGameCode("Footer.py", replacements));
			concatenatedCode.Add(this.Translator.NL);

			output["game.py"] = new FileOutput()
			{
				Type = FileOutputType.Text,
				TextContent = string.Join("", concatenatedCode)
			};

			foreach (string file in filesToCopyOver)
			{
				output[file] = new FileOutput()
				{
					Type = FileOutputType.Copy,
					RelativeInputPath = file,
				};
			}

			return output;
		}

		private string GetPyGameCode(string file, Dictionary<string, string> replacements)
		{
			string pygameCode = Util.ReadFileInternally("Translator/Python/PyGame/" + file);
			pygameCode = Constants.DoReplacements(pygameCode, replacements);
			return pygameCode;
		}
	}
}
