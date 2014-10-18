using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon.Translator.CSharp
{
	class CSharpSystemFunctionTranslator : AbstractSystemFunctionTranslator
	{
		public CSharpSystemFunctionTranslator() : base() { }

		protected override void TranslateBeginFrame(List<string> output)
		{
			// Nope
		}

		protected override void TranslateComment(List<string> output, ParseTree.Expression commentValue)
		{
#if DEBUG
			output.Add("// " + ((ParseTree.StringConstant)commentValue).Value);
#endif
		}

		protected override void TranslateDictionaryContains(List<string> output, ParseTree.Expression dictionary, ParseTree.Expression key)
		{
			this.Translator.TranslateExpression(output, dictionary);
			output.Add(".ContainsKey(");
			this.Translator.TranslateExpression(output, key);
			output.Add(")");
		}

		protected override void TranslateDictionaryGet(List<string> output, ParseTree.Expression dictionary, ParseTree.Expression key, ParseTree.Expression defaultValue)
		{
			this.Translator.TranslateExpression(output, dictionary);
			output.Add("[");
			this.Translator.TranslateExpression(output, key);
			output.Add("]");
			// TODO: audit each usage and see which ones are guaranteed keys and which should have fallbacks
		}

		protected override void TranslateDictionaryGetKeys(List<string> output, ParseTree.Expression dictionary)
		{
			this.Translator.TranslateExpression(output, dictionary);
			output.Add(".Keys");
		}

		protected override void TranslateDictionaryGetValues(List<string> output, ParseTree.Expression dictionary)
		{
			this.Translator.TranslateExpression(output, dictionary);
			output.Add(".Values");
		}

		protected override void TranslateDictionaryRemove(List<string> output, ParseTree.Expression dictionary, ParseTree.Expression key)
		{
			this.Translator.TranslateExpression(output, dictionary);
			output.Add(".Remove(");
			this.Translator.TranslateExpression(output, key);
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

		protected override void TranslateDictionarySize(List<string> output, ParseTree.Expression dictionary)
		{
			this.Translator.TranslateExpression(output, dictionary);
			output.Add(".Count");
		}

		protected override void TranslateExponent(List<string> output, ParseTree.Expression baseNum, ParseTree.Expression powerNum)
		{
			output.Add("System.Math.Pow(");
			this.Translator.TranslateExpression(output, baseNum);
			output.Add(", ");
			this.Translator.TranslateExpression(output, powerNum);
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
			output.Add("// TODO: Framework code for " + id);
		}

		protected override void TranslateInt(List<string> output, ParseTree.Expression value)
		{
			output.Add("((int)");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateListConcat(List<string> output, ParseTree.Expression listA, ParseTree.Expression listB)
		{
			output.Add("TranslationUtil.ValueListConcat(");
			this.Translator.TranslateExpression(output, listA);
			output.Add(", ");
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
			output.Add(".Insert(");
			this.Translator.TranslateExpression(output, index);
			output.Add(", ");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateListJoin(List<string> output, ParseTree.Expression list, ParseTree.Expression sep)
		{
			output.Add("string.join(");
			this.Translator.TranslateExpression(output, sep);
			output.Add(", ");
			this.Translator.TranslateExpression(output, list);
			output.Add(")");
		}

		protected override void TranslateListLastIndex(List<string> output, ParseTree.Expression list)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add(".Count - 1");
		}

		protected override void TranslateListLength(List<string> output, ParseTree.Expression list)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add(".Count");
		}

		protected override void TranslateListNew(List<string> output, ParseTree.Expression length)
		{
			output.Add("TranslationHelper.NewListOfSize(");
			this.Translator.TranslateExpression(output, length);
			output.Add(")");
		}

		protected override void TranslateListPop(List<string> output, ParseTree.Expression list)
		{
			// TODO: make some lists stacks
			this.Translator.TranslateExpression(output, list);
			output.Add(".RemoveAt(");
			this.Translator.TranslateExpression(output, list);
			output.Add(".Count - 1)");
		}

		protected override void TranslateListPush(List<string> output, ParseTree.Expression list, ParseTree.Expression value)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add(".Add(");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateListRemoveAt(List<string> output, ParseTree.Expression list, ParseTree.Expression index)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add(".RemoveAt(");
			this.Translator.TranslateExpression(output, index);
			output.Add(")");
		}

		protected override void TranslateListReverseInPlace(List<string> output, ParseTree.Expression listVar)
		{
			this.Translator.TranslateExpression(output, listVar);
			output.Add(".Reverse()");
		}

		protected override void TranslateListSet(List<string> output, ParseTree.Expression list, ParseTree.Expression index, ParseTree.Expression value)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add("[");
			this.Translator.TranslateExpression(output, index);
			output.Add("] = ");
			this.Translator.TranslateExpression(output, value);
		}

		protected override void TranslateListShuffleInPlace(List<string> output, ParseTree.Expression list)
		{
			output.Add("TranslationHelper.ShuffleInPlace(");
			this.Translator.TranslateExpression(output, list);
			output.Add(")");
		}

		protected override void TranslatePauseForFrame(List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslatePrint(List<string> output, ParseTree.Expression message)
		{
			output.Add("System.Console.WriteLine(");
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

		protected override void TranslateSetProgramData(List<string> output, ParseTree.Expression programData)
		{
			output.Add("TranslationHelper.ProgramData = ");
			this.Translator.TranslateExpression(output, programData);
		}

		protected override void TranslateStringCast(List<string> output, ParseTree.Expression thing, bool strongCast)
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

		protected override void TranslateStringCharAt(List<string> output, ParseTree.Expression stringValue, ParseTree.Expression index)
		{
			this.Translator.TranslateExpression(output, stringValue);
			output.Add("[");
			this.Translator.TranslateExpression(output, index);
			output.Add("]");
		}

		protected override void TranslateStringContains(List<string> output, ParseTree.Expression haystack, ParseTree.Expression needle)
		{
			this.Translator.TranslateExpression(output, haystack);
			output.Add(".Contains(");
			this.Translator.TranslateExpression(output, needle);
			output.Add(")");
		}

		protected override void TranslateStringEndsWith(List<string> output, ParseTree.Expression stringExpr, ParseTree.Expression findMe)
		{
			this.Translator.TranslateExpression(output, stringExpr);
			output.Add(".EndsWith(");
			this.Translator.TranslateExpression(output, findMe);
			output.Add(")");
		}

		protected override void TranslateStringFromCode(List<string> output, ParseTree.Expression characterCode)
		{
			output.Add("(\"\" + (char)");
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
			output.Add(".Length");
		}

		protected override void TranslateStringLower(List<string> output, ParseTree.Expression stringValue)
		{
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(".ToLowerInvariant()");
		}

		protected override void TranslateStringParseFloat(List<string> output, ParseTree.Expression stringValue)
		{
			output.Add("double.Parse(");
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(")");
		}

		protected override void TranslateStringParseInt(List<string> output, ParseTree.Expression value)
		{
			output.Add("int.Parse(");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateStringReplace(List<string> output, ParseTree.Expression stringValue, ParseTree.Expression findMe, ParseTree.Expression replaceWith)
		{
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(".Replace(");
			this.Translator.TranslateExpression(output, findMe);
			output.Add(", ");
			this.Translator.TranslateExpression(output, replaceWith);
			output.Add(")");
		}

		protected override void TranslateStringReverse(List<string> output, ParseTree.Expression stringValue)
		{
			output.Add("TranslationHelper.StringReverse(");
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(")");
		}

		protected override void TranslateStringSplit(List<string> output, ParseTree.Expression stringExpr, ParseTree.Expression sep)
		{
			this.Translator.TranslateExpression(output, stringExpr);
			output.Add(".Split(");
			this.Translator.TranslateExpression(output, sep);
			output.Add(")");
		}

		protected override void TranslateStringStartsWith(List<string> output, ParseTree.Expression stringExpr, ParseTree.Expression findMe)
		{
			this.Translator.TranslateExpression(output, stringExpr);
			output.Add(".StartsWith(");
			this.Translator.TranslateExpression(output, findMe);
			output.Add(")");
		}

		protected override void TranslateStringTrim(List<string> output, ParseTree.Expression stringValue)
		{
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(".Trim()");
		}

		protected override void TranslateStringUpper(List<string> output, ParseTree.Expression stringValue)
		{
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(".ToUpperInvariant()");
		}

		protected override void TranslateUnregisterTicker(List<string> output)
		{
			// Nope
		}

		protected override void TranslateUnsafeFloatDivision(List<string> output, ParseTree.Expression numerator, ParseTree.Expression denominator)
		{
			this.Translator.TranslateExpression(output, numerator);
			output.Add(" / ");
			this.Translator.TranslateExpression(output, denominator);
		}

		protected override void TranslateUnsafeIntegerDivision(List<string> output, ParseTree.Expression numerator, ParseTree.Expression denominator)
		{
			this.Translator.TranslateExpression(output, numerator);
			output.Add(" / ");
			this.Translator.TranslateExpression(output, denominator);
		}
	}
}
