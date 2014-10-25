using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon.Translator.Python
{
	// TODO: subclass this to PyGamePlatform when other Python based platforms are made.

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
		public override string OutputFolderName { get { return "pygame"; } }

		public override Dictionary<string, FileOutput> Package(string projectId, Dictionary<string, ParseTree.Executable[]> finalCode, List<string> filesToCopyOver, ICollection<ParseTree.StructDefinition> structDefinitions)
		{
			Dictionary<string, FileOutput> output = new Dictionary<string, FileOutput>();
			List<string> concatenatedCode = new List<string>();

			concatenatedCode.Add(this.GetPyGameCode("Header.py"));
			concatenatedCode.Add(this.Translator.NL);
			this.Translator.TranslateGlobals(concatenatedCode, finalCode);
			concatenatedCode.Add(this.Translator.NL);
			this.Translator.TranslateSwitchLookups(concatenatedCode, finalCode);
			concatenatedCode.Add(this.Translator.NL);
			this.Translator.TranslateFunctions(concatenatedCode, finalCode);
			concatenatedCode.Add(this.Translator.NL);

			concatenatedCode.Add(this.GetPyGameCode("Footer.py"));
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

		private string GetPyGameCode(string file)
		{
			string pygameCode = Util.ReadFileInternally("Translator/Python/PyGame/" + file);
			pygameCode = Constants.DoReplacements(pygameCode);
			return pygameCode;
		}
	}
}
