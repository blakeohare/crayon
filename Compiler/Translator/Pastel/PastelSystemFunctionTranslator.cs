using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crayon.ParseTree;
using Common;

namespace Crayon.Translator.Pastel
{
    class PastelSystemFunctionTranslator : AbstractSystemFunctionTranslator
    {
        protected override void TranslateCast(List<string> output, StringConstant typeValue, Expression expression)
        {
            output.Add("((");
            output.Add(typeValue.Value);
            output.Add(") ");
            this.Translator.TranslateExpression(output, expression);
            output.Add(")");
        }

        protected override void TranslateCastToList(List<string> output, StringConstant typeValue, Expression enumerableThing)
        {
            output.Add("Core.ConvertRawDictionaryValueCollectionToAReusableValueList(");
            this.Translator.TranslateExpression(output, enumerableThing);
            output.Add(")");
        }

        protected override void TranslateChr(List<string> output, Expression asciiValue)
        {
            output.Add("Core.Chr(");
            this.Translator.TranslateExpression(output, asciiValue);
            output.Add(")");
        }

        protected override void TranslateCommandLineArgs(List<string> output)
        {
            output.Add("Core.CommandLineArgs()");
        }

        protected override void TranslateComment(List<string> output, StringConstant commentValue)
        {
            output.Add("Core.EmitComment(");
            output.Add(Util.ConvertStringValueToCode(commentValue.Value));
            output.Add(")");
        }

        protected override void TranslateConvertListToArray(List<string> output, StringConstant type, Expression list)
        {
            output.Add("Core.ListToArray(");
            this.Translator.TranslateExpression(output, list);
            output.Add(")");
        }

        protected override void TranslateDictionaryGetGuaranteed(List<string> output, Expression dictionary, Expression key)
        {
            this.Translator.TranslateExpression(output, dictionary);
            output.Add("[");
            this.Translator.TranslateExpression(output, key);
            output.Add("]");
        }

        protected override void TranslateDictionaryGetKeys(List<string> output, string keyType, Expression dictionary)
        {
            this.Translator.TranslateExpression(output, dictionary);
            output.Add(".Keys()");
        }

        protected override void TranslateDictionaryGetValues(List<string> output, Expression dictionary)
        {
            this.Translator.TranslateExpression(output, dictionary);
            output.Add(".Values()");
        }

        protected override void TranslateDictionaryRemove(List<string> output, Expression dictionary, Expression key)
        {
            this.Translator.TranslateExpression(output, dictionary);
            output.Add(".Remove(");
            this.Translator.TranslateExpression(output, key);
            output.Add(")");
        }

        protected override void TranslateDictionarySet(List<string> output, Expression dictionary, Expression key, Expression value)
        {
            this.Translator.TranslateExpression(output, dictionary);
            output.Add("[");
            this.Translator.TranslateExpression(output, key);
            output.Add("] = ");
            this.Translator.TranslateExpression(output, value);
        }

        protected override void TranslateDictionarySize(List<string> output, Expression dictionary)
        {
            this.Translator.TranslateExpression(output, dictionary);
            output.Add(".Size()");
        }

        protected override void TranslateEnqueueVmResume(List<string> output, Expression seconds, Expression executionContextId)
        {
            output.Add("Core.EnqueueVmResume(");
            this.Translator.TranslateExpression(output, seconds);
            output.Add(", ");
            this.Translator.TranslateExpression(output, executionContextId);
            output.Add(")");
        }

        protected override void TranslateInt(List<string> output, Expression value)
        {
            output.Add("Core.Int(");
            this.Translator.TranslateExpression(output, value);
            output.Add(")");
        }

        protected override void TranslateIsWindowsProgram(List<string> output)
        {
            output.Add("Core.IsWindows()");
        }

        protected override void TranslateNewArray(List<string> output, StringConstant type, Expression size)
        {
            output.Add("new Array<");
            output.Add(type.Value);
            output.Add(">(");
            this.Translator.TranslateExpression(output, size);
            output.Add(")");
        }

        protected override void TranslateNewDictionary(List<string> output, StringConstant keyType, StringConstant valueType)
        {
            output.Add("new Dictionary<");
            output.Add(keyType.Value);
            output.Add(", ");
            output.Add(valueType.Value);
            output.Add(">()");
        }

        protected override void TranslateNewList(List<string> output, StringConstant type)
        {
            output.Add("new List<");
            output.Add(type.Value);
            output.Add(">()");
        }

        protected override void TranslateNewListOfSize(List<string> output, StringConstant type, Expression length)
        {
            output.Add("new List<");
            output.Add(type.Value);
            output.Add(">(");
            this.Translator.TranslateExpression(output, length);
            output.Add(")");
        }

        protected override void TranslateOrd(List<string> output, Expression character)
        {
            output.Add("Core.Ord(");
            this.Translator.TranslateExpression(output, character);
            output.Add(")");
        }

        protected override void TranslateParseFloat(List<string> output, Expression outParam, Expression rawString)
        {
            output.Add("Core.ParseFloat(");
            this.Translator.TranslateExpression(output, rawString);
            output.Add(", ");
            this.Translator.TranslateExpression(output, outParam);
            output.Add(")");
        }

        protected override void TranslateParseInt(List<string> output, Expression rawString)
        {
            output.Add("Core.ParseInt(");
            this.Translator.TranslateExpression(output, rawString);
            output.Add(")");
        }

        protected override void TranslateResourceGetManifest(List<string> output)
        {
            output.Add("Core.GetResourceManifest()");
        }

        protected override void TranslateSortedCopyOfIntArray(List<string> output, Expression list)
        {
            output.Add("Core.SortedCopyOfIntArray(");
            this.Translator.TranslateExpression(output, list);
            output.Add(")");
        }

        protected override void TranslateSortedCopyOfStringArray(List<string> output, Expression list)
        {
            output.Add("Core.SortedCopyOfStringArray(");
            this.Translator.TranslateExpression(output, list);
            output.Add(")");
        }

        protected override void TranslateStringAppend(List<string> output, Expression target, Expression valueToAppend)
        {
            output.Add("Core.StringAppend(");
            this.Translator.TranslateExpression(output, target);
            output.Add(", ");
            this.Translator.TranslateExpression(output, valueToAppend);
            output.Add(")");
        }

        protected override void TranslateStringAsChar(List<string> output, StringConstant stringConstant)
        {
            string charConstant = Util.ConvertStringValueToCode(stringConstant.Value);
            output.Add("'");
            output.Add(charConstant.Substring(1, charConstant.Length - 2));
            output.Add("'");
        }

        protected override void TranslateStringCharAt(List<string> output, Expression stringValue, Expression index)
        {
            this.Translator.TranslateExpression(output, stringValue);
            output.Add("[");
            this.Translator.TranslateExpression(output, index);
            output.Add("]");
        }

        protected override void TranslateStringCharCodeAt(List<string> output, Expression stringValue, Expression index)
        {
            this.Translator.TranslateExpression(output, stringValue);
            output.Add(".CharCodeAt(");
            this.Translator.TranslateExpression(output, index);
            output.Add(")");
        }

        protected override void TranslateStringCompareIsReverse(List<string> output, Expression a, Expression b)
        {
            output.Add("Core.StringCompareIsReverse(");
            this.Translator.TranslateExpression(output, a);
            output.Add(", ");
            this.Translator.TranslateExpression(output, b);
            output.Add(")");
        }

        protected override void TranslateStringConcat(List<string> output, Expression[] values)
        {
            output.Add("Core.StringConcatAll(");
            for (int i = 0; i < values.Length; ++i)
            {
                if (i > 0) output.Add(", ");
                this.Translator.TranslateExpression(output, values[i]);
            }
            output.Add(")");
        }

        protected override void TranslateStringEquals(List<string> output, Expression aNonNull, Expression b)
        {
            output.Add("Core.StringEquals(");
            this.Translator.TranslateExpression(output, aNonNull);
            output.Add(", ");
            this.Translator.TranslateExpression(output, b);
            output.Add(")");
        }

        protected override void TranslateStringFromCode(List<string> output, Expression characterCode)
        {
            output.Add("Core.StringFromCharCode(");
            this.Translator.TranslateExpression(output, characterCode);
            output.Add(")");
        }

        protected override void TranslateStringIndexOf(List<string> output, Expression haystack, Expression needle, Expression optionalStartFrom)
        {
            this.Translator.TranslateExpression(output, haystack);
            output.Add(".IndexOf(");
            this.Translator.TranslateExpression(output, needle);
            if (optionalStartFrom != null)
            {
                output.Add(", ");
                this.Translator.TranslateExpression(output, optionalStartFrom);
            }
            output.Add(")");
        }

        protected override void TranslateStringParseFloat(List<string> output, Expression stringValue)
        {
            // TODO: remove this.
            // there's already one of these, although the signature is slightly different.
            output.Add("Core.ParseFloat_REDUNDANT(");
            this.Translator.TranslateExpression(output, stringValue);
            output.Add(")");
        }

        protected override void TranslateStringParseInt(List<string> output, Expression value)
        {
            output.Add("Core.ParseInt(");
            this.Translator.TranslateExpression(output, value);
            output.Add(")");
        }

        protected override void TranslateStringSubstring(List<string> output, Expression stringExpr, Expression startIndex, Expression optionalLength)
        {
            this.Translator.TranslateExpression(output, stringExpr);
            output.Add(".SubString(");
            this.Translator.TranslateExpression(output, startIndex);
            if (optionalLength != null)
            {
                output.Add(", ");
                this.Translator.TranslateExpression(output, optionalLength);
            }
            output.Add(")");
        }

        protected override void TranslateStringSubstringExistsAt(List<string> output, Expression stringExpr, Expression lookFor, Expression index)
        {
            this.Translator.TranslateExpression(output, stringExpr);
            output.Add(".SubStringIsEqualTo(");
            this.Translator.TranslateExpression(output, lookFor);
            output.Add(", ");
            this.Translator.TranslateExpression(output, index);
            output.Add(")");
        }

        protected override void TranslateThreadSleep(List<string> output, Expression timeDelaySeconds)
        {
            output.Add("Core.ThreadSleep(");
            this.Translator.TranslateExpression(output, timeDelaySeconds);
            output.Add(")");
        }
    }
}
