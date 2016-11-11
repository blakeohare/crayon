using System;
using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon.Translator.Ruby
{
	internal class RubyTranslator : AbstractTranslator
	{
		public RubyTranslator() : base() { }

		protected override void TranslateAssignment(List<string> output, Assignment assignment)
		{
			this.TranslateExpression(output, assignment.Target);
			output.Add(" ");
			output.Add(assignment.AssignmentOp);
			output.Add(" ");
			this.TranslateExpression(output, assignment.Value);
			output.Add(this.NL);
		}

		protected override void TranslateBinaryOpSyntax(List<string> output, string tokenValue)
		{
			base.TranslateBinaryOpSyntax(output, tokenValue);
		}

		protected override void TranslateForLoop(List<string> output, ForLoop forLoop)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDotStep(List<string> output, DotStep dotStep)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateVariable(List<string> output, Variable expr)
		{
			output.Add(expr.Name);
		}

		protected override void TranslateWhileLoop(List<string> output, WhileLoop whileLoop)
		{
			output.Add("while ");
			this.TranslateExpression(output, whileLoop.Condition);
			output.Add(this.NL);
			this.CurrentIndention++;
			this.Translate(output, whileLoop.Code);
			this.CurrentIndention--;
			output.Add(this.CurrentTabIndention);
			output.Add("end");
			output.Add(this.NL);
		}

		protected override void TranslateBooleanNot(List<string> output, BooleanNot booleanNot)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateIfStatement(List<string> output, IfStatement ifStatement)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateNullConstant(List<string> output, NullConstant nullConstant)
		{
			output.Add("nil");
		}

		protected override void TranslateNegativeSign(List<string> output, NegativeSign negativeSign)
		{
			output.Add("-(");
			this.TranslateExpression(output, negativeSign);
			output.Add(")");
		}

		protected override void TranslateFunctionCall(List<string> output, FunctionCall functionCall)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateFloatConstant(List<string> output, FloatConstant floatConstant)
		{
			output.Add(Util.FloatToString(floatConstant.Value));
		}

		protected override void TranslateDotStepStruct(List<string> output, DotStepStruct dotStepStruct)
		{
			output.Add(dotStepStruct.RootVar);
			output.Add("[");
			output.Add(dotStepStruct.StructDefinition.IndexByField[dotStepStruct.FieldName].ToString());
			output.Add("]");
		}

		protected override void TranslateIntegerConstant(List<string> output, IntegerConstant intConstant)
		{
			output.Add(intConstant.Value.ToString());
		}

		protected override void TranslateStringConstant(List<string> output, StringConstant stringConstant)
		{
			output.Add(Util.ConvertStringValueToCode(stringConstant.Value));
		}

		protected override void TranslateBreakStatement(List<string> output, BreakStatement breakStatement)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStructInstance(List<string> output, StructInstance structInstance)
		{
			output.Add("[");
			Expression[] args = structInstance.Args;
			for (int i = 0; i < args.Length; ++i)
			{
				if (i > 0) output.Add(", ");
				this.TranslateExpression(output, args[i]);
			}
			output.Add("]");
		}

		protected override void TranslateSwitchStatement(List<string> output, SwitchStatement switchStatement)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateReturnStatement(List<string> output, ReturnStatement returnStatement)
		{
			output.Add("return ");
			if (returnStatement.Expression == null)
			{
				output.Add("nil");
			}
			else
			{
				this.TranslateExpression(output, returnStatement.Expression);
			}
			output.Add(this.NL);
		}

		protected override void TranslateBooleanConstant(List<string> output, BooleanConstant booleanConstant)
		{
			output.Add(booleanConstant.Value ? "true" : "false");
		}

		protected override void TranslateFunctionDefinition(List<string> output, FunctionDefinition functionDef)
		{
			output.Add(this.NL);
			output.Add(this.CurrentTabIndention);
			output.Add("def v_");
			output.Add(functionDef.NameToken.Value);
			output.Add("(");
			Token[] args = functionDef.ArgNames;
			for (int i = 0; i < args.Length; ++i)
			{
				if (i > 0) output.Add(", ");
				output.Add("v_");
				output.Add(args[i].Value);
			}
			output.Add(")\n");
			this.CurrentIndention++;

			this.Translate(output, functionDef.Code);

			this.CurrentIndention--;
			output.Add(this.CurrentTabIndention);
			output.Add("end");
			output.Add(this.NL);
		}

		protected override void TranslateBooleanCombination(List<string> output, BooleanCombination booleanCombination)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateExpressionAsExecutable(List<string> output, ExpressionAsExecutable exprAsExec)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateSwitchStatementUnsafeBlotchy(List<string> output, SwitchStatementUnsafeBlotchy switchStatement)
		{
			this.TranslateSwitchStatementCommon(output, switchStatement.OriginalSwitchStatement);
		}

		protected override void TranslateSwitchStatementContinuousSafe(List<string> output, SwitchStatementContinuousSafe switchStatement)
		{
			this.TranslateSwitchStatementCommon(output, switchStatement.OriginalSwitchStatement);
		}

		private void TranslateSwitchStatementCommon(List<string> output, SwitchStatement switchStatement)
		{
			output.Add("case ");
			this.TranslateExpression(output, switchStatement.Condition);
			output.Add(this.NL);
			this.CurrentIndention++;
			SwitchStatement.Chunk[] chunks = switchStatement.Chunks;
			SwitchStatement.Chunk chunk;
			for (int i = 0; i < chunks.Length; ++i)
			{
				chunk = chunks[i];
				if (chunk.Cases.Length == 1 && chunk.Cases[0] == null)
				{
					output.Add("else");
				}
				else
				{
					output.Add("when ");
					for (int j = 0; j < chunk.Cases.Length; ++j)
					{
						if (j > 0) output.Add(", ");
						this.TranslateExpression(output, chunk.Cases[j]);
					}
				}
				output.Add(this.NL);
				this.CurrentIndention++;
				this.Translate(output, chunk.Code);
				this.CurrentIndention--;
			}
			this.CurrentIndention--;
			output.Add(this.CurrentTabIndention);
			output.Add("end");
			output.Add(this.NL);
		}

		protected override string TabString { get { return "    "; } }
		public override String NL { get { return "\n"; } }
	}
}
