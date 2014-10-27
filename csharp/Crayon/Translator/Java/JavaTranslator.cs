using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crayon.ParseTree;

namespace Crayon.Translator.Java
{
	class JavaTranslator : CurlyBraceImplementation
	{
		public JavaTranslator() : base(true) { }

		protected override void TranslateAssignment(List<string> output, ParseTree.Assignment assignment)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateFunctionDefinition(List<string> output, ParseTree.FunctionDefinition functionDef)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDotStepStruct(List<string> output, DotStepStruct dotStepStruct)
		{
			output.Add(dotStepStruct.RootVar);
			output.Add(".");
			output.Add(dotStepStruct.FieldName);
		}

		protected override void TranslateStructInstance(List<string> output, ParseTree.StructInstance structInstance)
		{
			output.Add("new ");
			output.Add(structInstance.NameToken.Value);
			output.Add("(");
			for (int i = 0; i < structInstance.Args.Length; ++i)
			{
				if (i > 0) output.Add(", ");
				this.TranslateExpression(output, structInstance.Args[i]);
			}
			output.Add(")");
		}
	}
}
