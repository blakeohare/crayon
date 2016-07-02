using System;
using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon.Translator.Php
{
	internal class PhpTranslator : CurlyBraceImplementation
	{
		public PhpTranslator() : base(true) { }

		protected override void TranslateAssignment(List<string> output, Assignment assignment)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDotStepStruct(List<string> output, DotStepStruct dotStepStruct)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateFunctionDefinition(List<string> output, FunctionDefinition functionDef)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStructInstance(List<string> output, StructInstance structInstance)
		{
			throw new NotImplementedException();
		}
	}
}

