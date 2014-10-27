using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon.Translator.Java
{
	class JavaSystemFunctionTranslator : AbstractSystemFunctionTranslator
	{
		protected override void TranslateArrayGet(List<string> output, ParseTree.Expression list, ParseTree.Expression index)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateArrayLength(List<string> output, ParseTree.Expression list)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateArraySet(List<string> output, ParseTree.Expression list, ParseTree.Expression index, ParseTree.Expression value)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateBeginFrame(List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateCast(List<string> output, ParseTree.Expression typeValue, ParseTree.Expression expression)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateCastToList(List<string> output, ParseTree.Expression enumerableThing)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateCharToString(List<string> output, ParseTree.Expression charValue)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateComment(List<string> output, ParseTree.Expression commentValue)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateConvertListToArray(List<string> output, ParseTree.Expression list)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDictionaryContains(List<string> output, ParseTree.Expression dictionary, ParseTree.Expression key)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDictionaryGetGuaranteed(List<string> output, ParseTree.Expression dictionary, ParseTree.Expression key)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDictionaryGetKeys(List<string> output, ParseTree.Expression dictionary)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDictionaryGetValues(List<string> output, ParseTree.Expression dictionary)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDictionaryRemove(List<string> output, ParseTree.Expression dictionary, ParseTree.Expression key)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDictionarySet(List<string> output, ParseTree.Expression dict, ParseTree.Expression key, ParseTree.Expression value)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDictionarySize(List<string> output, ParseTree.Expression dictionary)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDotEquals(List<string> output, ParseTree.Expression root, ParseTree.Expression compareTo)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateExponent(List<string> output, ParseTree.Expression baseNum, ParseTree.Expression powerNum)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateForceParens(List<string> output, ParseTree.Expression expression)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateGetProgramData(List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateGetRawByteCodeString(List<string> output, string theString)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateInsertFrameworkCode(string tab, List<string> output, string id)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateInt(List<string> output, ParseTree.Expression value)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateListClear(List<string> output, ParseTree.Expression list)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateListConcat(List<string> output, ParseTree.Expression listA, ParseTree.Expression listB)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateListGet(List<string> output, ParseTree.Expression list, ParseTree.Expression index)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateListInsert(List<string> output, ParseTree.Expression list, ParseTree.Expression index, ParseTree.Expression value)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateListJoin(List<string> output, ParseTree.Expression list, ParseTree.Expression sep)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateListLastIndex(List<string> output, ParseTree.Expression list)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateListLength(List<string> output, ParseTree.Expression list)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateListPop(List<string> output, ParseTree.Expression list)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateListPush(List<string> output, ParseTree.Expression list, ParseTree.Expression value)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateListRemoveAt(List<string> output, ParseTree.Expression list, ParseTree.Expression index)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateListReverseInPlace(List<string> output, ParseTree.Expression listVar)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateListSet(List<string> output, ParseTree.Expression list, ParseTree.Expression index, ParseTree.Expression value)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateListShuffleInPlace(List<string> output, ParseTree.Expression list)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateMultiplyList(List<string> output, ParseTree.Expression list, ParseTree.Expression num)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateNewArray(List<string> output, ParseTree.StringConstant type, ParseTree.Expression size)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateNewDictionary(List<string> output, ParseTree.StringConstant keyType, ParseTree.StringConstant valueType)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateNewList(List<string> output, ParseTree.StringConstant type)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateNewListOfSize(List<string> output, ParseTree.StringConstant type, ParseTree.Expression length)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateNewStack(List<string> output, ParseTree.StringConstant type)
		{
			throw new NotImplementedException();
		}

		protected override void TranslatePauseForFrame(List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslatePrint(List<string> output, ParseTree.Expression message)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateRegisterTicker(List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateRegisterTimeout(List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStackGet(List<string> output, ParseTree.Expression stack, ParseTree.Expression index)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStackLength(List<string> output, ParseTree.Expression stack)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStackPop(List<string> output, ParseTree.Expression list)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStackPush(List<string> output, ParseTree.Expression list, ParseTree.Expression value)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStackSet(List<string> output, ParseTree.Expression stack, ParseTree.Expression index, ParseTree.Expression value)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateSetProgramData(List<string> output, ParseTree.Expression programData)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringAsChar(List<string> output, ParseTree.StringConstant stringConstant)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringCast(List<string> output, ParseTree.Expression thing, bool strongCast)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringCharAt(List<string> output, ParseTree.Expression stringValue, ParseTree.Expression index)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringContains(List<string> output, ParseTree.Expression haystack, ParseTree.Expression needle)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringEndsWith(List<string> output, ParseTree.Expression stringExpr, ParseTree.Expression findMe)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringFromCode(List<string> output, ParseTree.Expression characterCode)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringIndexOf(List<string> output, ParseTree.Expression haystack, ParseTree.Expression needle)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringLength(List<string> output, ParseTree.Expression stringValue)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringLower(List<string> output, ParseTree.Expression stringValue)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringParseFloat(List<string> output, ParseTree.Expression stringValue)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringParseInt(List<string> output, ParseTree.Expression value)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringReplace(List<string> output, ParseTree.Expression stringValue, ParseTree.Expression findMe, ParseTree.Expression replaceWith)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringReverse(List<string> output, ParseTree.Expression stringValue)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringSplit(List<string> output, ParseTree.Expression stringExpr, ParseTree.Expression sep)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringStartsWith(List<string> output, ParseTree.Expression stringExpr, ParseTree.Expression findMe)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringTrim(List<string> output, ParseTree.Expression stringValue)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringUpper(List<string> output, ParseTree.Expression stringValue)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateUnregisterTicker(List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateUnsafeFloatDivision(List<string> output, ParseTree.Expression numerator, ParseTree.Expression denominator)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateUnsafeIntegerDivision(List<string> output, ParseTree.Expression numerator, ParseTree.Expression denominator)
		{
			throw new NotImplementedException();
		}
	}
}
