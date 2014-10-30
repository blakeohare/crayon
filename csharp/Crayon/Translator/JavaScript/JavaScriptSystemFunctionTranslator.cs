using System;
using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon.Translator.JavaScript
{
	internal class JavaScriptSystemFunctionTranslator : AbstractSystemFunctionTranslator
	{
		public JavaScriptSystemFunctionTranslator()
			: base()
		{ }

		protected override void TranslateArcCos(List<string> output, Expression value)
		{
			output.Add("Math.acos(");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateArcSin(List<string> output, Expression value)
		{
			output.Add("Math.asin(");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateArcTan(List<string> output, Expression dy, Expression dx)
		{
			output.Add("Math.atan2(");
			this.Translator.TranslateExpression(output, dy);
			output.Add(", ");
			this.Translator.TranslateExpression(output, dx);
			output.Add(")");
		}

		protected override void TranslateArrayGet(List<string> output, Expression list, Expression index)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add("[");
			this.Translator.TranslateExpression(output, index);
			output.Add("]");
		}

		protected override void TranslateArrayLength(List<string> output, Expression list)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add(".length");
		}

		protected override void TranslateArraySet(List<string> output, Expression list, Expression index, Expression value)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add("[");
			this.Translator.TranslateExpression(output, index);
			output.Add(this.Shorten("] = "));
			this.Translator.TranslateExpression(output, value);
		}

		protected override void TranslateBeginFrame(List<string> output)
		{
			output.Add("R.beginFrame()");
		}

		protected override void TranslateCast(List<string> output, StringConstant typeValue, Expression expression)
		{
			this.Translator.TranslateExpression(output, expression);
		}

		protected override void TranslateCastToList(List<string> output, Expression enumerableThing)
		{
			this.Translator.TranslateExpression(output, enumerableThing);
		}

		protected override void TranslateCharToString(List<string> output, Expression charValue)
		{
			this.Translator.TranslateExpression(output, charValue);
		}

		protected override void TranslateComment(List<string> output, StringConstant commentValue)
		{
#if DEBUG
			if (!this.IsMin)
			{
				output.Add("// " + commentValue.Value);
			}
#endif
		}

		protected override void TranslateConvertListToArray(List<string> output, StringConstant type, Expression list)
		{
			this.Translator.TranslateExpression(output, list);
		}

		protected override void TranslateCos(List<string> output, Expression value)
		{
			output.Add("Math.cos(");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateCurrentTimeSeconds(List<string> output)
		{
			output.Add("R.now()");
		}

		protected override void TranslateDictionaryContains(List<string> output, Expression dictionary, Expression key)
		{
			output.Add("(");
			this.Translator.TranslateExpression(output, dictionary);
			output.Add("[");
			this.Translator.TranslateExpression(output, key);
			output.Add(this.Shorten("] !== undefined)"));
		}

		protected override void TranslateDictionaryGetGuaranteed(List<string> output, Expression dictionary, Expression key)
		{
			this.Translator.TranslateExpression(output, dictionary);
			output.Add("[");
			this.Translator.TranslateExpression(output, key);
			output.Add("]");
		}

		protected override void TranslateDictionaryGetKeys(List<string> output, Expression dictionary)
		{
			output.Add("slow_dictionary_get_keys(");
			this.Translator.TranslateExpression(output, dictionary);
			output.Add(")");
		}

		protected override void TranslateDictionaryGetValues(List<string> output, Expression dictionary)
		{
			output.Add("slow_dictionary_get_values(");
			this.Translator.TranslateExpression(output, dictionary);
			output.Add(")");
		}

		protected override void TranslateDictionaryRemove(List<string> output, Expression dictionary, Expression key)
		{
			output.Add("delete ");
			this.Translator.TranslateExpression(output, dictionary);
			output.Add("[");
			this.Translator.TranslateExpression(output, key);
			output.Add("]");
		}

		protected override void TranslateDictionarySet(List<string> output, Expression dict, Expression key, Expression value)
		{
			this.Translator.TranslateExpression(output, dict);
			output.Add("[");
			this.Translator.TranslateExpression(output, key);
			output.Add(this.Shorten("] = "));
			this.Translator.TranslateExpression(output, value);
		}

		protected override void TranslateDictionarySize(List<string> output, Expression dictionary)
		{
			output.Add("Object.keys(");
			this.Translator.TranslateExpression(output, dictionary);
			output.Add(").length");
		}

		protected override void TranslateDotEquals(List<string> output, Expression root, Expression compareTo)
		{
			throw new Exception("This should have been optimized out.");
		}

		protected override void TranslateExponent(List<string> output, Expression baseNum, Expression powerNum)
		{
			output.Add("Math.pow(");
			this.Translator.TranslateExpression(output, baseNum);
			output.Add(this.Shorten(", "));
			this.Translator.TranslateExpression(output, powerNum);
			output.Add(")");
		}

		protected override void TranslateForceParens(List<string> output, Expression expression)
		{
			output.Add("(");
			this.Translator.TranslateExpression(output, expression);
			output.Add(")");
		}

		protected override void TranslateGetProgramData(List<string> output)
		{
			output.Add("R.ProgramData");
		}

		protected override void TranslateGetRawByteCodeString(List<string> output, string theString)
		{
			output.Add("\"");
			output.Add(theString);
			output.Add("\"");
		}

		protected override void TranslateInsertFrameworkCode(string tab, List<string> output, string id)
		{
			switch (id)
			{
				case "ff_blit_image":
					output.Add(this.Shorten("R.blit(v_arg1[1][1], v_arg2[1], v_arg3[1])"));
					break;

				case "ff_download_image":
					output.Add(this.Shorten("R.enqueue_image_download(v_arg1[1], v_arg2[1])"));
					break;

				case "ff_draw_ellipse":
					output.Add(this.Shorten("R.drawEllipse(v_arg1[1] + v_arg3[1] / 2, v_arg2[1] + v_arg4[1] / 2, v_arg3[1] / 2, v_arg4[1] / 2, v_arg5[1], v_arg6[1], v_arg7[1])"));
					break;

				case "ff_draw_line":
					output.Add(this.Shorten("R.drawLine(v_arg1[1], v_arg2[1], v_arg3[1], v_arg4[1], v_arg5[1], v_arg6[1], v_arg7[1], v_arg8[1])"));
					break;

				case "ff_draw_rectangle":
					output.Add(this.Shorten("R.drawRect(v_arg1[1], v_arg2[1], v_arg3[1], v_arg4[1], v_arg5[1], v_arg6[1], v_arg7[1])"));
					break;

				case "ff_fill_screen":
					output.Add(this.Shorten("R.fillScreen(v_arg1[1], v_arg2[1], v_arg3[1])"));
					break;

				case "ff_flip_image":
					output.Add("v_output = R.flipImage(v_arg1[1], v_arg2[1], v_arg3[1])");
					break;

				case "ff_floor":
					output.Add(this.Shorten("v_output = [" + (int)Types.INTEGER + ", Math.floor(v_arg1[1])]"));
					break;

				case "ff_get_events":
					output.Add(this.Shorten("v_output = [" + (int)Types.LIST + ", R.pump_event_objects()]"));
					break;

				case "ff_get_image":
					output.Add(this.Shorten("v_output = R.get_image_impl(v_arg1[1])"));
					break;

				case "ff_get_image_height":
					output.Add(this.Shorten("v_output = v_build_integer(v_arg1[1][1].height)"));
					break;

				case "ff_get_image_width":
					output.Add(this.Shorten("v_output = v_build_integer(v_arg1[1][1].width)"));
					break;

				case "ff_initialize_game":
					output.Add("R.initializeGame(v_arg1[1])");
					break;

				case "ff_initialize_screen":
					output.Add(this.Shorten("R.initializeScreen(v_arg1[1], v_arg2[1], null, null)"));
					break;

				case "ff_initialize_screen_scaled":
					output.Add(this.Shorten("R.initializeScreen(v_arg1[1], v_arg2[1], v_arg3[1], v_arg4[1])"));
					break;

				case "ff_is_image_loaded":
					output.Add(this.Shorten("v_output = R.is_image_loaded(v_arg1[1]) ? v_VALUE_TRUE : v_VALUE_FALSE"));
					break;

				case "ff_parse_int":
					// TODO: need to throw if not an integer
					output.Add(this.Shorten("v_output = [" + (int)Types.INTEGER + ", parseInt(v_arg1[1])]"));
					break;

				case "ff_set_title":
					output.Add("TODO('set title');");
					break;

				default:
					throw new NotImplementedException();
			}
		}

		protected override void TranslateInt(List<string> output, Expression value)
		{
			output.Add("Math.floor(");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateListClear(List<string> output, Expression list)
		{
			throw new Exception("This should have been optimized out.");
		}

		protected override void TranslateListConcat(List<string> output, Expression listA, Expression listB)
		{
			this.Translator.TranslateExpression(output, listA);
			output.Add(".concat(");
			this.Translator.TranslateExpression(output, listB);
			output.Add(")");
		}

		protected override void TranslateListGet(List<string> output, Expression list, Expression index)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add("[");
			this.Translator.TranslateExpression(output, index);
			output.Add("]");
		}

		protected override void TranslateListInsert(List<string> output, Expression list, Expression index, Expression value)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add(".splice(");
			this.Translator.TranslateExpression(output, index);
			output.Add(this.Shorten(", 0, "));
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateListJoin(List<string> output, Expression list, Expression sep)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add(".join(");
			this.Translator.TranslateExpression(output, sep);
			output.Add(")");
		}

		protected override void TranslateListJoinChars(List<string> output, Expression list)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add(".join('')");
		}

		protected override void TranslateListLastIndex(List<string> output, Expression list)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add(this.Shorten(".length - 1"));
		}

		protected override void TranslateListLength(List<string> output, Expression list)
		{
			output.Add("(");
			this.Translator.TranslateExpression(output, list);
			output.Add(").length");
		}

		protected override void TranslateListPop(List<string> output, Expression list)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add(".pop()");
		}

		protected override void TranslateListPush(List<string> output, Expression list, Expression value)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add(".push(");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateListRemoveAt(List<string> output, Expression list, Expression index)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add(".splice(");
			this.Translator.TranslateExpression(output, index);
			output.Add(this.Shorten(", 1)"));
		}

		protected override void TranslateListReverseInPlace(List<string> output, Expression list)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add(".reverse()");
		}

		protected override void TranslateListSet(List<string> output, Expression list, Expression index, Expression value)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add("[");
			this.Translator.TranslateExpression(output, index);
			output.Add(this.Shorten("] = "));
			this.Translator.TranslateExpression(output, value);
		}

		protected override void TranslateListShuffleInPlace(List<string> output, Expression list)
		{
			output.Add("shuffle(");
			this.Translator.TranslateExpression(output, list);
			output.Add(")");
		}

		protected override void TranslateMultiplyList(List<string> output, Expression list, Expression num)
		{
			output.Add("multiply_list(");
			this.Translator.TranslateExpression(output, list);
			output.Add(this.Shorten(", "));
			this.Translator.TranslateExpression(output, num);
			output.Add(")");
		}

		protected override void TranslateNewArray(List<string> output, StringConstant type, Expression size)
		{
			output.Add("create_new_array(");
			this.Translator.TranslateExpression(output, size);
			output.Add(")");
		}

		protected override void TranslateNewDictionary(List<string> output, StringConstant keyType, StringConstant valueType)
		{
			output.Add("{}");
		}

		protected override void TranslateNewList(List<string> output, StringConstant type)
		{
			output.Add("[]");
		}

		protected override void TranslateNewListOfSize(List<string> output, StringConstant type, Expression length)
		{
			output.Add("create_list_of_size(");
			this.Translator.TranslateExpression(output, length);
			output.Add(")");
		}

		protected override void TranslateNewStack(List<string> output, StringConstant type)
		{
			output.Add("[]");
		}

		protected override void TranslatePauseForFrame(List<string> output)
		{
			throw new Exception("This should have been optimized out.");
		}

		protected override void TranslatePrint(List<string> output, Expression message)
		{
			output.Add("R.print(");
			this.Translator.TranslateExpression(output, message);
			output.Add(")");
		}

		protected override void TranslateRandomFloat(List<string> output)
		{
			output.Add("Math.random()");
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

		protected override void TranslateSetProgramData(List<string> output, Expression programData)
		{
			output.Add("R.ProgramData = ");
			this.Translator.TranslateExpression(output, programData);
		}

		protected override void TranslateSin(List<string> output, Expression value)
		{
			output.Add("Math.sin(");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateStackGet(List<string> output, Expression stack, Expression index)
		{
			this.Translator.TranslateExpression(output, stack);
			output.Add("[");
			this.Translator.TranslateExpression(output, index);
			output.Add("]");
		}

		protected override void TranslateStackLength(List<string> output, Expression stack)
		{
			this.Translator.TranslateExpression(output, stack);
			output.Add(".length");
		}

		protected override void TranslateStackPop(List<string> output, Expression list)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add(".pop()");
		}

		protected override void TranslateStackPush(List<string> output, Expression stack, Expression value)
		{
			this.Translator.TranslateExpression(output, stack);
			output.Add(".push(");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateStackSet(List<string> output, Expression stack, Expression index, Expression value)
		{
			this.Translator.TranslateExpression(output, stack);
			output.Add("[");
			this.Translator.TranslateExpression(output, index);
			output.Add(this.Shorten("] = "));
			this.Translator.TranslateExpression(output, value);
		}

		protected override void TranslateStringAsChar(List<string> output, StringConstant stringConstant)
		{
			this.Translator.TranslateExpression(output, stringConstant);
		}

		protected override void TranslateStringCast(List<string> output, Expression thing, bool strongCast)
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

		protected override void TranslateStringCharAt(List<string> output, Expression stringValue, Expression index)
		{
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(".charAt(");
			this.Translator.TranslateExpression(output, index);
			output.Add(")");
		}

		protected override void TranslateStringContains(List<string> output, Expression haystack, Expression needle)
		{
			output.Add("(");
			this.Translator.TranslateExpression(output, haystack);
			output.Add(".indexOf(");
			this.Translator.TranslateExpression(output, needle);
			output.Add(this.Shorten(") != -1)"));
		}

		protected override void TranslateStringEndsWith(List<string> output, Expression stringExpr, Expression findMe)
		{
			output.Add("stringEndsWith(");
			this.Translator.TranslateExpression(output, stringExpr);
			output.Add(this.Shorten(", "));
			this.Translator.TranslateExpression(output, findMe);
			output.Add(")");
		}

		protected override void TranslateStringFromCode(List<string> output, Expression characterCode)
		{
			output.Add("String.fromCharCode(");
			this.Translator.TranslateExpression(output, characterCode);
			output.Add(")");
		}

		protected override void TranslateStringIndexOf(List<string> output, Expression haystack, Expression needle)
		{
			this.Translator.TranslateExpression(output, haystack);
			output.Add(".indexOf(");
			this.Translator.TranslateExpression(output, needle);
			output.Add(")");
		}

		protected override void TranslateStringLength(List<string> output, Expression stringValue)
		{
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(".length");
		}

		protected override void TranslateStringLower(List<string> output, Expression stringValue)
		{
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(".toLowerCase()");
		}

		protected override void TranslateStringParseFloat(List<string> output, Expression stringValue)
		{
			output.Add("parseFloat(");
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(")");
		}

		protected override void TranslateStringParseInt(List<string> output, Expression value)
		{
			output.Add("parseInt(");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateStringReplace(List<string> output, Expression stringValue, Expression findMe, Expression replaceWith)
		{
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(".split(");
			this.Translator.TranslateExpression(output, findMe);
			output.Add(").join(");
			this.Translator.TranslateExpression(output, replaceWith);
			output.Add(")");
		}

		protected override void TranslateStringReverse(List<string> output, Expression stringValue)
		{
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(".split('').reverse().join('')");
		}

		protected override void TranslateStringSplit(List<string> output, Expression stringExpr, Expression sep)
		{
			this.Translator.TranslateExpression(output, stringExpr);
			output.Add(".split(");
			this.Translator.TranslateExpression(output, sep);
			output.Add(")");
		}

		protected override void TranslateStringStartsWith(List<string> output, Expression stringExpr, Expression findMe)
		{
			output.Add("(");
			this.Translator.TranslateExpression(output, stringExpr);
			output.Add(".indexOf(");
			this.Translator.TranslateExpression(output, findMe);
			output.Add(this.Shorten(") == 0)"));
		}

		protected override void TranslateStringTrim(List<string> output, Expression stringValue)
		{
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(".trim()");
		}

		protected override void TranslateStringUpper(List<string> output, Expression stringValue)
		{
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(".toUpperCase()");
		}

		protected override void TranslateTan(List<string> output, Expression value)
		{
			output.Add("Math.tan(");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateUnregisterTicker(List<string> output)
		{
			// Nope.
		}

		protected override void TranslateUnsafeFloatDivision(List<string> output, Expression numerator, Expression denominator)
		{
			this.Translator.TranslateExpression(output, numerator);
			output.Add(this.Shorten(" / "));
			this.Translator.TranslateExpression(output, denominator);
		}

		protected override void TranslateUnsafeIntegerDivision(List<string> output, Expression numerator, Expression denominator)
		{
			output.Add("Math.floor(");
			this.Translator.TranslateExpression(output, numerator);
			output.Add(this.Shorten(" / "));
			this.Translator.TranslateExpression(output, denominator);
			output.Add(")");
		}
	}
}
