using System;
using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon.Translator
{
	internal abstract class AbstractTranslator
	{
		public bool IsMin { get; private set; }
		public PlatformTarget Mode { get; private set; }
		public Parser Parser { get; private set; }
		private AbstractSystemFunctionTranslator systemFunctionTranslator;

		protected string NL { get { return this.IsMin ? "" : "\r\n"; } }
		protected string Shorten(string value)
		{
			return this.IsMin ? value.Replace(" ", "") : value;
		}

		public AbstractTranslator(Parser parser, bool min, AbstractSystemFunctionTranslator systemFunctionTranslator)
		{
			this.Parser = parser;
			this.Mode = parser.Mode;
			this.IsMin = min;
			this.CurrentIndention = 0;
			this.systemFunctionTranslator = systemFunctionTranslator;
			this.systemFunctionTranslator.Translator = this;
		}

		protected abstract void TranslateIfStatement(List<string> output, IfStatement ifStatement);
		protected abstract void TranslateForLoop(List<string> output, ForLoop forLoop);
		protected abstract void TranslateFunctionDefinition(List<string> output, FunctionDefinition functionDef);
		protected abstract void TranslateAssignment(List<string> output, Assignment assignment);
		protected abstract void TranslateExpressionAsExecutable(List<string> output, ExpressionAsExecutable exprAsExec);
		protected abstract void TranslateWhileLoop(List<string> output, WhileLoop whileLoop);
		protected abstract void TranslateSwitchStatement(List<string> output, SwitchStatement switchStatement);
		protected abstract void TranslateBreakStatement(List<string> output, BreakStatement breakStatement);
		protected abstract void TranslateReturnStatement(List<string> output, ReturnStatement returnStatement);
		protected abstract void TranslateSwitchStatementContinuousSafe(List<string> output, SwitchStatementContinuousSafe switchStatement);
		protected abstract void TranslateSwitchStatementUnsafeBlotchy(List<string> output, SwitchStatementUnsafeBlotchy switchStatement);

		protected abstract void TranslateBinaryOpChain(List<string> output, BinaryOpChain expr);
		protected abstract void TranslateVariable(List<string> output, Variable expr);
		protected abstract void TranslateStructInstance(List<string> output, StructInstance structInstance);
		protected abstract void TranslateIntegerConstant(List<string> output, IntegerConstant intConstant);
		protected abstract void TranslateDictionaryDefinition(List<string> output, DictionaryDefinition dictDef);
		protected abstract void TranslateListDefinition(List<string> output, ListDefinition listDef);
		protected abstract void TranslateNullConstant(List<string> output, NullConstant nullConstant);
		protected abstract void TranslateFunctionCall(List<string> output, FunctionCall functionCall);
		protected abstract void TranslateBracketIndex(List<string> output, BracketIndex bracketIndex);
		protected abstract void TranslateBooleanConstant(List<string> output, BooleanConstant booleanConstant);
		protected abstract void TranslateDotStep(List<string> output, DotStep dotStep);
		protected abstract void TranslateStringConstant(List<string> output, StringConstant stringConstant);
		protected abstract void TranslateNegativeSign(List<string> output, NegativeSign negativeSign);
		protected abstract void TranslateBooleanCombination(List<string> output, BooleanCombination booleanCombination);
		protected abstract void TranslateBooleanNot(List<string> output, BooleanNot booleanNot);
		protected abstract void TranslateFloatConstant(List<string> output, FloatConstant floatConstant);

		private void TranslateSystemFunctionCall(List<string> output, SystemFunctionCall systemFunctionCall)
		{
			this.systemFunctionTranslator.Translate(this.CurrentTabIndention, output, systemFunctionCall);
		}

		private void TranslateFunctionDefinitionWrapped(List<string> output, FunctionDefinition functionDef)
		{
			foreach (Expression expr in functionDef.DefaultValues)
			{
				if (expr != null)
				{
					throw new ParserException(functionDef.FirstToken, "Code translation mode does not support function argument default values.");
				}
			}

			this.TranslateFunctionDefinition(output, functionDef);
		}

		private int currentIndention = 0;
		public int CurrentIndention
		{
			get { return this.currentIndention; }
			set
			{
				this.currentIndention = value;
				string tabs = "";
				while (value-- > 0)
				{
					tabs += this.TabString;
				}
				this.tabIndention = tabs;
			}
		}

		protected abstract string TabString { get; }

		private string tabIndention = "";
		public string CurrentTabIndention { get { return (this.IsMin && !(this is Python.PythonTranslator)) ? "" : this.tabIndention; } }

		// TODO: Nope. This works for JS and PY but will fail miserable on more structured programming languages that don't just allow you to
		// stick arbitrary code at the front of a file.
		public string DoTranslationOfInterpreterClassWithEmbeddedByteCode(Parser parser, Executable[] finalCodeBase)
		{
			string prefix = this.systemFunctionTranslator.Platform.SerializeBoilerPlates(parser);

			return BeginTranslation(prefix, finalCodeBase);
		}

		private string BeginTranslation(string prefix, Executable[] code)
		{
			List<string> output = new List<string>() { prefix, "\r\n" };
			this.Translate(output, code);
			return string.Join("", output);
		}

		public void Translate(List<string> output, Executable[] lines)
		{
			foreach (Executable line in lines)
			{
				Translate(output, line);
			}
		}

		public void Translate(List<string> output, Executable exec)
		{
			if (exec is IfStatement) this.TranslateIfStatement(output, (IfStatement)exec);
			else if (exec is ForLoop) this.TranslateForLoop(output, (ForLoop)exec);
			else if (exec is FunctionDefinition) this.TranslateFunctionDefinitionWrapped(output, (FunctionDefinition)exec);
			else if (exec is Assignment) this.TranslateAssignment(output, (Assignment)exec);
			else if (exec is ExpressionAsExecutable) this.TranslateExpressionAsExecutable(output, (ExpressionAsExecutable)exec);
			else if (exec is WhileLoop) this.TranslateWhileLoop(output, (WhileLoop)exec);
			else if (exec is SwitchStatement) this.TranslateSwitchStatement(output, (SwitchStatement)exec);
			else if (exec is BreakStatement) this.TranslateBreakStatement(output, (BreakStatement)exec);
			else if (exec is ReturnStatement) this.TranslateReturnStatement(output, (ReturnStatement)exec);
			else if (exec is SwitchStatementContinuousSafe) this.TranslateSwitchStatementContinuousSafe(output, (SwitchStatementContinuousSafe)exec);
			else if (exec is SwitchStatementUnsafeBlotchy) this.TranslateSwitchStatementUnsafeBlotchy(output, (SwitchStatementUnsafeBlotchy)exec);
			else throw new Exception("Executable type not handled: " + exec.GetType());
		}

		public void TranslateExpression(List<string> output, Expression expr)
		{
			if (expr is BinaryOpChain) this.TranslateBinaryOpChain(output, (BinaryOpChain)expr);
			else if (expr is Variable) this.TranslateVariable(output, (Variable)expr);
			else if (expr is IntegerConstant) this.TranslateIntegerConstant(output, (IntegerConstant)expr);
			else if (expr is StructInstance) this.TranslateStructInstance(output, (StructInstance)expr);
			else if (expr is DictionaryDefinition) this.TranslateDictionaryDefinition(output, (DictionaryDefinition)expr);
			else if (expr is ListDefinition) this.TranslateListDefinition(output, (ListDefinition)expr);
			else if (expr is NullConstant) this.TranslateNullConstant(output, (NullConstant)expr);
			else if (expr is FunctionCall) this.TranslateFunctionCall(output, (FunctionCall)expr);
			else if (expr is BracketIndex) this.TranslateBracketIndex(output, (BracketIndex)expr);
			else if (expr is BooleanConstant) this.TranslateBooleanConstant(output, (BooleanConstant)expr);
			else if (expr is DotStep) this.TranslateDotStep(output, (DotStep)expr);
			else if (expr is Increment) throw new ParserException(expr.FirstToken, "++ and -- aren't allowed in translation mode.");
			else if (expr is StringConstant) this.TranslateStringConstant(output, (StringConstant)expr);
			else if (expr is SystemFunctionCall) this.TranslateSystemFunctionCall(output, (SystemFunctionCall)expr);
			else if (expr is NegativeSign) this.TranslateNegativeSign(output, (NegativeSign)expr);
			else if (expr is BooleanCombination) this.TranslateBooleanCombination(output, (BooleanCombination)expr);
			else if (expr is BooleanNot) this.TranslateBooleanNot(output, (BooleanNot)expr);
			else if (expr is FloatConstant) this.TranslateFloatConstant(output, (FloatConstant)expr);
			else throw new Exception("Expression type not handled: " + expr.GetType());
		}
	}
}
