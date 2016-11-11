using System.Collections.Generic;

namespace Crayon.Translator.Ruby
{
	internal class RubyPlatform : AbstractPlatform
	{
		public RubyPlatform()
			: base(PlatformId.RUBY_GOSU, LanguageId.RUBY, new RubyTranslator(), new RubySystemFunctionTranslator())
		{ }

		public override bool IsAsync { get { return false; } }
		public override bool RemoveBreaksFromSwitch { get { return true; } }
		public override bool SupportsListClear { get { return false; } }
		public override bool IsStronglyTyped { get { return false; } }
		public override bool IsArraySameAsList { get { return true; } }
		public override bool IsCharANumber { get { return false; } }
		public override bool IntIsFloor { get { return false; } }
		public override bool IsThreadBlockingAllowed { get { return true; } }

        public override string PlatformShortId { get { return "game-ruby-gosu"; } }
	
		public override Dictionary<string, FileOutput> Package(
			BuildContext buildContext, 
			string projectId, 
			Dictionary<string, ParseTree.Executable[]> finalCode, 
			ICollection<ParseTree.StructDefinition> structDefinitions, 
			ResourceDatabase resourceDatabase, 
			SystemLibraryManager libraryManager)
		{
			Dictionary<string, FileOutput> files = new Dictionary<string, FileOutput>();
			List<string> concatenatedCode = new List<string>();

            this.Translator.TranslateGlobals(concatenatedCode, finalCode);
			concatenatedCode.Add(this.Translator.NL);
			//this.Translator.TranslateSwitchLookups(concatenatedCode, finalCode);
			concatenatedCode.Add(this.Translator.NL);
			this.Translator.TranslateFunctions(concatenatedCode, finalCode);
			concatenatedCode.Add(this.Translator.NL);

			files["run.rb"] = new FileOutput()
			{
				Type = FileOutputType.Text,
				TextContent = string.Join("", concatenatedCode),
			};

			return files;
		}
	}
}
