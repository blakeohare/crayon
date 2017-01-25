using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crayon.ParseTree;

namespace Crayon.Translator.C
{
    internal class CSystemFunctionTranslator : AbstractSystemFunctionTranslator
    {
        protected override void TranslateCast(List<string> output, StringConstant typeValue, Expression expression)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateCastToList(List<string> output, StringConstant typeValue, Expression enumerableThing)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateChr(List<string> output, Expression asciiValue)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateCommandLineArgs(List<string> output)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateComment(List<string> output, StringConstant commentValue)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateConvertListToArray(List<string> output, StringConstant type, Expression list)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateDictionaryGetGuaranteed(List<string> output, Expression dictionary, Expression key)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateDictionaryGetKeys(List<string> output, string keyType, Expression dictionary)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateDictionaryGetValues(List<string> output, Expression dictionary)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateDictionaryRemove(List<string> output, Expression dictionary, Expression key)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateDictionarySet(List<string> output, Expression dictionary, Expression key, Expression value)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateDictionarySize(List<string> output, Expression dictionary)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateEnqueueVmResume(List<string> output, Expression seconds, Expression executionContextId)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateInt(List<string> output, Expression value)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateIsWindowsProgram(List<string> output)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateNewArray(List<string> output, StringConstant type, Expression size)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateNewDictionary(List<string> output, StringConstant keyType, StringConstant valueType)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateNewList(List<string> output, StringConstant type)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateNewListOfSize(List<string> output, StringConstant type, Expression length)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateOrd(List<string> output, Expression character)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateParseFloat(List<string> output, Expression outParam, Expression rawString)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateParseInt(List<string> output, Expression rawString)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateResourceGetManifest(List<string> output)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateSortedCopyOfIntArray(List<string> output, Expression list)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateSortedCopyOfStringArray(List<string> output, Expression list)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateStringAppend(List<string> output, Expression target, Expression valueToAppend)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateStringAsChar(List<string> output, StringConstant stringConstant)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateStringCast(List<string> output, Expression thing, bool strongCast)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateStringCharAt(List<string> output, Expression stringValue, Expression index)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateStringCharCodeAt(List<string> output, Expression stringValue, Expression index)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateStringCompareIsReverse(List<string> output, Expression a, Expression b)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateStringConcat(List<string> output, Expression[] values)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateStringEquals(List<string> output, Expression aNonNull, Expression b)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateStringFromCode(List<string> output, Expression characterCode)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateStringIndexOf(List<string> output, Expression haystack, Expression needle, Expression optionalStartFrom)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateStringParseFloat(List<string> output, Expression stringValue)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateStringParseInt(List<string> output, Expression value)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateStringSubstring(List<string> output, Expression stringExpr, Expression startIndex, Expression optionalLength)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateStringSubstringExistsAt(List<string> output, Expression stringExpr, Expression lookFor, Expression index)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateThreadSleep(List<string> output, Expression timeDelaySeconds)
        {
            throw new NotImplementedException();
        }
    }
}
