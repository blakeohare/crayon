using System;
using System.Collections.Generic;
using System.Linq;
using Crayon.ParseTree;

namespace Crayon.Translator
{
	internal abstract class CurlyBraceImplementation : AbstractTranslator
	{
		private readonly bool isEgyptian;

		public CurlyBraceImplementation(bool isEgyptian)
			: base()
		{
			this.isEgyptian = isEgyptian;
		}

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

		protected override void TranslateSwitchStatement(List<string> output, SwitchStatement switchStatement)
		{
			output.Add(this.CurrentTabIndention);
			output.Add("switch (");
			this.TranslateExpression(output, switchStatement.Condition);
			output.Add(") {" + this.NL);
			this.CurrentIndention++;

			foreach (SwitchStatement.Chunk chunk in switchStatement.Chunks)
			{
				foreach (Expression caseExpr in chunk.Cases)
				{
					output.Add(this.CurrentTabIndention);
					if (caseExpr == null)
					{
						output.Add("default:" + this.NL);
					}
					else
					{
						output.Add("case ");
						TranslateExpression(output, caseExpr);
						output.Add(":" + this.NL);
					}

					this.CurrentIndention++;

					Translate(output, chunk.Code);

					this.CurrentIndention--;
				}
			}

			this.CurrentIndention--;
			output.Add(this.CurrentTabIndention);
			output.Add("}" + this.NL);
		}

		protected override void TranslateFloatConstant(List<string> output, FloatConstant floatConstant)
		{
			output.Add(Util.FloatToString(floatConstant.Value));
		}

		protected override void TranslateNegativeSign(List<string> output, NegativeSign negativeSign)
		{
			output.Add("-(");
			this.TranslateExpression(output, negativeSign.Root);
			output.Add(")");
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

		protected override void TranslateIfStatement(List<string> output, IfStatement ifStatement)
		{
			output.Add(this.CurrentTabIndention);
			output.Add("if (");
			this.TranslateExpression(output, ifStatement.Condition);
			if (isEgyptian)
			{
				output.Add(") {" + this.NL);
			}
			else
			{
				output.Add(")");
				output.Add(this.NL);
				output.Add(this.CurrentTabIndention);
				output.Add("{");
				output.Add(this.NL);
			}
			this.CurrentIndention++;
			this.Translate(output, ifStatement.TrueCode);
			this.CurrentIndention--;

			output.Add(this.CurrentTabIndention);
			output.Add("}");
			if (ifStatement.FalseCode.Length > 0)
			{
				if (this.isEgyptian)
				{
					output.Add(" else {" + this.NL);
				}
				else
				{
					output.Add(this.NL);
					output.Add(this.CurrentTabIndention);
					output.Add("else");
					output.Add(this.NL);
					output.Add(this.CurrentTabIndention);
					output.Add("{");
					output.Add(this.NL);
				}
				this.CurrentIndention++;
				this.Translate(output, ifStatement.FalseCode);
				this.CurrentIndention--;
				output.Add(this.CurrentTabIndention);
				output.Add("}");
			}
			output.Add(this.NL);
		}

		protected override void TranslateBreakStatement(List<string> output, BreakStatement breakStatement)
		{
			output.Add(this.CurrentTabIndention);
			output.Add("break;" + this.NL);
		}

		protected override void TranslateBooleanConstant(List<string> output, BooleanConstant booleanConstant)
		{
			output.Add(booleanConstant.Value ? "true" : "false");
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
				this.TranslateNullConstant(output, null);
			}
			output.Add(";" + this.NL);
		}

		protected override void TranslateDotStep(List<string> output, DotStep dotStep)
		{
			output.Add("(");
			TranslateExpression(output, dotStep.Root);
			output.Add(")");
			output.Add(".f_");
			output.Add(dotStep.StepToken.Value);
		}

		protected override void TranslateExpressionAsExecutable(List<string> output, ExpressionAsExecutable exprAsExec)
		{
			output.Add(this.CurrentTabIndention);
			TranslateExpression(output, exprAsExec.Expression);
			output.Add(";" + this.NL);
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

		protected override string TabString
		{
			get { return "\t"; }
		}

		protected override void TranslateWhileLoop(List<string> output, WhileLoop whileLoop)
		{
			output.Add(this.CurrentTabIndention);
			output.Add("while (");
			this.TranslateExpression(output, whileLoop.Condition);
			if (this.isEgyptian)
			{
				output.Add(") {" + this.NL);
			}
			else
			{
				output.Add(")");
				output.Add(this.NL);
				output.Add(this.CurrentTabIndention);
				output.Add("{");
				output.Add(this.NL);
			}
			this.CurrentIndention++;
			this.Translate(output, whileLoop.Code);
			this.CurrentIndention--;
			output.Add(this.CurrentTabIndention);
			output.Add("}" + this.NL);
		}

		protected override void TranslateVariable(List<string> output, Variable expr)
		{
			output.Add(this.GetVariableName(expr.Name));
		}

		protected override void TranslateForLoop(List<string> output, ForLoop exec)
		{
			this.Translate(output, exec.Init);
			output.Add(this.CurrentTabIndention);
			output.Add("while (");
			this.TranslateExpression(output, exec.Condition);
			if (this.isEgyptian)
			{
				output.Add(") {" + this.NL);
			}
			else
			{
				output.Add(")");
				output.Add(this.NL);
				output.Add(this.CurrentTabIndention);
				output.Add("{");
				output.Add(this.NL);
			}
			this.CurrentIndention++;
			this.Translate(output, exec.Code);
			this.Translate(output, exec.Step);
			this.CurrentIndention--;
			output.Add(this.CurrentTabIndention);
			output.Add("}" + this.NL);
		}
	}
}
