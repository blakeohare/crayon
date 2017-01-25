using System;
using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon.Translator.Python
{
    internal class PythonSystemFunctionTranslator : AbstractSystemFunctionTranslator
    {
        protected override void TranslateResourceGetManifest(List<string> output)
        {
            output.Add("RESOURCES.getManifest()");
        }

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
            output.Add("sys.argv[1:]");
        }

        protected override void TranslateComment(List<string> output, StringConstant commentValue)
        {
#if DEBUG
            output.Add("# " + commentValue.Value);
#endif
        }

        protected override void TranslateConvertListToArray(List<string> output, StringConstant type, Expression list)
        {
            this.Translator.TranslateExpression(output, list);
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
            output.Add("list(");
            this.Translator.TranslateExpression(output, dictionary);
            output.Add(".keys())");
        }

        protected override void TranslateDictionaryGetValues(List<string> output, Expression dictionary)
        {
            output.Add("list(");
            this.Translator.TranslateExpression(output, dictionary);
            output.Add(".values())");
        }

        protected override void TranslateDictionaryRemove(List<string> output, Expression dictionary, Expression key)
        {
            this.Translator.TranslateExpression(output, dictionary);
            output.Add(".pop(");
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
            output.Add("len(");
            this.Translator.TranslateExpression(output, dictionary);
            output.Add(")");
        }

        protected override void TranslateEnqueueVmResume(List<string> output, Expression seconds, Expression executionContextId)
        {
            throw new InvalidOperationException(); // optimized out.
        }

        protected override void TranslateInt(List<string> output, Expression value)
        {
            output.Add("int(");
            this.Translator.TranslateExpression(output, value);
            output.Add(")");
        }

        protected override void TranslateIsWindowsProgram(List<string> output)
        {
            output.Add("(sys.platform == 'win32')");
        }

        private void TranslateNewPythonList(List<string> output, Expression size)
        {
            if (size is IntegerConstant)
            {
                int length = ((IntegerConstant)size).Value;
                if (length == 0)
                {
                    output.Add("[]");
                }
                else if (length == 1)
                {
                    output.Add("[None]");
                }
                else
                {
                    output.Add("[None]");
                    output.Add(" * ");
                    output.Add("" + length);
                }
            }
            else
            {
                output.Add("[None] * ");
                this.Translator.TranslateExpression(output, size);
            }
        }

        protected override void TranslateNewArray(List<string> output, StringConstant type, Expression size)
        {
            this.TranslateNewPythonList(output, size);
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
            this.TranslateNewPythonList(output, length);
        }

        protected override void TranslateOrd(List<string> output, Expression character)
        {
            output.Add("ord(");
            this.Translator.TranslateExpression(output, character);
            output.Add(")");
        }

        protected override void TranslateParseFloat(List<string> output, Expression outParam, Expression rawString)
        {
            output.Add("_parse_float_helper(");
            this.Translator.TranslateExpression(output, outParam);
            output.Add(", ");
            this.Translator.TranslateExpression(output, rawString);
            output.Add(")");
        }

        protected override void TranslateParseInt(List<string> output, Expression rawString)
        {
            output.Add("int(");
            this.Translator.TranslateExpression(output, rawString);
            output.Add(")");
        }

        protected override void TranslateSortedCopyOfIntArray(List<string> output, Expression list)
        {
            output.Add("create_sorted_copy_of_list(");
            this.Translator.TranslateExpression(output, list);
            output.Add(")");
        }

        protected override void TranslateSortedCopyOfStringArray(List<string> output, Expression list)
        {
            output.Add("create_sorted_copy_of_list(");
            this.Translator.TranslateExpression(output, list);
            output.Add(")");
        }

        protected override void TranslateStringAppend(List<string> output, Expression target, Expression valueToAppend)
        {
            this.Translator.TranslateExpression(output, target);
            output.Add(" += ");
            this.Translator.TranslateExpression(output, valueToAppend);
        }

        protected override void TranslateStringAsChar(List<string> output, StringConstant stringConstant)
        {
            this.Translator.TranslateExpression(output, stringConstant);
        }

        protected override void TranslateStringCast(List<string> output, Expression thing, bool strongCast)
        {
            output.Add("str(");
            this.Translator.TranslateExpression(output, thing);
            output.Add(")");
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
            output.Add("(");
            this.Translator.TranslateExpression(output, a);
            output.Add(" > ");
            this.Translator.TranslateExpression(output, b);
            output.Add(")");
        }

        protected override void TranslateStringConcat(List<string> output, Expression[] values)
        {
            for (int i = 0; i < values.Length; ++i)
            {
                if (i > 0) output.Add(" + ");
                this.Translator.TranslateExpression(output, values[i]);
            }
        }

        protected override void TranslateStringEquals(List<string> output, Expression aNonNull, Expression b)
        {
            this.Translator.TranslateExpression(output, aNonNull);
            output.Add(" == ");
            this.Translator.TranslateExpression(output, b);
        }

        protected override void TranslateStringFromCode(List<string> output, Expression characterCode)
        {
            output.Add("wrappedChr(");
            this.Translator.TranslateExpression(output, characterCode);
            output.Add(")");
        }

        protected override void TranslateStringIndexOf(List<string> output, Expression haystack, Expression needle, Expression optionalStartFrom)
        {
            this.Translator.TranslateExpression(output, haystack);
            output.Add(".find(");
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
            output.Add("float(");
            this.Translator.TranslateExpression(output, stringValue);
            output.Add(")");
        }

        protected override void TranslateStringParseInt(List<string> output, Expression value)
        {
            output.Add("int(");
            this.Translator.TranslateExpression(output, value);
            output.Add(")");
        }

        protected override void TranslateStringSubstring(List<string> output, Expression stringExpr, Expression startIndex, Expression optionalLength)
        {
            output.Add("string_substring(");
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
            output.Add("string_check_slice(");
            this.Translator.TranslateExpression(output, stringExpr);
            output.Add(", ");
            this.Translator.TranslateExpression(output, lookFor);
            output.Add(", ");
            this.Translator.TranslateExpression(output, index);
            output.Add(")");
        }

        protected override void TranslateThreadSleep(List<string> output, Expression timeDelaySeconds)
        {
            output.Add("time.sleep(");
            this.Translator.TranslateExpression(output, timeDelaySeconds);
            output.Add(")");
        }
    }
}
