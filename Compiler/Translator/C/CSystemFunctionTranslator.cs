using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crayon.ParseTree;

namespace Crayon.Translator.C
{
    internal class CSystemFunctionTranslator : AbstractSystemFunctionTranslator
    {
        private COpenGlPlatform CPlatform { get { return (COpenGlPlatform)this.Platform; } }

        protected override void TranslateCast(List<string> output, StringConstant typeValue, Expression expression)
        {
            output.Add("(");
            output.Add(this.CPlatform.GetTypeStringFromAnnotation(typeValue.FirstToken, typeValue.Value));
            output.Add(")");
            this.Translator.TranslateExpression(output, expression);
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
            string cType = this.CPlatform.GetTypeStringFromAnnotation(type.FirstToken, type.Value);

            output.Add("(");
            output.Add(cType);
            output.Add("*) make_array_with_size(sizeof(");
            output.Add(cType);
            output.Add("), ");
            this.Translator.TranslateExpression(output, size);
            output.Add(")");
        }

        protected override void TranslateNewDictionary(List<string> output, StringConstant keyType, StringConstant valueType)
        {
            switch (keyType.Value)
            {
                case "string":
                    output.Add("DictString_new()");
                    break;
                case "int":
                    output.Add("DictInt_new()");
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        protected override void TranslateNewList(List<string> output, StringConstant type)
        {
            output.Add("List_new()");
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
            string value = stringConstant.Value;
            if (value.Length != 1)
            {
                throw new Exception(); // needs to be one character.
            }
            output.Add("" + (int)value[0]);
        }
        
        protected override void TranslateStringCharAt(List<string> output, Expression stringValue, Expression index)
        {
            this.Translator.TranslateExpression(output, stringValue);
            output.Add("->characters[");
            this.Translator.TranslateExpression(output, index);
            output.Add("]");
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
            output.Add("String_from_char_code(");
            this.Translator.TranslateExpression(output, characterCode);
            output.Add(")");
        }

        protected override void TranslateStringIndexOf(List<string> output, Expression haystack, Expression needle, Expression optionalStartFrom)
        {
            output.Add("String_index_of(");
            this.Translator.TranslateExpression(output, haystack);
            output.Add(", ");
            this.Translator.TranslateExpression(output, needle);
            if (optionalStartFrom != null)
            {
                output.Add(", ");
                this.Translator.TranslateExpression(output, optionalStartFrom);
            }
            else
            {
                output.Add(", 0");
            }
            output.Add(")");
        }

        protected override void TranslateStringParseFloat(List<string> output, Expression stringValue)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateStringParseInt(List<string> output, Expression value)
        {
            output.Add("cth_string_parse_int(");
            this.Translator.TranslateExpression(output, value);
            output.Add(")");
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
