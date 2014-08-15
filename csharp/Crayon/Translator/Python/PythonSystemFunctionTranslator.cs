using System;
using System.Collections.Generic;

namespace Crayon.Translator.Python
{
	internal class PythonSystemFunctionTranslator : AbstractSystemFunctionTranslator
	{
		public PythonSystemFunctionTranslator(AbstractPlatformImplementation platform)
			: base(platform)
		{

		}

		protected override void TranslatePauseForFrame(List<string> output)
		{
			output.Add("pygame.display.flip()\r\n");
			output.Add(this.Translator.CurrentTabIndention);
			output.Add("_global_vars['clock'].tick(_global_vars['fps'])");
		}

		protected override void TranslateUnregisterTicker(List<string> output)
		{
			throw new Exception("This code path should be optimized out of the python translation.");
		}

		protected override void TranslateRegisterTimeout(List<string> output)
		{
			throw new Exception("This code path should be optimized out of the python translation.");
		}

		protected override void TranslateRegisterTicker(List<string> output)
		{
			throw new Exception("This code path should be optimized out of the python translation.");
		}

		protected override void TranslateComment(List<string> output, ParseTree.Expression commentValue)
		{
#if DEBUG
			output.Add("# " + ((ParseTree.StringConstant)commentValue).Value);
#endif
		}

		protected override void TranslateUnsafeFloatDivision(List<string> output, ParseTree.Expression numerator, ParseTree.Expression denominator)
		{
			output.Add("1.0 * ");
			this.Translator.TranslateExpression(output, numerator);
			output.Add(" / ");
			this.Translator.TranslateExpression(output, denominator);
		}

		protected override void TranslateStringCharAt(List<string> output, ParseTree.Expression stringValue, ParseTree.Expression index)
		{
			this.Translator.TranslateExpression(output, stringValue);
			output.Add("[");
			this.Translator.TranslateExpression(output, index);
			output.Add("]");
		}

		protected override void TranslateListSplit(List<string> output, ParseTree.Expression originalString, ParseTree.Expression sep)
		{
			this.Translator.TranslateExpression(output, originalString);
			output.Add(".split(");
			this.Translator.TranslateExpression(output, sep);
			output.Add(")");
		}

		protected override void TranslateListSet(List<string> output, ParseTree.Expression list, ParseTree.Expression index, ParseTree.Expression value)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add("[");
			this.Translator.TranslateExpression(output, index);
			output.Add("] = ");
			this.Translator.TranslateExpression(output, value);
		}

		protected override void TranslateUnsafeIntegerDivision(List<string> output, ParseTree.Expression numerator, ParseTree.Expression denominator)
		{
			this.Translator.TranslateExpression(output, numerator);
			output.Add(" // ");
			this.Translator.TranslateExpression(output, denominator);
		}

		protected override void TranslateListGet(List<string> output, ParseTree.Expression list, ParseTree.Expression index)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add("[");
			this.Translator.TranslateExpression(output, index);
			output.Add("]");
		}

		protected override void TranslateListLength(List<string> output, ParseTree.Expression list)
		{
			output.Add("len(");
			this.Translator.TranslateExpression(output, list);
			output.Add(")");
		}

		protected override void TranslateDictionaryGet(List<string> output, ParseTree.Expression dictionary, ParseTree.Expression key, ParseTree.Expression defaultValue)
		{
			this.Translator.TranslateExpression(output, dictionary);
			output.Add(".get(");
			this.Translator.TranslateExpression(output, key);
			output.Add(", ");
			this.Translator.TranslateExpression(output, defaultValue);
			output.Add(")");
		}

		protected override void TranslateListReverse(List<string> output, ParseTree.Expression listVar)
		{
			this.Translator.TranslateExpression(output, listVar);
			output.Add(" = ");
			this.Translator.TranslateExpression(output, listVar);
			output.Add("[::-1]");
		}

		protected override void TranslatePrint(List<string> output, ParseTree.Expression message)
		{
			output.Add("print(");
			this.Translator.TranslateExpression(output, message);
			output.Add(")");
		}

		protected override void TranslateDictionarySet(List<string> output, ParseTree.Expression dict, ParseTree.Expression key, ParseTree.Expression value)
		{
			this.Translator.TranslateExpression(output, dict);
			output.Add("[");
			this.Translator.TranslateExpression(output, key);
			output.Add("] = ");
			this.Translator.TranslateExpression(output, value);
		}

		protected override void TranslateStringCast(List<string> output, ParseTree.Expression thing, bool strongCast)
		{
			output.Add("str(");
			this.Translator.TranslateExpression(output, thing);
			output.Add(")");
		}

		// TODO: this is supposed to be in the pygame platform stuff.
		// Also, implement each switch result as an abstract function
		protected override void TranslateInsertFrameworkCode(string tab, List<string> output, string id)
		{
			switch (id)
			{
				case "ff_current_time":
					output.Add("v_output = [" + (int)Types.FLOAT + ", time.time()]");
					break;

				case "ff_draw_rectangle":
					// TODO: alpha?
					output.Add("_PDR(v_scr[1][1], (v_red[1], v_green[1], v_blue[1]), _PR(v_x[1], v_y[1], v_width[1], v_height[1]))");
					break;

				case "ff_fill_screen":
					output.Add("v_scr[1][1].fill((v_red[1], v_green[1], v_blue[1]))");
					break;

				case "ff_invalidate_display":
					output.Add("pass");
					break;

				case "ff_floor":
					output.Add("v_output = v_build_integer(int(v_value[1]))");
					break;

				case "ff_get_events":
					output.Add("v_output = _pygame_pump_events()");
					break;

				case "ff_initialize_game":
					output.Add("platform_begin(v_fps[1])");
					break;

				case "ff_initialize_screen":
					output.Add("v_output = _pygame_initialize_screen(v_width[1], v_height[1])");
					break;

				case "ff_print":
					output.Add("print(str(v_valueStack.pop()))");
					break;

				case "ff_random":
					output.Add("v_output = [" + (int)Types.FLOAT + ", random.random()]");
					break;

				case "ff_set_title":
					output.Add("pygame.display.set_caption(v_value)");
					break;

				default:
					throw new NotImplementedException();
			}
		}

		protected override void TranslateListPush(List<string> output, ParseTree.Expression list, ParseTree.Expression value)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add(".append(");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateListRemoveAt(List<string> output, ParseTree.Expression list, ParseTree.Expression index)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add(".pop(");
			this.Translator.TranslateExpression(output, index);
			output.Add(")");
		}

		protected override void TranslateStringLength(List<string> output, ParseTree.Expression stringValue)
		{
			output.Add("len(");
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(")");
		}

		protected override void TranslateKillExecution(List<string> output, ParseTree.Expression exceptionMessage)
		{
			output.Add("_kill_execution(");
			this.Translator.TranslateExpression(output, exceptionMessage);
			output.Add(")");

		}

		protected override void TranslateListPop(List<string> output, ParseTree.Expression list)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add(".pop()");
		}
	}
}
