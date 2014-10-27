using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon.Translator.Java
{
	internal class JavaPlatform : AbstractPlatform
	{
		public override bool IsAsync { get { return true; } }
		public override string OutputFolderName { get { return "java"; } }
		public override bool SupportsListClear { get { return true; } }
		public override bool IsStronglyTyped { get { return true; } }
		public override bool UseFixedListArgConstruction { get { return true; } }

		public JavaPlatform()
			: base(false, new JavaTranslator(), new JavaSystemFunctionTranslator())
		{ }

		public override Dictionary<string, FileOutput> Package(string projectId, Dictionary<string, ParseTree.Executable[]> finalCode, List<string> filesToCopyOver, ICollection<ParseTree.StructDefinition> structDefinitions)
		{
			throw new NotImplementedException();
		}
	}
}
