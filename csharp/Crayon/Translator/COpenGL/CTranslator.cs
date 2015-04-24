using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crayon.ParseTree;

namespace Crayon.Translator.COpenGL
{
	class CTranslator : CurlyBraceImplementation
	{
		public CTranslator()
			: base(false)
		{ }

		public COpenGLPlatform CPlatform { get { return (COpenGLPlatform)this.Platform; } }

		protected override void TranslateAssignment(List<string> output, ParseTree.Assignment assignment)
		{
			output.Add(this.CurrentTabIndention);
			Expression target = assignment.Target;
			Annotation typeAnnotation = target.GetAnnotation("type");

			if (typeAnnotation != null)
			{
				string type = this.CPlatform.GetTypeStringFromAnnotation(
					typeAnnotation.FirstToken,
					typeAnnotation.GetSingleArgAsString(null),
					false, false);
				output.Add(type);
				output.Add(" ");
			}
		}

		protected override void TranslateFunctionDefinition(List<string> output, ParseTree.FunctionDefinition functionDef)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDotStepStruct(List<string> output, ParseTree.DotStepStruct dotStepStruct)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStructInstance(List<string> output, ParseTree.StructInstance structInstance)
		{
			throw new NotImplementedException();
		}
	}
}
