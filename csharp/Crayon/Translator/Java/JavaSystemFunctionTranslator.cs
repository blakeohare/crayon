using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crayon.ParseTree;

namespace Crayon.Translator.Java
{
	class JavaSystemFunctionTranslator : AbstractSystemFunctionTranslator
	{
		public JavaPlatform JavaPlatform { get { return (JavaPlatform)this.Platform; } }

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
			output.Add("] = ");
			this.Translator.TranslateExpression(output, value);
		}

		protected override void TranslateBeginFrame(List<string> output)
		{
			// Nope
		}

		protected override void TranslateCast(List<string> output, StringConstant typeValue, Expression expression)
		{
			this.Translator.TranslateExpression(output, expression);
		}

		protected override void TranslateCastToList(List<string> output, Expression enumerableThing)
		{
			output.Add("new ArrayList<Value>(");
			this.Translator.TranslateExpression(output, enumerableThing);
			output.Add(".asList())");
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
			output.Add(".toArray(new ");
			this.JavaPlatform.GetTypeStringFromString(type.Value, false);
			output.Add("[");
			this.Translator.TranslateExpression(output, list);
			output.Add(".size()])");
		}

		protected override void TranslateDictionaryContains(List<string> output, Expression dictionary, Expression key)
		{
			this.Translator.TranslateExpression(output, dictionary);
			output.Add(".contains(");
			this.Translator.TranslateExpression(output, key);
			output.Add(")");
		}

		protected override void TranslateDictionaryGetGuaranteed(List<string> output, Expression dictionary, Expression key)
		{
			this.Translator.TranslateExpression(output, dictionary);
			output.Add(".get(");
			this.Translator.TranslateExpression(output, key);
			output.Add(")");
		}

		protected override void TranslateDictionaryGetKeys(List<string> output, Expression dictionary)
		{
			this.Translator.TranslateExpression(output, dictionary);
			output.Add(".keySet()");
		}

		protected override void TranslateDictionaryGetValues(List<string> output, Expression dictionary)
		{
			this.Translator.TranslateExpression(output, dictionary);
			output.Add(".values()");
		}

		protected override void TranslateDictionaryRemove(List<string> output, Expression dictionary, Expression key)
		{
			this.Translator.TranslateExpression(output, dictionary);
			output.Add(".remove(");
			this.Translator.TranslateExpression(output, key);
			output.Add(")");
		}

		protected override void TranslateDictionarySet(List<string> output, Expression dictionary, Expression key, Expression value)
		{
			this.Translator.TranslateExpression(output, dictionary);
			output.Add(".put(");
			this.Translator.TranslateExpression(output, key);
			output.Add(", ");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateDictionarySize(List<string> output, Expression dictionary)
		{
			this.Translator.TranslateExpression(output, dictionary);
			output.Add(".size()");
		}

		protected override void TranslateDotEquals(List<string> output, Expression root, Expression compareTo)
		{
			this.Translator.TranslateExpression(output, root);
			output.Add(".equals(");
			this.Translator.TranslateExpression(output, compareTo);
			output.Add(")");
		}

		protected override void TranslateExponent(List<string> output, Expression baseNum, Expression powerNum)
		{
			output.Add("Math.pow(");
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
			output.Add("TranslationHelper.getProgramData()");
		}

		protected override void TranslateGetRawByteCodeString(List<string> output, string theString)
		{
			output.Add("TranslationHelper.getRawByteCodeString()");
		}

		protected override void TranslateInsertFrameworkCode(string tab, List<string> output, string id)
		{
			output.Add("TODO:(\"refactor the insert framework code stuff\"");
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
			output.Add(".clear()");
		}

		protected override void TranslateListConcat(List<string> output, Expression listA, Expression listB)
		{
			output.Add("TranslationHelper.concatLists(");
			this.Translator.TranslateExpression(output, listA);
			output.Add(", ");
			this.Translator.TranslateExpression(output, listB);
			output.Add(")");
		}

		protected override void TranslateListGet(List<string> output, Expression list, Expression index)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add(".get(");
			this.Translator.TranslateExpression(output, index);
			output.Add(")");
		}

		protected override void TranslateListInsert(List<string> output, Expression list, Expression index, Expression value)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add(".insert(");
			this.Translator.TranslateExpression(output, index);
			output.Add(", ");
			this.Translator.TranslateExpression(output, index);
			output.Add(")");	
		}

		protected override void TranslateListJoin(List<string> output, Expression list, Expression sep)
		{
			output.Add("TranslationHelper.joinList(");
			this.Translator.TranslateExpression(output, sep);
			output.Add(", ");
			this.Translator.TranslateExpression(output, list);
			output.Add(")");
		}

		protected override void TranslateListLastIndex(List<string> output, Expression list)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add(".get(");
			this.Translator.TranslateExpression(output, list);
			output.Add(".length - 1)");
		}

		protected override void TranslateListLength(List<string> output, Expression list)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add(".size()");
		}

		protected override void TranslateListPop(List<string> output, Expression list)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add(".removeAt(");
			this.Translator.TranslateExpression(output, list);
			output.Add(".length - 1)");
		}

		protected override void TranslateListPush(List<string> output, Expression list, Expression value)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add(".add(");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateListRemoveAt(List<string> output, Expression list, Expression index)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add(".removeAt(");
			this.Translator.TranslateExpression(output, index);
			output.Add(")");
		}

		protected override void TranslateListReverseInPlace(List<string> output, Expression list)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add(".reverse()");
		}

		protected override void TranslateListSet(List<string> output, Expression list, Expression index, Expression value)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add(".set(");
			this.Translator.TranslateExpression(output, index);
			output.Add(", ");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateListShuffleInPlace(List<string> output, Expression list)
		{
			output.Add("TranslationHelper.shuffleInPlace(");
			this.Translator.TranslateExpression(output, list);
			output.Add(")");
		}

		protected override void TranslateMultiplyList(List<string> output, Expression list, Expression num)
		{

			output.Add("TranslationHelper.multiplyList(");
			this.Translator.TranslateExpression(output, list);
			output.Add(", ");
			this.Translator.TranslateExpression(output, num);
			output.Add(")");
		}

		protected override void TranslateNewArray(List<string> output, StringConstant type, Expression size)
		{
			output.Add("new ");
			this.JavaPlatform.GetTypeStringFromString(type.Value, false);
			output.Add("[");
			this.Translator.TranslateExpression(output, size);
			output.Add("]");
		}

		protected override void TranslateNewDictionary(List<string> output, StringConstant keyType, StringConstant valueType)
		{
			output.Add("new HashMap<");
			this.JavaPlatform.GetTypeStringFromString(keyType.Value, true);
			output.Add(", ");
			this.JavaPlatform.GetTypeStringFromString(valueType.Value, true);
			output.Add(">()");
		}

		protected override void TranslateNewList(List<string> output, StringConstant type)
		{
			output.Add("new ArrayList<");
			output.Add(this.JavaPlatform.GetTypeStringFromString(type.Value, true));
			output.Add(">()");
		}

		protected override void TranslateNewListOfSize(List<string> output, StringConstant type, Expression length)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateNewStack(List<string> output, StringConstant type)
		{
			output.Add("new Stack<");
			output.Add(this.JavaPlatform.GetTypeStringFromString(type.Value, true));
			output.Add(">()");
		}

		protected override void TranslatePauseForFrame(List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslatePrint(List<string> output, Expression message)
		{
			output.Add("System.out.println(");
			this.Translator.TranslateExpression(output, message);
			output.Add(")");
		}

		protected override void TranslateRegisterTicker(List<string> output)
		{
			// Nope
		}

		protected override void TranslateRegisterTimeout(List<string> output)
		{
			// Nope
		}

		protected override void TranslateStackGet(List<string> output, Expression stack, Expression index)
		{
			this.Translator.TranslateExpression(output, stack);
			output.Add(".get(");
			this.Translator.TranslateExpression(output, index);
			output.Add(")");
		}

		protected override void TranslateStackLength(List<string> output, Expression stack)
		{
			this.Translator.TranslateExpression(output, stack);
			output.Add(".size()");
		}

		protected override void TranslateStackPop(List<string> output, Expression stack)
		{
			this.Translator.TranslateExpression(output, stack);
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
			output.Add(".set(");
			this.Translator.TranslateExpression(output, index);
			output.Add(", ");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateSetProgramData(List<string> output, Expression programData)
		{
			output.Add("TranslationHelper.getProgramData(");
			this.Translator.TranslateExpression(output, programData);
			output.Add(")");
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
				output.Add("(\"\" + ");
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
			this.Translator.TranslateExpression(output, haystack);
			output.Add(".contains(");
			this.Translator.TranslateExpression(output, needle);
			output.Add(")");
		}

		protected override void TranslateStringEndsWith(List<string> output, Expression stringExpr, Expression findMe)
		{
			this.Translator.TranslateExpression(output, stringExpr);
			output.Add(".endsWith(");
			this.Translator.TranslateExpression(output, findMe);
			output.Add(")");
		}

		protected override void TranslateStringFromCode(List<string> output, Expression characterCode)
		{
			output.Add("Character.toString((char)");
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
			output.Add(".length()");
		}

		protected override void TranslateStringLower(List<string> output, Expression stringValue)
		{
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(".toLower()");
		}

		protected override void TranslateStringParseFloat(List<string> output, Expression stringValue)
		{
			output.Add("Double.parseDouble(");
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(")");
		}

		protected override void TranslateStringParseInt(List<string> output, Expression value)
		{
			output.Add("Integer.parseInt(");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateStringReplace(List<string> output, Expression stringValue, Expression findMe, Expression replaceWith)
		{
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(".replace((CharSequence)");
			this.Translator.TranslateExpression(output, findMe);
			output.Add(", (CharSequence)");
			this.Translator.TranslateExpression(output, replaceWith);
			output.Add(")");
		}

		protected override void TranslateStringReverse(List<string> output, Expression stringValue)
		{
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(".reverse()");
		}

		protected override void TranslateStringSplit(List<string> output, Expression stringExpr, Expression sep)
		{
			this.Translator.TranslateExpression(output, stringExpr);
			output.Add(".split(Pattern.quote(");
			this.Translator.TranslateExpression(output, sep);
			output.Add(")");
		}

		protected override void TranslateStringStartsWith(List<string> output, Expression stringExpr, Expression findMe)
		{
			this.Translator.TranslateExpression(output, stringExpr);
			output.Add(".startsWith(");
			this.Translator.TranslateExpression(output, findMe);
			output.Add(")");
		}

		protected override void TranslateStringTrim(List<string> output, Expression stringValue)
		{
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(".trim()");
		}

		protected override void TranslateStringUpper(List<string> output, Expression stringValue)
		{
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(".toUpper()");
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
