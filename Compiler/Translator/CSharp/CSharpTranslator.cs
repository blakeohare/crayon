using System;
using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon.Translator.CSharp
{
	class CSharpTranslator : CurlyBraceImplementation
	{
		public CSharpTranslator()
			: base(false)
		{ }

        public override string NL { get { return "\r\n"; } }

        public CSharpPlatform CSharpPlatform { get { return (CSharpPlatform)this.Platform; } }

		protected override void TranslateDotStepStruct(List<string> output, DotStepStruct dotStepStruct)
		{
			output.Add(dotStepStruct.RootVar);
			output.Add(".");
			output.Add(dotStepStruct.FieldName);
		}

		protected override void TranslateFunctionDefinition(List<string> output, FunctionDefinition functionDef)
		{
			output.Add(this.CurrentTabIndention);
			output.Add("public static ");

			string returnType = "object";
			Annotation returnTypeAnnotation = functionDef.GetAnnotation("type");
			if (returnTypeAnnotation != null)
			{
				returnType = this.CSharpPlatform.GetTypeStringFromAnnotation(returnTypeAnnotation);
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
					string type = this.CSharpPlatform.GetTypeStringFromAnnotation(functionDef.ArgAnnotations[i].FirstToken, argType);
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
			output.Add(this.NL);
		}

		protected override void TranslateAssignment(List<string> output, Assignment assignment)
		{
			output.Add(this.CurrentTabIndention);
			Expression target = assignment.Target;

			if (target is Variable && ((Variable)target).IsStatic)
			{
				output.Add("public static ");
			}

			Annotation typeAnnotation = target.GetAnnotation("type");

			if (typeAnnotation != null)
			{
				CSharpPlatform csharpPlatform = (CSharpPlatform)this.Platform;
				string csharpType = csharpPlatform.GetTypeStringFromAnnotation(typeAnnotation.FirstToken, typeAnnotation.GetSingleArgAsString(null));
				output.Add(csharpType);
				output.Add(" ");
			}

			this.TranslateExpression(output, target);
			output.Add(" ");
            output.Add(this.GetAssignmentOp(assignment));
            output.Add(" ");
			this.TranslateExpression(output, assignment.Value);
			output.Add(";");
			output.Add(this.NL);
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
