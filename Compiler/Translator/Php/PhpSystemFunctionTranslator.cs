using System;
using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon.Translator.Php
{
    internal class PhpSystemFunctionTranslator : AbstractSystemFunctionTranslator
    {
        private PhpTranslator PhpTranslator { get { return (PhpTranslator)this.Translator; } }

        protected override void TranslateCast(List<string> output, StringConstant typeValue, Expression expression)
        {
            this.Translator.TranslateExpression(output, expression);
        }

        protected override void TranslateCastToList(List<string> output, StringConstant typeValue, Expression enumerableThing)
        {
            this.Translator.TranslateExpression(output, enumerableThing);
        }

        protected override void TranslateChr(List<string> output, Expression asciiValue)
        {
            output.Add("chr(");
            this.Translator.TranslateExpression(output, asciiValue);
            output.Add(")");
        }

        protected override void TranslateCommandLineArgs(List<string> output)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateComment(List<string> output, StringConstant commentValue)
        {
            output.Add("// ");
            output.Add(commentValue.Value);
        }

        protected override void TranslateConvertListToArray(List<string> output, StringConstant type, Expression list)
        {
            output.Add("new Rf(array_merge(array(), ");
            this.Translator.TranslateExpression(output, list);
            output.Add("->r))");
        }

        protected override void TranslateDictionaryGetGuaranteed(List<string> output, Expression dictionary, Expression key)
        {
            this.Translator.TranslateExpression(output, dictionary);
            output.Add("->r[");
            this.Translator.TranslateExpression(output, key);
            output.Add("]");
        }

        protected override void TranslateDictionaryGetKeys(List<string> output, string keyType, Expression dictionary)
        {
            output.Add("pth_dictionary_get_keys(");
            this.PhpTranslator.TranslateExpression(output, dictionary);
            output.Add(")");
        }

        protected override void TranslateDictionaryGetValues(List<string> output, Expression dictionary)
        {
            output.Add("pth_dictionary_get_values(");
            this.PhpTranslator.TranslateExpression(output, dictionary);
            output.Add(")");
        }

        protected override void TranslateDictionaryRemove(List<string> output, Expression dictionary, Expression key)
        {
            output.Add("unset(");
            this.Translator.TranslateExpression(output, dictionary);
            output.Add("->r[");
            this.Translator.TranslateExpression(output, key);
            output.Add("])");
        }

        protected override void TranslateDictionarySet(List<string> output, Expression dictionary, Expression key, Expression value)
        {
            this.Translator.TranslateExpression(output, dictionary);
            output.Add("->r[");
            this.Translator.TranslateExpression(output, key);
            output.Add("] = ");
            this.Translator.TranslateExpression(output, value);
        }

        protected override void TranslateDictionarySize(List<string> output, Expression dictionary)
        {
            output.Add("count(");
            this.Translator.TranslateExpression(output, dictionary);
            output.Add("->r)");
        }

        protected override void TranslateEnqueueVmResume(List<string> output, Expression seconds, Expression executionContextId)
        {
            throw new InvalidOperationException(); // optimized out.
        }

        protected override void TranslateInt(List<string> output, Expression value)
        {
            output.Add("intval(");
            this.Translator.TranslateExpression(output, value);
            output.Add(")");
        }

        protected override void TranslateIsWindowsProgram(List<string> output)
        {
            output.Add("TODO_optimize_out()");
        }

        protected override void TranslateNewArray(List<string> output, StringConstant type, Expression size)
        {
            output.Add("pth_new_array(");
            this.PhpTranslator.TranslateExpression(output, size);
            output.Add(")");
        }

        protected override void TranslateNewDictionary(List<string> output, StringConstant keyType, StringConstant valueType)
        {
            output.Add("new Rf(array())");
        }

        protected override void TranslateNewList(List<string> output, StringConstant type)
        {
            output.Add("new Rf(array())");
        }

        protected override void TranslateNewListOfSize(List<string> output, StringConstant type, Expression length)
        {
            output.Add("pth_new_array(");
            this.PhpTranslator.TranslateExpression(output, length);
            output.Add(")");
        }

        protected override void TranslateOrd(List<string> output, Expression character)
        {
            output.Add("ord(");
            this.Translator.TranslateExpression(output, character);
            output.Add(")");
        }

        protected override void TranslateParseFloat(List<string> output, Expression outParam, Expression rawString)
        {
            output.Add("pth_parse_float(");
            this.PhpTranslator.TranslateExpression(output, outParam);
            output.Add(", ");
            this.PhpTranslator.TranslateExpression(output, rawString);
            output.Add(")");
        }

        protected override void TranslateParseInt(List<string> output, Expression rawString)
        {
            output.Add("intval(");
            this.Translator.TranslateExpression(output, rawString);
            output.Add(")");
        }

        protected override void TranslateResourceGetManifest(List<string> output)
        {
            output.Add("''");
        }

        protected override void TranslateSortedCopyOfIntArray(List<string> output, Expression list)
        {
            output.Add("pth_sorted_copy_ints(");
            this.PhpTranslator.TranslateExpression(output, list);
            output.Add(")");
        }

        protected override void TranslateSortedCopyOfStringArray(List<string> output, Expression list)
        {
            output.Add("pth_sorted_copy_strings(");
            this.PhpTranslator.TranslateExpression(output, list);
            output.Add(")");
        }

        protected override void TranslateStringAppend(List<string> output, Expression target, Expression valueToAppend)
        {
            this.Translator.TranslateExpression(output, target);
            output.Add(" .= ");
            this.Translator.TranslateExpression(output, valueToAppend);
        }

        protected override void TranslateStringAsChar(List<string> output, StringConstant stringConstant)
        {
            string value = stringConstant.Value;
            if (value == "$")
            {
                output.Add("'$'");
            }
            else
            {
                output.Add(Util.ConvertStringValueToCode(value));
            }
        }

        protected override void TranslateStringCast(List<string> output, Expression thing, bool strongCast)
        {
            if (strongCast)
            {
                output.Add("('' . ");
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
            output.Add("[");
            this.Translator.TranslateExpression(output, index);
            output.Add("]");
        }

        protected override void TranslateStringCharCodeAt(List<string> output, Expression stringValue, Expression index)
        {
            output.Add("ord(");
            this.Translator.TranslateExpression(output, stringValue);
            output.Add("[");
            this.Translator.TranslateExpression(output, index);
            output.Add("])");
        }

        protected override void TranslateStringCompareIsReverse(List<string> output, Expression a, Expression b)
        {
            output.Add("(strcmp(");
            this.Translator.TranslateExpression(output, a);
            output.Add(", ");
            this.Translator.TranslateExpression(output, b);
            output.Add(") > 0)");
        }

        protected override void TranslateStringConcat(List<string> output, Expression[] values)
        {
            for (int i = 0; i < values.Length; ++i)
            {
                if (i > 0) output.Add(" . ");
                this.Translator.TranslateExpression(output, values[i]);
            }
        }

        protected override void TranslateStringEquals(List<string> output, Expression aNonNull, Expression b)
        {
            output.Add("(");
            this.Translator.TranslateExpression(output, aNonNull);
            output.Add(" === ");
            this.Translator.TranslateExpression(output, b);
            output.Add(")");
        }

        protected override void TranslateStringFromCode(List<string> output, Expression characterCode)
        {
            output.Add("chr(");
            this.Translator.TranslateExpression(output, characterCode);
            output.Add(")");
        }

        protected override void TranslateStringIndexOf(List<string> output, Expression haystack, Expression needle, Expression optionalStartFrom)
        {
            output.Add("pth_string_index_of(");
            this.PhpTranslator.TranslateExpression(output, haystack);
            output.Add(", ");
            this.PhpTranslator.TranslateExpression(output, needle);
            if (optionalStartFrom != null)
            {
                output.Add(", ");
                this.Translator.TranslateExpression(output, optionalStartFrom);
            }
            output.Add(")");
        }

        protected override void TranslateStringParseFloat(List<string> output, Expression stringValue)
        {
            output.Add("floatval(");
            this.Translator.TranslateExpression(output, stringValue);
            output.Add(")");
        }

        protected override void TranslateStringParseInt(List<string> output, Expression value)
        {
            output.Add("intval(");
            this.Translator.TranslateExpression(output, value);
            output.Add(")");
        }

        protected override void TranslateStringSubstring(List<string> output, Expression stringExpr, Expression startIndex, Expression optionalLength)
        {
            output.Add("pth_string_substring(");
            this.Translator.TranslateExpression(output, stringExpr);
            output.Add(", ");
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
            output.Add("pth_string_check_slice(");
            this.Translator.TranslateExpression(output, stringExpr);
            output.Add(", ");
            this.Translator.TranslateExpression(output, lookFor);
            output.Add(", ");
            this.Translator.TranslateExpression(output, index);
            output.Add(")");
        }

        protected override void TranslateThreadSleep(List<string> output, Expression timeDelaySeconds)
        {
            // http://php.net/manual/en/function.time-nanosleep.php
            throw new NotImplementedException();
        }
    }
}
