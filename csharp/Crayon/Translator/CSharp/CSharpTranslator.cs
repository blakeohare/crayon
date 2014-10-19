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

		protected override void TranslateFunctionDefinition(List<string> output, FunctionDefinition functionDef)
		{
			output.Add(this.CurrentTabIndention);
			output.Add("public static object ");
			output.Add(functionDef.NameToken.Value);
			output.Add("(");
			for (int i = 0; i < functionDef.ArgNames.Length; ++i)
			{
				if (i > 0) output.Add(", ");
				output.Add("object ");
				output.Add("v_" + functionDef.ArgNames[i].Value);
			}
			output.Add(")");
			output.Add(this.NL);
			output.Add(this.CurrentTabIndention);
			output.Add("{");
			output.Add(this.NL);
			this.CurrentIndention++;

			foreach (string varName in functionDef.GetVariableDeclarationList())
			{
				output.Add(this.CurrentTabIndention);
				output.Add("object v_" + varName + this.Shorten(" = null;") + this.NL);
			}

			this.Translate(output, functionDef.Code);
			this.CurrentIndention--;
			output.Add(this.CurrentTabIndention);
			output.Add("}");
			output.Add(this.NL);
		}

		protected override void TranslateAssignment(List<string> output, Assignment assignment)
		{
			output.Add(this.CurrentTabIndention);
			this.TranslateExpression(output, assignment.Target);
			output.Add(" ");
			output.Add(assignment.AssignmentOp);
			output.Add(" ");
			this.TranslateExpression(output, assignment.Value);
			output.Add(";");
			output.Add(this.NL);
		}

		protected override void TranslateStructDefinition(List<string> output, StructDefinition structDefinition)
		{
			output.Add(this.CurrentTabIndention);
			output.Add("internal class ");
			output.Add(structDefinition.Name.Value);
			output.Add(this.NL);
			this.CurrentIndention++;
			for (int i = 0; i < structDefinition.Fields.Length; ++i)
			{
				Token fieldToken = structDefinition.Fields[0];
				string field = fieldToken.Value;
				output.Add(this.CurrentTabIndention);
				output.Add("public object ");
				output.Add(field);
				output.Add(";");
				output.Add(this.NL);
			}
			this.CurrentIndention--;
			output.Add(this.CurrentTabIndention);
			output.Add("}");
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
