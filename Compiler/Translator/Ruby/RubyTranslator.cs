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
			throw new NotImplementedException();
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
			throw new NotImplementedException();
		}

		protected override void TranslateWhileLoop(List<string> output, WhileLoop whileLoop)
		{
			throw new NotImplementedException();
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
			throw new NotImplementedException();
		}

		protected override void TranslateNegativeSign(List<string> output, NegativeSign negativeSign)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateFunctionCall(List<string> output, FunctionCall functionCall)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateFloatConstant(List<string> output, FloatConstant floatConstant)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDotStepStruct(List<string> output, DotStepStruct dotStepStruct)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateIntegerConstant(List<string> output, IntegerConstant intConstant)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringConstant(List<string> output, StringConstant stringConstant)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateBreakStatement(List<string> output, BreakStatement breakStatement)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStructInstance(List<string> output, StructInstance structInstance)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateSwitchStatement(List<string> output, SwitchStatement switchStatement)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateReturnStatement(List<string> output, ReturnStatement returnStatement)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateBooleanConstant(List<string> output, BooleanConstant booleanConstant)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateFunctionDefinition(List<string> output, FunctionDefinition functionDef)
		{
			throw new NotImplementedException();
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
			throw new NotImplementedException();
		}

		protected override void TranslateSwitchStatementContinuousSafe(List<string> output, SwitchStatementContinuousSafe switchStatement)
		{
			throw new NotImplementedException();
		}

		protected override string TabString { get { return "    "; } }
		public override String NL { get { return "\n"; } }
	}
}
