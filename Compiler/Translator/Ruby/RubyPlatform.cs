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
	
		public override System.Collections.Generic.Dictionary<string, FileOutput> Package(BuildContext buildContext, string projectId, System.Collections.Generic.Dictionary<string, ParseTree.Executable[]> finalCode, System.Collections.Generic.ICollection<ParseTree.StructDefinition> structDefinitions, ResourceDatabase resourceDatabase, SystemLibraryManager libraryManager)
		{
			throw new System.NotImplementedException();
		}
	}
}
