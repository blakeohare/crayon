using System;
using Crayon.ParseTree;

namespace Crayon.Translator.Ruby
{
	internal class RubySystemFunctionTranslator : AbstractSystemFunctionTranslator
	{
		protected override void TranslateRandomFloat(System.Collections.Generic.List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateGetProgramData(System.Collections.Generic.List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateByteCodeGetOps(System.Collections.Generic.List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateCommandLineArgs(System.Collections.Generic.List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateIsWindowsProgram(System.Collections.Generic.List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateByteCodeGetIntArgs(System.Collections.Generic.List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateResourceGetManifest(System.Collections.Generic.List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateGetRawByteCodeString(System.Collections.Generic.List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateByteCodeGetStringArgs(System.Collections.Generic.List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateSin(System.Collections.Generic.List<string> output, Expression value)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateTan(System.Collections.Generic.List<string> output, Expression value)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateCos(System.Collections.Generic.List<string> output, Expression value)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateInt(System.Collections.Generic.List<string> output, Expression value)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateArcCos(System.Collections.Generic.List<string> output, Expression value)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateArcSin(System.Collections.Generic.List<string> output, Expression value)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateOrd(System.Collections.Generic.List<string> output, Expression character)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateMathLog(System.Collections.Generic.List<string> output, Expression value)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateChr(System.Collections.Generic.List<string> output, Expression asciiValue)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateNewList(System.Collections.Generic.List<string> output, StringConstant type)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateParseInt(System.Collections.Generic.List<string> output, Expression rawString)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringParseInt(System.Collections.Generic.List<string> output, Expression value)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringConcat(System.Collections.Generic.List<string> output, Expression[] values)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateIsValidInteger(System.Collections.Generic.List<string> output, Expression number)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateResourceReadText(System.Collections.Generic.List<string> output, Expression path)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringTrim(System.Collections.Generic.List<string> output, Expression stringValue)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateForceParens(System.Collections.Generic.List<string> output, Expression expression)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateCharToString(System.Collections.Generic.List<string> output, Expression charValue)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringUpper(System.Collections.Generic.List<string> output, Expression stringValue)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringLower(System.Collections.Generic.List<string> output, Expression stringValue)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringLtrim(System.Collections.Generic.List<string> output, Expression stringValue)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringRtrim(System.Collections.Generic.List<string> output, Expression stringValue)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateComment(System.Collections.Generic.List<string> output, StringConstant commentValue)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringReverse(System.Collections.Generic.List<string> output, Expression stringValue)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDictionarySize(System.Collections.Generic.List<string> output, Expression dictionary)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateSortedCopyOfIntArray(System.Collections.Generic.List<string> output, Expression list)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateSetProgramData(System.Collections.Generic.List<string> output, Expression programData)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateThreadSleep(System.Collections.Generic.List<string> output, Expression timeDelaySeconds)
		{
			throw new NotImplementedException();
		}

		protected override void TranslatePrint(System.Collections.Generic.List<string> output, Expression expression, bool isErr)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringFromCode(System.Collections.Generic.List<string> output, Expression characterCode)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringParseFloat(System.Collections.Generic.List<string> output, Expression stringValue)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateSortedCopyOfStringArray(System.Collections.Generic.List<string> output, Expression list)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDictionaryGetValues(System.Collections.Generic.List<string> output, Expression dictionary)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringAsChar(System.Collections.Generic.List<string> output, StringConstant stringConstant)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringCast(System.Collections.Generic.List<string> output, Expression thing, bool strongCast)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateArcTan(System.Collections.Generic.List<string> output, Expression dy, Expression dx)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDictionaryGetKeys(System.Collections.Generic.List<string> output, string keyType, Expression dictionary)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateMultiplyList(System.Collections.Generic.List<string> output, Expression list, Expression num)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateIncrement(System.Collections.Generic.List<string> output, Expression expression, bool increment, bool prefix)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateNewArray(System.Collections.Generic.List<string> output, StringConstant type, Expression size)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringEquals(System.Collections.Generic.List<string> output, Expression aNonNull, Expression b)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDotEquals(System.Collections.Generic.List<string> output, Expression root, Expression compareTo)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateExponent(System.Collections.Generic.List<string> output, Expression baseNum, Expression powerNum)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringSplit(System.Collections.Generic.List<string> output, Expression stringExpr, Expression sep)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringCompareIsReverse(System.Collections.Generic.List<string> output, Expression a, Expression b)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateCast(System.Collections.Generic.List<string> output, StringConstant typeValue, Expression expression)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateParseFloat(System.Collections.Generic.List<string> output, Expression outParam, Expression rawString)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateNewListOfSize(System.Collections.Generic.List<string> output, StringConstant type, Expression length)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringCharAt(System.Collections.Generic.List<string> output, Expression stringValue, Expression index)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringContains(System.Collections.Generic.List<string> output, Expression haystack, Expression needle)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDictionaryRemove(System.Collections.Generic.List<string> output, Expression dictionary, Expression key)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringEndsWith(System.Collections.Generic.List<string> output, Expression stringExpr, Expression findMe)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateConvertListToArray(System.Collections.Generic.List<string> output, StringConstant type, Expression list)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringAppend(System.Collections.Generic.List<string> output, Expression target, Expression valueToAppend)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringStartsWith(System.Collections.Generic.List<string> output, Expression stringExpr, Expression findMe)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringCharCodeAt(System.Collections.Generic.List<string> output, Expression stringValue, Expression index)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDictionaryGetGuaranteed(System.Collections.Generic.List<string> output, Expression dictionary, Expression key)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateNewDictionary(System.Collections.Generic.List<string> output, StringConstant keyType, StringConstant valueType)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateCastToList(System.Collections.Generic.List<string> output, StringConstant typeValue, Expression enumerableThing)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateUnsafeFloatDivision(System.Collections.Generic.List<string> output, Expression numerator, Expression denominator)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateEnqueueVmResume(System.Collections.Generic.List<string> output, Expression seconds, Expression executionContextId)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateUnsafeIntegerDivision(System.Collections.Generic.List<string> output, Expression numerator, Expression denominator)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDictionarySet(System.Collections.Generic.List<string> output, Expression dictionary, Expression key, Expression value)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringReplace(System.Collections.Generic.List<string> output, Expression stringValue, Expression findMe, Expression replaceWith)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringIndexOf(System.Collections.Generic.List<string> output, Expression haystack, Expression needle, Expression optionalStartFrom)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringSubstringExistsAt(System.Collections.Generic.List<string> output, Expression stringExpr, Expression lookFor, Expression index)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringSubstring(System.Collections.Generic.List<string> output, Expression stringExpr, Expression startIndex, Expression optionalLength)
		{
			throw new NotImplementedException();
		}
	}
}
