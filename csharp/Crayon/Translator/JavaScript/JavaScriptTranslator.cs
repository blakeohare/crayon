using System;
using System.Collections.Generic;
using Crayon.ParseTree;
using System.Linq;

namespace Crayon.Translator.JavaScript
{
	internal class JavaScriptTranslator : AbstractTranslator
	{
		public JavaScriptTranslator(Parser parser, bool min, AbstractPlatformImplementation platform)
			: base(parser, min, new JavaScriptSystemFunctionTranslator(platform))
		{ }

		protected override void TranslateSwitchStatementContinuousSafe(List<string> output, SwitchStatementContinuousSafe switchStatement)
		{
			foreach (SwitchStatement.Chunk chunks in switchStatement.OriginalSwitchStatement.Chunks)
			{
				if (chunks.Cases.Length > 1) throw new Exception("Not a continuous switch statement.");
				if (chunks.Cases[0] == null) throw new Exception("Not a safe switch statement.");
			}

			this.TranslateSwitchStatement(output, switchStatement.OriginalSwitchStatement);
		}

		protected override void TranslateSwitchStatementUnsafeBlotchy(List<string> output, SwitchStatementUnsafeBlotchy switchStatement)
		{
			this.TranslateSwitchStatement(output, switchStatement.OriginalSwitchStatement);
		}

		private void TranslateSwitchStatementImpl(List<string> output, SwitchStatement switchStatement)
		{
			output.Add(this.CurrentTabIndention);
			output.Add("switch(");
			this.TranslateExpression(output, switchStatement.Condition);
			output.Add(") {\r\n");
			this.CurrentIndention++;
			foreach (SwitchStatement.Chunk chunks in switchStatement.Chunks)
			{
				foreach (Expression caseExpr in chunks.Cases)
				{
					output.Add(this.CurrentTabIndention);
					if (chunks.Cases[0] == null)
					{
						output.Add("default:\r\n");
					}
					else
					{
						output.Add("case ");
						this.TranslateExpression(output, chunks.Cases[0]);
						output.Add(":\r\n");
					}
				}

				this.CurrentIndention++;
				this.Translate(output, chunks.Code);
				output.Add(this.CurrentTabIndention);
				output.Add("break;\r\n");
				this.CurrentIndention--;
			}
			this.CurrentIndention--;
			output.Add(this.CurrentTabIndention);
			output.Add("}\r\n");
		}

		protected override void TranslateFloatConstant(List<string> output, FloatConstant floatConstant)
		{
			output.Add(Util.FloatToString(floatConstant.Value));
		}

		protected override void TranslateBooleanNot(List<string> output, BooleanNot booleanNot)
		{
			output.Add("!(");
			this.TranslateExpression(output, booleanNot.Root);
			output.Add(")");
		}

		protected override void TranslateBooleanCombination(List<string> output, BooleanCombination booleanCombination)
		{
			output.Add("(");
			this.TranslateExpression(output, booleanCombination.Expressions[0]);
			output.Add(")");

			for (int i = 0; i < booleanCombination.Ops.Length; ++i)
			{
				if (booleanCombination.Ops[i].Value == "&&")
				{
					output.Add(" && ");
				}
				else
				{
					output.Add(" || ");
				}

				output.Add("(");
				this.TranslateExpression(output, booleanCombination.Expressions[i + 1]);
				output.Add(")");
			}
		}

		protected override void TranslateNegativeSign(List<string> output, NegativeSign negativeSign)
		{
			output.Add("-(");
			this.TranslateExpression(output, negativeSign.Root);
			output.Add(")");
		}

		protected override void TranslateStringConstant(List<string> output, StringConstant stringConstant)
		{
			output.Add("\"");
			string value = stringConstant.Value;
			foreach (char c in value)
			{
				string nc = "" + c;
				switch (c)
				{
					case '\\': nc = "\\\\"; break;
					case '"': nc = "\\\""; break;
					case '\0': nc = "\\0"; break;
					case '\t': nc = "\\t"; break;
					case '\n': nc = "\\n"; break;
					case '\r': nc = "\\r"; break;
					default: break;
				}
				output.Add(nc);
			}
			output.Add("\"");
		}

		protected override void TranslateReturnStatement(List<string> output, ReturnStatement returnStatement)
		{
			output.Add(this.CurrentTabIndention);
			output.Add("return ");
			if (returnStatement.Expression != null)
			{
				TranslateExpression(output, returnStatement.Expression);
			}
			else
			{
				output.Add("null");
			}
			output.Add(";\r\n");
		}
		protected override void TranslateBreakStatement(List<string> output, BreakStatement breakStatement)
		{
			output.Add(this.CurrentTabIndention);
			output.Add("break;\r\n");
		}

		protected override void TranslateDotStep(List<string> output, DotStep dotStep)
		{
			// okay, you're going to have to do fancy crap for system dotSteps. Like foo.length where foo is a string. More casting methinks
			// but that would have to have happened before now. Everything at this point is a true dot step.

			// TODO: determine if the root is atomic. If so, leave off parenthesis.
			output.Add("(");
			TranslateExpression(output, dotStep.Root);
			output.Add(")");
			output.Add(".f_");
			output.Add(dotStep.StepToken.Value);
		}

		protected override void TranslateSwitchStatement(List<string> output, SwitchStatement switchStatement)
		{
			output.Add(this.CurrentTabIndention);
			output.Add("switch (");
			this.TranslateExpression(output, switchStatement.Condition);
			output.Add(") {\r\n");
			this.CurrentIndention++;

			foreach (SwitchStatement.Chunk chunk in switchStatement.Chunks)
			{
				foreach (Expression caseExpr in chunk.Cases)
				{
					output.Add(this.CurrentTabIndention);
					if (caseExpr == null)
					{
						output.Add("default:\r\n");
					}
					else
					{
						output.Add("case ");
						TranslateExpression(output, caseExpr);
						output.Add(":\r\n");
					}

					this.CurrentIndention++;

					Translate(output, chunk.Code);

					if (chunk.ContainsFallthrough)
					{
						//output.Add(this.CurrentTabIndention);
						//output.Add("break;\r\n");
						//output.Add("\r\n");
					}

					this.CurrentIndention--;
				}
			}

			this.CurrentIndention--;
			output.Add(this.CurrentTabIndention);
			output.Add("}\r\n");
		}

		protected override void TranslateBooleanConstant(List<string> output, BooleanConstant booleanConstant)
		{
			output.Add(booleanConstant.Value ? "true" : "false");
		}

		protected override void TranslateWhileLoop(List<string> output, WhileLoop whileLoop)
		{
			output.Add(this.CurrentTabIndention);
			output.Add("while (");
			this.TranslateExpression(output, whileLoop.Condition);
			output.Add(") {\r\n");
			this.CurrentIndention++;
			this.Translate(output, whileLoop.Code);
			this.CurrentIndention--;
			output.Add(this.CurrentTabIndention);
			output.Add("}\r\n");
		}

		protected override void TranslateExpressionAsExecutable(List<string> output, ExpressionAsExecutable exprAsExec)
		{
			output.Add(this.CurrentTabIndention);
			TranslateExpression(output, exprAsExec.Expression);
			output.Add(";\r\n");
		}

		protected override void TranslateBracketIndex(List<string> output, BracketIndex bracketIndex)
		{
			TranslateExpression(output, bracketIndex.Root);
			output.Add("[");
			TranslateExpression(output, bracketIndex.Index);
			output.Add("]");
		}

		protected override void TranslateFunctionCall(List<string> output, FunctionCall functionCall)
		{
			TranslateExpression(output, functionCall.Root);
			output.Add("(");
			for (int i = 0; i < functionCall.Args.Length; ++i)
			{
				if (i > 0) output.Add(", ");
				TranslateExpression(output, functionCall.Args[i]);
			}
			output.Add(")");
		}

		protected override void TranslateNullConstant(List<string> output, NullConstant nullConstant)
		{
			output.Add("null");
		}

		protected override void TranslateListDefinition(List<string> output, ListDefinition listDef)
		{
			output.Add("[");
			for (int i = 0; i < listDef.Items.Length; ++i)
			{
				if (i > 0) output.Add(", ");
				TranslateExpression(output, listDef.Items[i]);
			}
			output.Add("]");
		}

		// This will fail if you put non string expressions in translated dictionaries.
		protected override void TranslateDictionaryDefinition(List<string> output, DictionaryDefinition dictDef)
		{
			output.Add("{");
			for (int i = 0; i < dictDef.Keys.Length; ++i)
			{
				if (i > 0) output.Add(", ");
				TranslateExpression(output, dictDef.Keys[i]);
				output.Add(": ");
				TranslateExpression(output, dictDef.Values[i]);
			}
			output.Add(" }");
		}

		protected override void TranslateIntegerConstant(List<string> output, IntegerConstant intConstant)
		{
			int value = intConstant.Value;
			if (value >= 0)
			{
				output.Add("" + value);
			}
			else
			{
				output.Add("(" + value + ")");
			}
		}

		protected override void TranslateStructInstance(List<string> output, StructInstance structInstance)
		{
			output.Add("[");
			for (int i = 0; i < structInstance.Args.Length; ++i)
			{
				if (i > 0) output.Add(", ");
				this.TranslateExpression(output, structInstance.Args[i]);
			}
			output.Add("]");
		}

		protected override void TranslateAssignment(List<string> output, Assignment assignment)
		{
			output.Add(this.CurrentTabIndention);
			this.TranslateExpression(output, assignment.Target);
			output.Add(" ");
			output.Add(assignment.AssignmentOp);
			output.Add(" ");
			this.TranslateExpression(output, assignment.Value);
			output.Add(";\r\n");
		}

		protected override string TabString
		{
			get { return "\t"; }
		}

		protected override void TranslateIfStatement(List<string> output, IfStatement exec)
		{
			output.Add(this.CurrentTabIndention);
			output.Add("if (");
			TranslateExpression(output, exec.Condition);
			output.Add(") {\r\n");
			this.CurrentIndention++;
			this.Translate(output, exec.TrueCode);
			this.CurrentIndention--;

			output.Add(this.CurrentTabIndention);
			output.Add("}");
			if (exec.FalseCode.Length > 0)
			{
				output.Add(" else {\r\n");
				this.CurrentIndention++;
				this.Translate(output, exec.FalseCode);
				this.CurrentIndention--;
				output.Add(this.CurrentTabIndention);
				output.Add("}");
			}
			output.Add("\r\n");
		}

		protected override void TranslateForLoop(List<string> output, ForLoop exec)
		{
			this.Translate(output, exec.Init);
			output.Add(this.CurrentTabIndention);
			output.Add("while (");
			this.TranslateExpression(output, exec.Condition);
			output.Add(") {\r\n");
			this.CurrentIndention++;
			this.Translate(output, exec.Code);
			this.Translate(output, exec.Step);
			this.CurrentIndention--;
			output.Add(this.CurrentTabIndention);
			output.Add("}\r\n");
		}

		protected override void TranslateBinaryOpChain(List<string> output, BinaryOpChain expr)
		{
			List<string> t = new List<string>() { "(" };
			TranslateExpression(t, expr.Expressions[0]);
			t.Add(")");
			for (int i = 0; i < expr.Ops.Length; ++i)
			{
				string op = expr.Ops[i].Value;

				switch (op)
				{
					case "+":
					case "-":
					case "*":
					case "/":
					case "%":
					case "<<":
					case ">>":
					case "<":
					case ">":
					case "==":
					case "!=":
					case "<=":
					case ">=":
					case "&":
					case "|":
					case "^":
						t.Insert(0, "(");
						t.Add(" " + op + " ");
						TranslateExpression(t, expr.Expressions[i + 1]);
						t.Add(")");
						break;

					case "**":
						t.Insert(0, "Math.pow(");
						t.Add(", ");
						TranslateExpression(t, expr.Expressions[i + 1]);
						t.Add(")");
						break;

					default:
						throw new Exception("Unknown binary op: " + op);
				}
			}

			output.AddRange(t);
		}

		protected override void TranslateVariable(List<string> output, Variable expr)
		{
			output.Add(this.Parser.GetVariableName(expr.Name));
		}

		protected override void TranslateFunctionDefinition(List<string> output, FunctionDefinition functionDef)
		{
			output.Add("\r\n");
			output.Add(this.CurrentTabIndention);
			output.Add("function ");
			output.Add("v_" + functionDef.NameToken.Value);
			output.Add("(");
			for (int i = 0; i < functionDef.ArgNames.Length; ++i)
			{
				if (i > 0) output.Add(", ");
				output.Add(functionDef.ArgNames[i].Value);
			}
			output.Add(") {\r\n");
			this.CurrentIndention++;

			foreach (string varName in functionDef.GetVariableDeclarationList())
			{
				output.Add(this.CurrentTabIndention);
				output.Add("var v_" + varName + " = null;\r\n");
			}

			Translate(output, functionDef.Code);
			this.CurrentIndention--;
			output.Add(this.CurrentTabIndention);
			output.Add("}\r\n");
		}
	}
}
