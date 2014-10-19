using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crayon.ParseTree;

namespace Crayon.Translator.CSharp
{
	class CSharpTranslator : CurlyBraceImplementation
	{
		public CSharpTranslator()
			: base(false)
		{ }

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
			output.Add("public static object ");
			output.Add("v_" + functionDef.NameToken.Value);
			output.Add("(");
			for (int i = 0; i < functionDef.ArgNames.Length; ++i)
			{
				if (i > 0) output.Add(", ");
				if (functionDef.Annotations[i] == null)
				{
					output.Add("object ");
				}
				else
				{
					string argType = functionDef.Annotations[i].GetSingleArgAsString(null);
					string type = this.CSharpPlatform.GetTypeStringFromAnnotation(functionDef.Annotations[i].FirstToken, argType);
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

			this.Translate(output, functionDef.Code);
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
				output.Add("static ");
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
			output.Add(assignment.AssignmentOp);
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

		protected override void TranslateDictionaryDefinition(List<string> output, DictionaryDefinition dictDef)
		{
			output.Add("new Dictionary<object, object>()");
			int keyCount = dictDef.Keys.Length;
			if (keyCount > 0)
			{
				output.Add(" { ");
				for (int i = 0; i < keyCount; ++i)
				{
					if (i > 0)
					{
						output.Add(", ");
					}
					output.Add("{ ");
					this.TranslateExpression(output, dictDef.Keys[i]);
					output.Add(", ");
					this.TranslateExpression(output, dictDef.Values[i]);
					output.Add(" }");
				}
				output.Add(" }");
			}
		}

		protected override void TranslateListDefinition(List<string> output, ListDefinition listDef)
		{
			output.Add("new List<object>() {");
			for (int i = 0; i < listDef.Items.Length; ++i)
			{
				if (i > 0) output.Add(",");
				output.Add(" ");
				this.TranslateExpression(output, listDef.Items[i]);
			}
			output.Add(" }");
		}
	}
}
