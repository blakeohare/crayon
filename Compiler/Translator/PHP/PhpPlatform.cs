using System;
using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon.Translator.Php
{
	internal class PhpPlatform : AbstractPlatform
	{
		public PhpPlatform() : base(
			Crayon.PlatformId.PHP_SERVER,
			Crayon.LanguageId.PHP,
			false, new PhpTranslator(),
			new PhpSystemFunctionTranslator(),
			false)
		{ }

		public override bool ImagesLoadInstantly { get { return true; } }

		public override bool IsArraySameAsList { get { return true; } }

		public override bool IsAsync { get { return false; } }

		public override bool IsStronglyTyped { get { return false; } }

		public override string PlatformShortId { get { return "php-server"; } }

		public override bool SupportsListClear { get { return false; } }

		public override Dictionary<string, FileOutput> Package(
			BuildContext buildContext,
			string projectId,
			Dictionary<string, Executable[]> finalCode,
			ICollection<StructDefinition> structDefinitions,
			string fileCopySourceRoot,
			ResourceDatabase resourceDatabase)
		{
			throw new NotImplementedException();
		}
	}
}

