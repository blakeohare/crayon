﻿using System;
using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon.Translator.Php
{
    internal class PhpSystemFunctionTranslator : AbstractSystemFunctionTranslator
    {
        private PhpTranslator PhpTranslator { get { return (PhpTranslator)this.Translator; } }

        protected override void TranslatePrint(List<string> output, Expression expression, bool isErr)
        {
            output.Add("echo ");
            this.Translator.TranslateExpression(output, expression);
            output.Add(" . \"\\n\"");
        }

        protected override void TranslateAppDataRoot(List<string> output)
        {
            output.Add("TODO_optimize_out()");
        }

        protected override void TranslateArcCos(List<string> output, Expression value)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateArcSin(List<string> output, Expression value)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateArcTan(List<string> output, Expression dy, Expression dx)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateArrayGet(List<string> output, Expression list, Expression index)
        {
            this.Translator.TranslateExpression(output, list);
            output.Add("->r[");
            this.Translator.TranslateExpression(output, index);
            output.Add("]");
        }

        protected override void TranslateArrayJoin(List<string> output, Expression array, Expression sep)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateArrayLength(List<string> output, Expression list)
        {
            output.Add("count(");
            this.Translator.TranslateExpression(output, list);
            output.Add("->r)");
        }

        protected override void TranslateArraySet(List<string> output, Expression list, Expression index, Expression value)
        {
            this.Translator.TranslateExpression(output, list);
            output.Add("->r[");
            this.Translator.TranslateExpression(output, index);
            output.Add("] = ");
            this.Translator.TranslateExpression(output, value);
        }

        protected override void TranslateAssert(List<string> output, Expression message)
        {
            output.Add("pth_assert(");
            this.Translator.TranslateExpression(output, message);
            output.Add(")");
        }
        
        protected override void TranslateByteCodeGetIntArgs(List<string> output)
        {
            output.Add("bytecode_get_iargs()");
        }

        protected override void TranslateByteCodeGetOps(List<string> output)
        {
            output.Add("bytecode_get_ops()");
        }

        protected override void TranslateByteCodeGetStringArgs(List<string> output)
        {
            output.Add("bytecode_get_sargs()");
        }

        protected override void TranslateCast(List<string> output, StringConstant typeValue, Expression expression)
        {
            this.Translator.TranslateExpression(output, expression);
        }

        protected override void TranslateCastToList(List<string> output, StringConstant typeValue, Expression enumerableThing)
        {
            this.Translator.TranslateExpression(output, enumerableThing);
        }

        protected override void TranslateCharToString(List<string> output, Expression charValue)
        {
            this.Translator.TranslateExpression(output, charValue);
        }

        protected override void TranslateChr(List<string> output, Expression asciiValue)
        {
            output.Add("chr(");
            this.Translator.TranslateExpression(output, asciiValue);
            output.Add(")");
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

        protected override void TranslateCos(List<string> output, Expression value)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateCurrentTimeSeconds(List<string> output)
        {
            output.Add("pth_current_time()");
        }

        protected override void TranslateDictionaryContains(List<string> output, Expression dictionary, Expression key)
        {
            output.Add("isset(");
            this.Translator.TranslateExpression(output, dictionary);
            output.Add("->r[");
            this.Translator.TranslateExpression(output, key);
            output.Add("])");
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

        protected override void TranslateDotEquals(List<string> output, Expression root, Expression compareTo)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateEnqueueVmResume(List<string> output, Expression seconds, Expression executionContextId)
        {
            throw new InvalidOperationException(); // optimized out.
        }

        protected override void TranslateExponent(List<string> output, Expression baseNum, Expression powerNum)
        {
            output.Add("pow(");
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
            output.Add("pth_getProgramData()");
        }

        protected override void TranslateGetRawByteCodeString(List<string> output)
        {
            output.Add("TODO_optimize_this_out()");
        }

        protected override void TranslateIncrement(List<string> output, Expression expression, bool increment, bool prefix)
        {
            string token = increment ? "++" : "--";
            if (prefix) output.Add(token);
            this.Translator.TranslateExpression(output, expression);
            if (!prefix) output.Add(token);
        }

        protected override void TranslateInt(List<string> output, Expression value)
        {
            output.Add("intval(");
            this.Translator.TranslateExpression(output, value);
            output.Add(")");
        }

        protected override void TranslateIsValidInteger(List<string> output, Expression number)
        {
            output.Add("pth_is_valid_integer(");
            this.PhpTranslator.TranslateExpression(output, number);
            output.Add(")");
        }

        protected override void TranslateIsWindowsProgram(List<string> output)
        {
            output.Add("TODO_optimize_out()");
        }

        protected override void TranslateListClear(List<string> output, Expression list)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateListConcat(List<string> output, Expression listA, Expression listB)
        {
            output.Add("new Rf(array_merge(");
            this.Translator.TranslateExpression(output, listA);
            output.Add("->r, ");
            this.Translator.TranslateExpression(output, listB);
            output.Add("->r))");
        }

        protected override void TranslateListGet(List<string> output, Expression list, Expression index)
        {
            this.Translator.TranslateExpression(output, list);
            output.Add("->r[");
            this.Translator.TranslateExpression(output, index);
            output.Add("]");
        }

        protected override void TranslateListInsert(List<string> output, Expression list, Expression index, Expression value)
        {
            output.Add("new Rf(array_splice(");
            this.Translator.TranslateExpression(output, list);
            output.Add("->r, ");
            this.Translator.TranslateExpression(output, index);
            output.Add(", 0, array(");
            this.Translator.TranslateExpression(output, value);
            output.Add(")))");
        }

        protected override void TranslateListJoin(List<string> output, Expression list, Expression sep)
        {
            output.Add("implode(");
            this.Translator.TranslateExpression(output, sep);
            output.Add(", ");
            this.Translator.TranslateExpression(output, list);
            output.Add("->r)");
        }

        protected override void TranslateListJoinChars(List<string> output, Expression list)
        {
            output.Add("implode('', ");
            this.Translator.TranslateExpression(output, list);
            output.Add("->r)");
        }

        protected override void TranslateListLastIndex(List<string> output, Expression list)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateListLength(List<string> output, Expression list)
        {
            output.Add("count(");
            this.Translator.TranslateExpression(output, list);
            output.Add("->r)");
        }

        protected override void TranslateListPop(List<string> output, Expression list)
        {
            output.Add("array_pop(");
            this.Translator.TranslateExpression(output, list);
            output.Add("->r)");
        }

        protected override void TranslateListPush(List<string> output, Expression list, Expression value)
        {
            output.Add("array_push(");
            this.Translator.TranslateExpression(output, list);
            output.Add("->r, ");
            this.Translator.TranslateExpression(output, value);
            output.Add(")");
        }

        protected override void TranslateListRemoveAt(List<string> output, Expression list, Expression index)
        {
            output.Add("array_splice(");
            this.Translator.TranslateExpression(output, list);
            output.Add("->r, ");
            this.Translator.TranslateExpression(output, index);
            output.Add(", 1)");
        }

        protected override void TranslateListReverseInPlace(List<string> output, Expression list)
        {
            output.Add("pth_list_reverse(");
            this.PhpTranslator.TranslateExpression(output, list);
            output.Add(")");
        }

        protected override void TranslateListSet(List<string> output, Expression list, Expression index, Expression value)
        {
            this.Translator.TranslateExpression(output, list);
            output.Add("->r[");
            this.Translator.TranslateExpression(output, index);
            output.Add("] = ");
            this.Translator.TranslateExpression(output, value);
        }

        protected override void TranslateListShuffleInPlace(List<string> output, Expression list)
        {
            output.Add("shuffle(");
            this.Translator.TranslateExpression(output, list);
            output.Add("->r)");
        }

        protected override void TranslateMathLog(List<string> output, Expression value)
        {
            output.Add("log(");
            this.Translator.TranslateExpression(output, value);
            output.Add(")");
        }

        protected override void TranslateMultiplyList(List<string> output, Expression list, Expression num)
        {
            output.Add("pth_multiply_list(");
            this.PhpTranslator.TranslateExpression(output, list);
            output.Add(", ");
            this.PhpTranslator.TranslateExpression(output, num);
            output.Add(")");
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

        protected override void TranslateRandomFloat(List<string> output)
        {
            output.Add("pth_random_float()");
        }

        protected override void TranslateResourceGetManifest(List<string> output)
        {
            output.Add("''");
        }

        protected override void TranslateResourceReadText(List<string> output, Expression path)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateSetProgramData(List<string> output, Expression programData)
        {
            output.Add("pth_setProgramData(");
            this.PhpTranslator.TranslateExpression(output, programData);
            output.Add(")");
        }

        protected override void TranslateSin(List<string> output, Expression value)
        {
            throw new NotImplementedException();
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

        protected override void TranslateStringCompare(List<string> output, Expression a, Expression b)
        {
            output.Add("strcmp(");
            this.Translator.TranslateExpression(output, a);
            output.Add(", ");
            this.Translator.TranslateExpression(output, b);
            output.Add(")");
        }

        protected override void TranslateStringConcat(List<string> output, Expression[] values)
        {
            for (int i = 0; i < values.Length; ++i)
            {
                if (i > 0) output.Add(" . ");
                this.Translator.TranslateExpression(output, values[i]);
            }
        }

        protected override void TranslateStringContains(List<string> output, Expression haystack, Expression needle)
        {
            output.Add("pth_string_contains(");
            this.PhpTranslator.TranslateExpression(output, haystack);
            output.Add(", ");
            this.PhpTranslator.TranslateExpression(output, needle);
            output.Add(")");
        }

        protected override void TranslateStringEndsWith(List<string> output, Expression stringExpr, Expression findMe)
        {
            output.Add("pth_string_ends_with(");
            this.PhpTranslator.TranslateExpression(output, stringExpr);
            output.Add(", ");
            this.PhpTranslator.TranslateExpression(output, findMe);
            output.Add(")");
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

        protected override void TranslateStringLength(List<string> output, Expression stringValue)
        {
            output.Add("strlen(");
            this.Translator.TranslateExpression(output, stringValue);
            output.Add(")");
        }

        protected override void TranslateStringLower(List<string> output, Expression stringValue)
        {
            output.Add("strtolower(");
            this.Translator.TranslateExpression(output, stringValue);
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

        protected override void TranslateStringReplace(List<string> output, Expression stringValue, Expression findMe, Expression replaceWith)
        {
            output.Add("str_replace(");
            this.Translator.TranslateExpression(output, findMe);
            output.Add(", ");
            this.Translator.TranslateExpression(output, replaceWith);
            output.Add(", ");
            this.Translator.TranslateExpression(output, stringValue);
            output.Add(")");

        }

        protected override void TranslateStringReverse(List<string> output, Expression stringValue)
        {
            output.Add("str_reverse(");
            this.Translator.TranslateExpression(output, stringValue);
            output.Add(")");
        }

        protected override void TranslateStringSplit(List<string> output, Expression stringExpr, Expression sep)
        {
            output.Add("new Rf(explode(");
            this.Translator.TranslateExpression(output, sep);
            output.Add(", ");
            this.Translator.TranslateExpression(output, stringExpr);
            output.Add("))");
        }

        protected override void TranslateStringStartsWith(List<string> output, Expression stringExpr, Expression findMe)
        {
            output.Add("pth_string_starts_with(");
            this.PhpTranslator.TranslateExpression(output, stringExpr);
            output.Add(", ");
            this.PhpTranslator.TranslateExpression(output, findMe);
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

        protected override void TranslateStringTrim(List<string> output, Expression stringValue)
        {
            this.Translator.TranslateExpression(output, stringValue);
            output.Add(".trim()");
        }

        protected override void TranslateStringUpper(List<string> output, Expression stringValue)
        {
            output.Add("strtoupper(");
            this.Translator.TranslateExpression(output, stringValue);
            output.Add(")");
        }

        protected override void TranslateTan(List<string> output, Expression value)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateThreadSleep(List<string> output, Expression timeDelaySeconds)
        {
            // http://php.net/manual/en/function.time-nanosleep.php
            throw new NotImplementedException();
        }

        protected override void TranslateUnsafeFloatDivision(List<string> output, Expression numerator, Expression denominator)
        {
            output.Add("1.0 * ");
            this.Translator.TranslateExpression(output, numerator);
            output.Add(" / ");
            this.Translator.TranslateExpression(output, denominator);
        }

        protected override void TranslateUnsafeIntegerDivision(List<string> output, Expression numerator, Expression denominator)
        {
            output.Add("intval(");
            this.Translator.TranslateExpression(output, numerator);
            output.Add(" / ");
            this.Translator.TranslateExpression(output, denominator);
            output.Add(")");
        }
    }
}
