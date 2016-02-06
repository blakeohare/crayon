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

			this.TranslateExpression(output, target);
			output.Add(" ");
			output.Add(assignment.AssignmentOp);
			output.Add(" ");
			this.TranslateExpression(output, assignment.Value);
			output.Add(";");
			output.Add(this.NL);
		}

		protected override void TranslateFunctionDefinition(List<string> output, ParseTree.FunctionDefinition functionDef)
		{
			output.Add(this.CurrentTabIndention);

			string returnType = "void*";
			Annotation returnTypeAnnotation = functionDef.GetAnnotation("type");
			if (returnTypeAnnotation != null)
			{
				returnType = this.CPlatform.GetTypeStringFromAnnotation(new AnnotatedType(returnTypeAnnotation), false, true);
			}
			output.Add(returnType);
			output.Add(" ");
			output.Add("v_" + functionDef.NameToken.Value);
			output.Add("(");
			for (int i = 0; i < functionDef.ArgNames.Length; ++i)
			{
				if (i > 0) output.Add(", ");
				if (functionDef.ArgAnnotations[i] == null)
				{
					output.Add("object ");
				}
				else
				{
					string argType = functionDef.ArgAnnotations[i].GetSingleArgAsString(null);
					string type = this.CPlatform.GetTypeStringFromAnnotation(functionDef.ArgAnnotations[i].FirstToken, argType, false, true);
					output.Add(type);
					output.Add(" ");
				}
				output.Add("v_" + functionDef.ArgNames[i].Value);
			}
			output.Add(")");
			output.Add(this.NL);
			output.Add(this.CurrentTabIndention);
			output.Add("{");
			output.Add(this.NL);
			this.CurrentIndention++;

			Executable[] code = functionDef.Code;
			if (functionDef.GetAnnotation("omitReturn") != null)
			{
				Executable[] newCode = new Executable[code.Length - 1];
				Array.Copy(code, newCode, newCode.Length);
				code = newCode;
			}
			this.Translate(output, code);

			this.CurrentIndention--;
			output.Add(this.CurrentTabIndention);
			output.Add("}");
			//*/
			output.Add(this.NL);
		}

		protected override void TranslateDotStepStruct(List<string> output, ParseTree.DotStepStruct dotStepStruct)
		{
			output.Add(dotStepStruct.RootVar);
			output.Add("->");
			output.Add(dotStepStruct.FieldName);
		}

		protected override void TranslateStructInstance(List<string> output, ParseTree.StructInstance structInstance)
		{
			string name = structInstance.NameToken.Value;
			output.Add("((");
			output.Add(name);
			output.Add("*) malloc(sizeof(");
			output.Add(name);
			output.Add(")))");
		}

		protected override void TranslateBooleanConstant(List<string> output, BooleanConstant booleanConstant)
		{
			output.Add(booleanConstant.Value ? "TRUE" : "FALSE");
		}

		protected override void TranslateNullConstant(List<string> output, NullConstant nullConstant)
		{
			output.Add("NULL");
		}
	}
}
