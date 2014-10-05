using System;
using System.Collections.Generic;

namespace Crayon.Translator.JavaScript
{
	internal class JavaScriptSystemFunctionTranslator : AbstractSystemFunctionTranslator
	{
		public JavaScriptSystemFunctionTranslator(AbstractPlatformImplementation platform)
			: base(platform)
		{ }

		protected override void TranslateBeginFrame(List<string> output)
		{
			output.Add("R.beginFrame()");
		}

		protected override void TranslateComment(List<string> output, ParseTree.Expression commentValue)
		{
#if DEBUG
			if (!this.IsMin)
			{
				output.Add("// " + ((ParseTree.StringConstant)commentValue).Value);
			}
#endif
		}

		protected override void TranslateDictionaryContains(List<string> output, ParseTree.Expression dictionary, ParseTree.Expression key)
		{
			output.Add("(");
			this.Translator.TranslateExpression(output, dictionary);
			output.Add("[");
			this.Translator.TranslateExpression(output, key);
			output.Add(this.Shorten("] !== undefined)"));
		}

		protected override void TranslateDictionaryGet(List<string> output, ParseTree.Expression dictionary, ParseTree.Expression key, ParseTree.Expression defaultValue)
		{
			output.Add("slow_dictionary_get(");
			this.Translator.TranslateExpression(output, dictionary);
			output.Add(this.Shorten(", "));
			this.Translator.TranslateExpression(output, key);
			output.Add(this.Shorten(", "));
			this.Translator.TranslateExpression(output, defaultValue);
			output.Add(")");
		}

		protected override void TranslateDictionaryGetKeys(List<string> output, ParseTree.Expression dictionary)
		{
			output.Add("slow_dictionary_get_keys(");
			this.Translator.TranslateExpression(output, dictionary);
			output.Add(")");
		}

		protected override void TranslateDictionaryGetValues(List<string> output, ParseTree.Expression dictionary)
		{
			output.Add("slow_dictionary_get_values(");
			this.Translator.TranslateExpression(output, dictionary);
			output.Add(")");
		}

		protected override void TranslateDictionaryRemove(List<string> output, ParseTree.Expression dictionary, ParseTree.Expression key)
		{
			output.Add("delete ");
			this.Translator.TranslateExpression(output, dictionary);
			output.Add("[");
			this.Translator.TranslateExpression(output, key);
			output.Add("]");
		}

		protected override void TranslateDictionarySet(List<string> output, ParseTree.Expression dict, ParseTree.Expression key, ParseTree.Expression value)
		{
			this.Translator.TranslateExpression(output, dict);
			output.Add("[");
			this.Translator.TranslateExpression(output, key);
			output.Add(this.Shorten("] = "));
			this.Translator.TranslateExpression(output, value);
		}

		protected override void TranslateDictionarySize(List<string> output, ParseTree.Expression dictionary)
		{
			output.Add("Object.keys(");
			this.Translator.TranslateExpression(output, dictionary);
			output.Add(").length");
		}

		protected override void TranslateExponent(List<string> output, ParseTree.Expression baseNum, ParseTree.Expression powerNum)
		{
			output.Add("Math.pow(");
			this.Translator.TranslateExpression(output, baseNum);
			output.Add(this.Shorten(", "));
			this.Translator.TranslateExpression(output, powerNum);
			output.Add(")");
		}

		protected override void TranslateInsertFrameworkCode(string tab, List<string> output, string id)
		{
			switch (id)
			{
				case "ff_arctan2":
					output.Add(this.Shorten("v_output = [" + (int)Types.FLOAT + ", Math.atan2(v_y[1], v_x[1])]"));
					break;

				case "ff_blit_image":
					output.Add(this.Shorten("R.blit(v_image[1][1], v_x[1], v_y[1])"));
					break;

				case "ff_cos":
					output.Add(this.Shorten("v_output = [" + (int)Types.FLOAT + ", Math.cos(v_x[1])]"));
					break;

				case "ff_current_time":
					output.Add(this.Shorten("v_output = [" + (int)Types.FLOAT + ", R.now()]"));
					break;

				case "ff_download_image":
					output.Add(this.Shorten("R.enqueue_image_download(v_key[1], v_url[1])"));
					break;

				case "ff_draw_ellipse":
					output.Add(this.Shorten("R.drawEllipse(v_left[1] + v_width[1] / 2, v_top[1] + v_height[1] / 2, v_width[1] / 2, v_height[1] / 2, v_red[1], v_green[1], v_blue[1])"));
					break;

				case "ff_draw_line":
					output.Add(this.Shorten("R.drawLine(v_x1[1], v_y1[1], v_x2[1], v_y2[1], v_width[1], v_red[1], v_green[1], v_blue[1])"));
					break;

				case "ff_draw_rectangle":
					output.Add(this.Shorten("R.drawRect(v_x[1], v_y[1], v_width[1], v_height[1], v_red[1], v_green[1], v_blue[1])"));
					break;

				case "ff_fill_screen":
					output.Add(this.Shorten("R.fillScreen(v_red[1], v_green[1], v_blue[1])"));
					break;

				case "ff_flip_image":
					output.Add("v_output = R.flipImage(v_img[1], v_x[1], v_y[1])");
					break;

				case "ff_floor":
					output.Add(this.Shorten("v_output = [" + (int)Types.INTEGER + ", Math.floor(v_value[1])]"));
					break;

				case "ff_get_events":
					output.Add(this.Shorten("v_output = [" + (int)Types.LIST + ", R.pump_event_objects()]"));
					break;

				case "ff_get_image":
					output.Add(this.Shorten("v_output = R.get_image_impl(v_key[1])"));
					break;

				case "ff_get_image_height":
					output.Add(this.Shorten("v_output = v_build_integer(v_value[1][1].height)"));
					break;

				case "ff_get_image_width":
					output.Add(this.Shorten("v_output = v_build_integer(v_value[1][1].width)"));
					break;

				case "ff_initialize_game":
					output.Add("R.initializeGame(v_fps[1])");
					break;

				case "ff_initialize_screen":
					output.Add(this.Shorten("R.initializeScreen(v_width[1], v_height[1], null, null)"));
					break;

				case "ff_initialize_screen_scaled":
					output.Add(this.Shorten("R.initializeScreen(v_width[1], v_height[1], v_pwidth[1], v_pheight[1])"));
					break;

				case "ff_is_image_loaded":
					output.Add(this.Shorten("v_output = R.is_image_loaded(v_key[1]) ? v_VALUE_TRUE : v_VALUE_FALSE"));
					break;

				case "ff_parse_int":
					// TODO: need to throw if not an integer
					output.Add(this.Shorten("v_output = [" + (int)Types.INTEGER + ", parseInt(v_value[1])]"));
					break;

				case "ff_print":
					output.Add("R.print(v_string1)");
					break;

				case "ff_random":
					output.Add(this.Shorten("v_output = [" + (int)Types.FLOAT + ", Math.random()];"));
					break;

				case "ff_set_title":
					output.Add("TODO('set title...is this possible?');");
					break;

				case "ff_sin":
					output.Add(this.Shorten("v_output = [" + (int)Types.FLOAT + ", Math.sin(v_x[1])]"));
					break;

				default:
					throw new NotImplementedException();
			}
		}

		protected override void TranslateInt(List<string> output, ParseTree.Expression value)
		{
			output.Add("Math.floor(");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateListConcat(List<string> output, ParseTree.Expression listA, ParseTree.Expression listB)
		{
			this.Translator.TranslateExpression(output, listA);
			output.Add(".concat(");
			this.Translator.TranslateExpression(output, listB);
			output.Add(")");
		}

		protected override void TranslateListGet(List<string> output, ParseTree.Expression list, ParseTree.Expression index)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add("[");
			this.Translator.TranslateExpression(output, index);
			output.Add("]");
		}

		protected override void TranslateListInsert(List<string> output, ParseTree.Expression list, ParseTree.Expression index, ParseTree.Expression value)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add(".splice(");
			this.Translator.TranslateExpression(output, index);
			output.Add(this.Shorten(", 0, "));
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateListJoin(List<string> output, ParseTree.Expression list, ParseTree.Expression sep)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add(".join(");
			this.Translator.TranslateExpression(output, sep);
			output.Add(")");
		}

		protected override void TranslateListLastIndex(List<string> output, ParseTree.Expression list)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add(this.Shorten(".length - 1"));
		}

		protected override void TranslateListLength(List<string> output, ParseTree.Expression list)
		{
			output.Add("(");
			this.Translator.TranslateExpression(output, list);
			output.Add(").length");
		}

		protected override void TranslateListNew(List<string> output, ParseTree.Expression length)
		{
			output.Add("create_list_of_size(");
			this.Translator.TranslateExpression(output, length);
			output.Add(")");
		}

		protected override void TranslateListPop(List<string> output, ParseTree.Expression list)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add(".pop()");
		}

		protected override void TranslateListPush(List<string> output, ParseTree.Expression list, ParseTree.Expression value)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add(".push(");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateListRemoveAt(List<string> output, ParseTree.Expression list, ParseTree.Expression index)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add(".splice(");
			this.Translator.TranslateExpression(output, index);
			output.Add(this.Shorten(", 1)"));
		}

		protected override void TranslateListReverseInPlace(List<string> output, ParseTree.Expression listVar)
		{
			this.Translator.TranslateExpression(output, listVar);
			output.Add(".reverse()");
		}

		protected override void TranslateListSet(List<string> output, ParseTree.Expression list, ParseTree.Expression index, ParseTree.Expression value)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add("[");
			this.Translator.TranslateExpression(output, index);
			output.Add(this.Shorten("] = "));
			this.Translator.TranslateExpression(output, value);
		}

		protected override void TranslateListShuffleInPlace(List<string> output, ParseTree.Expression list)
		{
			output.Add("shuffle(");
			this.Translator.TranslateExpression(output, list);
			output.Add(")");
		}

		protected override void TranslatePauseForFrame(List<string> output)
		{
			throw new Exception("This should have been optimized out.");
		}
		protected override void TranslatePrint(List<string> output, ParseTree.Expression message)
		{
			output.Add("window.alert(");
			this.Translator.TranslateExpression(output, message);
			output.Add(")");
		}

		protected override void TranslateRegisterTicker(List<string> output)
		{
			// Nope
		}

		protected override void TranslateRegisterTimeout(List<string> output)
		{
			output.Add("R.endFrame();");
			output.Add("window.setTimeout(v_runTick, R.computeDelayMillis())");
		}

		protected override void TranslateStringCast(List<string> output, ParseTree.Expression thing, bool strongCast)
		{
			if (strongCast)
			{
				output.Add(this.Shorten("('' + "));
				this.Translator.TranslateExpression(output, thing);
				output.Add(")");
			}
			else
			{
				this.Translator.TranslateExpression(output, thing);
			}
		}

		protected override void TranslateStringCharAt(List<string> output, ParseTree.Expression stringValue, ParseTree.Expression index)
		{
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(".charAt(");
			this.Translator.TranslateExpression(output, index);
			output.Add(")");
		}

		protected override void TranslateStringContains(List<string> output, ParseTree.Expression haystack, ParseTree.Expression needle)
		{
			output.Add("(");
			this.Translator.TranslateExpression(output, haystack);
			output.Add(".indexOf(");
			this.Translator.TranslateExpression(output, needle);
			output.Add(this.Shorten(") != -1)"));
		}

		protected override void TranslateStringEndsWith(List<string> output, ParseTree.Expression stringExpr, ParseTree.Expression findMe)
		{
			output.Add("stringEndsWith(");
			this.Translator.TranslateExpression(output, stringExpr);
			output.Add(this.Shorten(", "));
			this.Translator.TranslateExpression(output, findMe);
			output.Add(")");
		}

		protected override void TranslateStringFromCode(List<string> output, ParseTree.Expression characterCode)
		{
			output.Add("String.fromCharCode(");
			this.Translator.TranslateExpression(output, characterCode);
			output.Add(")");
		}

		protected override void TranslateStringIndexOf(List<string> output, ParseTree.Expression haystack, ParseTree.Expression needle)
		{
			this.Translator.TranslateExpression(output, haystack);
			output.Add(".indexOf(");
			this.Translator.TranslateExpression(output, needle);
			output.Add(")");
		}

		protected override void TranslateStringLength(List<string> output, ParseTree.Expression stringValue)
		{
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(".length");
		}

		protected override void TranslateStringLower(List<string> output, ParseTree.Expression stringValue)
		{
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(".toLowerCase()");
		}

		protected override void TranslateStringParseFloat(List<string> output, ParseTree.Expression stringValue)
		{
			output.Add("parseFloat(");
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(")");
		}

		protected override void TranslateStringParseInt(List<string> output, ParseTree.Expression value)
		{
			output.Add("parseInt(");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateStringReplace(List<string> output, ParseTree.Expression stringValue, ParseTree.Expression findMe, ParseTree.Expression replaceWith)
		{
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(".split(");
			this.Translator.TranslateExpression(output, findMe);
			output.Add(").join(");
			this.Translator.TranslateExpression(output, replaceWith);
			output.Add(")");
		}

		protected override void TranslateStringReverse(List<string> output, ParseTree.Expression stringValue)
		{
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(".split('').reverse().join('')");
		}

		protected override void TranslateStringSplit(List<string> output, ParseTree.Expression stringExpr, ParseTree.Expression sep)
		{
			this.Translator.TranslateExpression(output, stringExpr);
			output.Add(".split(");
			this.Translator.TranslateExpression(output, sep);
			output.Add(")");
		}

		protected override void TranslateStringStartsWith(List<string> output, ParseTree.Expression stringExpr, ParseTree.Expression findMe)
		{
			output.Add("(");
			this.Translator.TranslateExpression(output, stringExpr);
			output.Add(".indexOf(");
			this.Translator.TranslateExpression(output, findMe);
			output.Add(this.Shorten(") == 0)"));
		}

		protected override void TranslateStringTrim(List<string> output, ParseTree.Expression stringValue)
		{
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(".trim()");
		}

		protected override void TranslateStringUpper(List<string> output, ParseTree.Expression stringValue)
		{
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(".toUpperCase()");
		}

		protected override void TranslateUnregisterTicker(List<string> output)
		{
			// Nope.
		}

		protected override void TranslateUnsafeFloatDivision(List<string> output, ParseTree.Expression numerator, ParseTree.Expression denominator)
		{
			this.Translator.TranslateExpression(output, numerator);
			output.Add(this.Shorten(" / "));
			this.Translator.TranslateExpression(output, denominator);
		}

		protected override void TranslateUnsafeIntegerDivision(List<string> output, ParseTree.Expression numerator, ParseTree.Expression denominator)
		{
			output.Add("Math.floor(");
			this.Translator.TranslateExpression(output, numerator);
			output.Add(this.Shorten(" / "));
			this.Translator.TranslateExpression(output, denominator);
			output.Add(")");
		}
	}
}
