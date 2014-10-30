using System;
using System.Collections.Generic;
using System.Linq;
using Crayon.ParseTree;
using System.Text;

namespace Crayon.Translator.CSharp
{
	class CSharpSystemFunctionTranslator : AbstractSystemFunctionTranslator
	{
		public CSharpSystemFunctionTranslator() : base() { }

		protected override void TranslateArcCos(List<string> output, Expression value)
		{
			output.Add("Math.Acos(");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateArcSin(List<string> output, Expression value)
		{
			output.Add("Math.Asin(");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateArcTan(List<string> output, Expression dy, Expression dx)
		{
			output.Add("Math.Atan2(");
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
			output.Add(".Length");
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
			// Nope
		}

		protected override void TranslateCast(List<string> output, StringConstant typeValue, Expression expression)
		{
			CSharpPlatform platform = (CSharpPlatform)this.Platform;
			string typeString = platform.GetTypeStringFromAnnotation(typeValue.FirstToken, ((StringConstant)typeValue).Value);

			output.Add("(");
			output.Add(typeString);
			output.Add(")");
			this.Translator.TranslateExpression(output, expression);
		}

		protected override void TranslateCastToList(List<string> output, Expression enumerableThing)
		{
			output.Add("new List<Value>(");
			this.Translator.TranslateExpression(output, enumerableThing);
			output.Add(")");
		}

		protected override void TranslateCharToString(List<string> output, Expression charValue)
		{
			output.Add("\"\" + ");
			this.Translator.TranslateExpression(output, charValue);
		}

		protected override void TranslateComment(List<string> output, StringConstant commentValue)
		{
#if DEBUG
			output.Add("// " + commentValue.Value);
#endif
		}

		protected override void TranslateConvertListToArray(List<string> output, StringConstant type, Expression list)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add(".ToArray()");
		}

		protected override void TranslateCos(List<string> output, Expression value)
		{
			output.Add("Math.Cos(");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateCurrentTimeSeconds(List<string> output)
		{
			output.Add("DateTime.Now.Ticks / 10000000.0");
		}

		protected override void TranslateDictionaryContains(List<string> output, Expression dictionary, Expression key)
		{
			this.Translator.TranslateExpression(output, dictionary);
			output.Add(".ContainsKey(");
			this.Translator.TranslateExpression(output, key);
			output.Add(")");
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
			this.Translator.TranslateExpression(output, dictionary);
			output.Add(".Keys.ToArray()");
		}

		protected override void TranslateDictionaryGetValues(List<string> output, Expression dictionary)
		{
			this.Translator.TranslateExpression(output, dictionary);
			output.Add(".Values");
		}

		protected override void TranslateDictionaryRemove(List<string> output, Expression dictionary, Expression key)
		{
			this.Translator.TranslateExpression(output, dictionary);
			output.Add(".Remove(");
			this.Translator.TranslateExpression(output, key);
			output.Add(")");
		}

		protected override void TranslateDictionarySet(List<string> output, Expression dict, Expression key, Expression value)
		{
			this.Translator.TranslateExpression(output, dict);
			output.Add("[");
			this.Translator.TranslateExpression(output, key);
			output.Add("] = ");
			this.Translator.TranslateExpression(output, value);
		}

		protected override void TranslateDictionarySize(List<string> output, Expression dictionary)
		{
			this.Translator.TranslateExpression(output, dictionary);
			output.Add(".Count");
		}

		protected override void TranslateDotEquals(List<string> output, Expression root, Expression compareTo)
		{
			this.Translator.TranslateExpression(output, root);
			output.Add(".Equals(");
			this.Translator.TranslateExpression(output, compareTo);
			output.Add(")");
		}

		protected override void TranslateExponent(List<string> output, Expression baseNum, Expression powerNum)
		{
			output.Add("System.Math.Pow(");
			this.Translator.TranslateExpression(output, baseNum);
			output.Add(", ");
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
			output.Add("TranslationHelper.ProgramData");
		}

		protected override void TranslateGetRawByteCodeString(List<string> output, string theString)
		{
			output.Add("ResourceReader.ReadByteCodeFile()");
		}

		protected override void TranslateInsertFrameworkCode(string tab, List<string> output, string id)
		{
			switch (id)
			{
				case "ff_blit_image":
					output.Add("GameWindow.BlitImage((Image)v_arg1.internalValue, (int)v_arg2.internalValue, (int)v_arg3.internalValue)");
					break;

				case "ff_download_image":
					output.Add("Image.LoadImage((string)v_arg1.internalValue, (string)v_arg2.internalValue)");
					break;

				case "ff_draw_ellipse":
					output.Add("// TODO: draw ellipse");
					break;

				case "ff_draw_line":
					output.Add("// TODO: draw line");
					break;

				case "ff_draw_rectangle":
					output.Add("GameWindow.DrawRectangle(");
					for (int i = 1; i <= 8; ++i)
					{
						if (i > 1) output.Add(", ");
						output.Add("(int)v_arg" + i + ".internalValue");
					}
					output.Add(")");
					break;

				case "ff_fill_screen":
					output.Add("// TODO: fill screen");
					break;

				case "ff_flip_image":
					output.Add("// TODO: flip image");
					break;

				case "ff_floor":
					string nl = this.Translator.NL;
					string tab2 = tab + "\t";
					// TODO: This is silly. returning arg1 when it's an int should be written directly in the interpreter crayon code
					output.AddRange(new string[] {
						"if (v_arg1.type == " + (int)Types.INTEGER + ")", nl,
						tab, "{",nl,
						tab2, "v_output = v_arg1;", nl,
						tab, "}", nl,
						tab, "else", nl,
						tab, "{", nl,
						tab2, "v_output = v_build_integer((int)System.Math.Floor((double)v_arg1.internalValue));", nl,
						tab, "}"
					});
					break;

				case "ff_get_events":
					output.Add("v_output = new Value(" + (int)Types.LIST + ", GameWindow.GetEvents())");
					break;

				case "ff_get_image":
					output.Add("v_output = new Value(" + (int)Types.NATIVE_OBJECT + ", Image.GetImageByKey((string)v_arg1.internalValue))");
					break;

				case "ff_get_image_height":
					output.Add("v_output = v_build_integer(((Image)v_arg1.internalValue).Height)");
					break;

				case "ff_get_image_width":
					output.Add("v_output = v_build_integer(((Image)v_arg1.internalValue).Width)");
					break;

				case "ff_initialize_game":
					output.Add("GameWindow.FPS = v_arg1.type == " + (int)Types.INTEGER + " ? (double)(int)v_arg1.internalValue : (double)v_arg1.internalValue");
					break;

				case "ff_initialize_screen":
					output.Add("v_yieldControl(v_stack);");
					output.Add(this.Translator.NL);
					output.Add(tab);
					output.Add("GameWindow.InitializeScreen((int)v_arg1.internalValue, (int)v_arg2.internalValue);");
					output.Add(this.Translator.NL);
					output.Add(tab);
					output.Add("return \"\"");
					break;

				case "ff_initialize_screen_scaled":
					output.Add("v_yieldControl(v_stack);");
					output.Add(this.Translator.NL);
					output.Add(tab);
					output.Add("GameWindow.InitializeScreen((int)v_arg1.internalValue, (int)v_arg2.internalValue, (int)v_arg3.internalValue, (int)v_arg4.internalValue);");
					output.Add(this.Translator.NL);
					output.Add(tab);
					output.Add("return \"\"");
					break;

				case "ff_is_image_loaded":
					output.Add("v_output = v_VALUE_TRUE");
					break;

				case "ff_parse_int":
					output.Add("v_output = TranslationHelper.ParseInt((string)v_arg1.internalValue)");
					break;

				case "ff_set_title":
					output.Add("// TODO: set title");
					break;

				default:
					throw new NotImplementedException("No framework code available: " + id);
			}
		}

		protected override void TranslateInt(List<string> output, Expression value)
		{
			output.Add("((int)");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateListClear(List<string> output, Expression list)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add(".Clear()");
		}

		protected override void TranslateListConcat(List<string> output, Expression listA, Expression listB)
		{
			output.Add("TranslationHelper.ValueListConcat(");
			this.Translator.TranslateExpression(output, listA);
			output.Add(", ");
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
			output.Add(".Insert(");
			this.Translator.TranslateExpression(output, index);
			output.Add(", ");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateListJoin(List<string> output, Expression list, Expression sep)
		{
			output.Add("string.Join(");
			this.Translator.TranslateExpression(output, sep);
			output.Add(", ");
			this.Translator.TranslateExpression(output, list);
			output.Add(")");
		}

		protected override void TranslateListLastIndex(List<string> output, Expression list)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add(".Count - 1");
		}

		protected override void TranslateListLength(List<string> output, Expression list)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add(".Count");
		}

		protected override void TranslateListPop(List<string> output, Expression list)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add(".RemoveAt(");
			this.Translator.TranslateExpression(output, list);
			output.Add(".Count - 1)");
		}

		protected override void TranslateListPush(List<string> output, Expression list, Expression value)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add(".Add(");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateListRemoveAt(List<string> output, Expression list, Expression index)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add(".RemoveAt(");
			this.Translator.TranslateExpression(output, index);
			output.Add(")");
		}

		protected override void TranslateListReverseInPlace(List<string> output, Expression list)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add(".Reverse()");
		}

		protected override void TranslateListSet(List<string> output, Expression list, Expression index, Expression value)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add("[");
			this.Translator.TranslateExpression(output, index);
			output.Add("] = ");
			this.Translator.TranslateExpression(output, value);
		}

		protected override void TranslateListShuffleInPlace(List<string> output, Expression list)
		{
			output.Add("TranslationHelper.ShuffleInPlace(");
			this.Translator.TranslateExpression(output, list);
			output.Add(")");
		}

		protected override void TranslateMultiplyList(List<string> output, Expression list, Expression num)
		{
			output.Add("TranslationHelper.MultiplyList(");
			this.Translator.TranslateExpression(output, list);
			output.Add(", ");
			this.Translator.TranslateExpression(output, num);
			output.Add(")");
		}

		protected override void TranslateNewArray(List<string> output, StringConstant type, Expression size)
		{
			CSharpPlatform platform = (CSharpPlatform)this.Platform;
			string csharpType = platform.GetTypeStringFromAnnotation(type.FirstToken, type.Value);
			output.Add("new ");
			// Delightful hack...
			int padding = 0;
			while (csharpType.EndsWith("[]"))
			{
				padding++;
				csharpType = csharpType.Substring(0, csharpType.Length - 2);
			}
			output.Add(csharpType);
			output.Add("[");
			this.Translator.TranslateExpression(output, size);
			output.Add("]");
			while (padding-- > 0)
			{
				output.Add("[]");
			}
		}

		protected override void TranslateNewDictionary(List<string> output, StringConstant keyType, StringConstant valueType)
		{
			CSharpPlatform platform = (CSharpPlatform)this.Platform;
			string csharpKeyType = platform.GetTypeStringFromAnnotation(keyType.FirstToken, keyType.Value);
			string csharpValueType = platform.GetTypeStringFromAnnotation(valueType.FirstToken, valueType.Value);
			output.Add("new Dictionary<");
			output.Add(csharpKeyType);
			output.Add(", ");
			output.Add(csharpValueType);
			output.Add(">()");
		}

		protected override void TranslateNewList(List<string> output, StringConstant type)
		{
			CSharpPlatform platform = (CSharpPlatform)this.Platform;
			string csharpType = platform.GetTypeStringFromAnnotation(type.FirstToken, ((StringConstant)type).Value);
			output.Add("new List<");
			output.Add(csharpType);
			output.Add(">()");
		}

		protected override void TranslateNewListOfSize(List<string> output, StringConstant type, Expression length)
		{
			CSharpPlatform platform = (CSharpPlatform)this.Platform;
			output.Add("TranslationHelper.NewListOfSize<");
			output.Add(platform.GetTypeStringFromAnnotation(type.FirstToken, type.Value));
			output.Add(">(");
			this.Translator.TranslateExpression(output, length);
			output.Add(")");
		}

		protected override void TranslateNewStack(List<string> output, StringConstant type)
		{
			CSharpPlatform platform = (CSharpPlatform)this.Platform;
			string csharpType = platform.GetTypeStringFromAnnotation(type.FirstToken, ((StringConstant)type).Value);
			output.Add("new CrStack<");
			output.Add(csharpType);
			output.Add(">()");
		}

		protected override void TranslatePauseForFrame(List<string> output)
		{
			// Nope
		}

		protected override void TranslatePrint(List<string> output, Expression message)
		{
			output.Add("System.Console.WriteLine(");
			this.Translator.TranslateExpression(output, message);
			output.Add(")");
		}

		protected override void TranslateRandomFloat(List<string> output)
		{
			output.Add("TranslationHelper.GetRandomNumber()");
		}

		protected override void TranslateRegisterTicker(List<string> output)
		{
			// Nope
		}

		protected override void TranslateRegisterTimeout(List<string> output)
		{
			// Nope
		}

		protected override void TranslateSetProgramData(List<string> output, Expression programData)
		{
			output.Add("TranslationHelper.ProgramData = ");
			this.Translator.TranslateExpression(output, programData);
		}

		protected override void TranslateSin(List<string> output, Expression value)
		{
			output.Add("Math.Sin(");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateStackGet(List<string> output, Expression stack, Expression index)
		{
			this.Translator.TranslateExpression(output, stack);
			output.Add(".Items[");
			this.Translator.TranslateExpression(output, index);
			output.Add("]");
		}

		protected override void TranslateStackLength(List<string> output, Expression stack)
		{
			this.Translator.TranslateExpression(output, stack);
			output.Add(".Size");
		}

		protected override void TranslateStackPop(List<string> output, Expression stack)
		{
			this.Translator.TranslateExpression(output, stack);
			output.Add(".Pop()");
		}

		protected override void TranslateStackPush(List<string> output, Expression stack, Expression value)
		{
			this.Translator.TranslateExpression(output, stack);
			output.Add(".Push(");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateStackSet(List<string> output, Expression stack, Expression index, Expression value)
		{
			this.Translator.TranslateExpression(output, stack);
			output.Add(".Items[");
			this.Translator.TranslateExpression(output, index);
			output.Add("] = ");
			this.Translator.TranslateExpression(output, value);
		}

		protected override void TranslateStringAsChar(List<string> output, StringConstant stringConstant)
		{
			char c = stringConstant.Value[0];
			string value;
			switch (c)
			{
				case '\'': value = "'\\''"; break;
				case '\\': value = "'\\\\'"; break;
				case '\n': value = "'\\n'"; break;
				case '\r': value = "'\\r'"; break;
				case '\0': value = "'\\0'"; break;
				case '\t': value = "'\\t'"; break;
				default: value = "'" + c + "'"; break;
			}
			output.Add(value);
		}

		protected override void TranslateStringCast(List<string> output, Expression thing, bool strongCast)
		{
			if (strongCast)
			{
				output.Add("(");
				this.Translator.TranslateExpression(output, thing);
				output.Add(").ToString()");
			}
			else
			{
				this.Translator.TranslateExpression(output, thing);
			}
		}

		protected override void TranslateStringCharAt(List<string> output, Expression stringValue, Expression index)
		{
			this.Translator.TranslateExpression(output, stringValue);
			output.Add("[");
			this.Translator.TranslateExpression(output, index);
			output.Add("]");
		}

		protected override void TranslateStringContains(List<string> output, Expression haystack, Expression needle)
		{
			this.Translator.TranslateExpression(output, haystack);
			output.Add(".Contains(");
			this.Translator.TranslateExpression(output, needle);
			output.Add(")");
		}

		protected override void TranslateStringEndsWith(List<string> output, Expression stringExpr, Expression findMe)
		{
			this.Translator.TranslateExpression(output, stringExpr);
			output.Add(".EndsWith(");
			this.Translator.TranslateExpression(output, findMe);
			output.Add(")");
		}

		protected override void TranslateStringFromCode(List<string> output, Expression characterCode)
		{
			output.Add("(\"\" + (char)");
			this.Translator.TranslateExpression(output, characterCode);
			output.Add(")");
		}

		protected override void TranslateStringIndexOf(List<string> output, Expression haystack, Expression needle)
		{
			this.Translator.TranslateExpression(output, haystack);
			output.Add(".IndexOf(");
			this.Translator.TranslateExpression(output, needle);
			output.Add(")");
		}

		protected override void TranslateStringLength(List<string> output, Expression stringValue)
		{
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(".Length");
		}

		protected override void TranslateStringLower(List<string> output, Expression stringValue)
		{
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(".ToLowerInvariant()");
		}

		protected override void TranslateStringParseFloat(List<string> output, Expression stringValue)
		{
			output.Add("double.Parse(");
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(")");
		}

		protected override void TranslateStringParseInt(List<string> output, Expression value)
		{
			output.Add("int.Parse(");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateStringReplace(List<string> output, Expression stringValue, Expression findMe, Expression replaceWith)
		{
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(".Replace(");
			this.Translator.TranslateExpression(output, findMe);
			output.Add(", ");
			this.Translator.TranslateExpression(output, replaceWith);
			output.Add(")");
		}

		protected override void TranslateStringReverse(List<string> output, Expression stringValue)
		{
			output.Add("TranslationHelper.StringReverse(");
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(")");
		}

		protected override void TranslateStringSplit(List<string> output, Expression stringExpr, Expression sep)
		{
			output.Add("TranslationHelper.StringSplit(");
			this.Translator.TranslateExpression(output, stringExpr);
			output.Add(", ");
			this.Translator.TranslateExpression(output, sep);
			output.Add(")");
		}

		protected override void TranslateStringStartsWith(List<string> output, Expression stringExpr, Expression findMe)
		{
			this.Translator.TranslateExpression(output, stringExpr);
			output.Add(".StartsWith(");
			this.Translator.TranslateExpression(output, findMe);
			output.Add(")");
		}

		protected override void TranslateStringTrim(List<string> output, Expression stringValue)
		{
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(".Trim()");
		}

		protected override void TranslateStringUpper(List<string> output, Expression stringValue)
		{
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(".ToUpperInvariant()");
		}

		protected override void TranslateTan(List<string> output, Expression value)
		{
			output.Add("Math.Tan(");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateUnregisterTicker(List<string> output)
		{
			// Nope
		}

		protected override void TranslateUnsafeFloatDivision(List<string> output, Expression numerator, Expression denominator)
		{
			this.Translator.TranslateExpression(output, numerator);
			output.Add(" / ");
			this.Translator.TranslateExpression(output, denominator);
		}

		protected override void TranslateUnsafeIntegerDivision(List<string> output, Expression numerator, Expression denominator)
		{
			this.Translator.TranslateExpression(output, numerator);
			output.Add(" / ");
			this.Translator.TranslateExpression(output, denominator);
		}
	}
}
