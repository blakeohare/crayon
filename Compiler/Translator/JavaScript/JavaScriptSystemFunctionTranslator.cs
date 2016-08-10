using System;
using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon.Translator.JavaScript
{
    internal class JavaScriptSystemFunctionTranslator : AbstractSystemFunctionTranslator
    {
        protected override void TranslatePrint(List<string> output, Expression expression, bool isErr)
        {
            output.Add("C$common$print(");
            this.Translator.TranslateExpression(output, expression);
            output.Add(")");
        }

        protected override void TranslateResourceGetManifest(List<string> output)
        {
            output.Add("C$resourceManifest");
        }

        protected override void TranslateAppDataRoot(List<string> output)
        {
            output.Add("'/'");
        }

        protected override void TranslateAsyncMessageQueuePump(List<string> output)
        {
            output.Add("C$common$pumpAsyncMessageQueue()");
        }

        protected override void TranslateArcCos(List<string> output, Expression value)
        {
            output.Add("Math.acos(");
            this.Translator.TranslateExpression(output, value);
            output.Add(")");
        }

        protected override void TranslateArcSin(List<string> output, Expression value)
        {
            output.Add("Math.asin(");
            this.Translator.TranslateExpression(output, value);
            output.Add(")");
        }

        protected override void TranslateArcTan(List<string> output, Expression dy, Expression dx)
        {
            output.Add("Math.atan2(");
            this.Translator.TranslateExpression(output, dy);
            output.Add(", ");
            this.Translator.TranslateExpression(output, dx);
            output.Add(")");
        }

        protected override void TranslateArrayGet(List<string> output, Expression list, Expression index)
        {
            this.Translator.TranslateExpression(output, list);
            output.Add("[");
            this.Translator.TranslateExpression(output, index);
            output.Add("]");
        }

        protected override void TranslateArrayJoin(List<string> output, Expression array, Expression sep)
        {
            throw new NotImplementedException();
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
            output.Add(this.Shorten("] = "));
            this.Translator.TranslateExpression(output, value);
        }

        protected override void TranslateAssert(List<string> output, Expression message)
        {
            output.Add("throw ");
            this.Translator.TranslateExpression(output, message);
        }

        protected override void TranslateByteCodeGetIntArgs(List<string> output)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateByteCodeGetOps(List<string> output)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateByteCodeGetStringArgs(List<string> output)
        {
            throw new NotImplementedException();
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
            output.Add("String.fromCharCode(");
            this.Translator.TranslateExpression(output, asciiValue);
            output.Add(")");
        }

        protected override void TranslateComment(List<string> output, StringConstant commentValue)
        {
#if DEBUG
            if (!this.IsMin)
            {
                output.Add("// " + commentValue.Value);
            }
#endif
        }

        protected override void TranslateConvertListToArray(List<string> output, StringConstant type, Expression list)
        {
            this.Translator.TranslateExpression(output, list);
        }

        protected override void TranslateCos(List<string> output, Expression value)
        {
            output.Add("Math.cos(");
            this.Translator.TranslateExpression(output, value);
            output.Add(")");
        }

        protected override void TranslateCurrentTimeSeconds(List<string> output)
        {
            output.Add("C$common$now()");
        }

        protected override void TranslateDictionaryContains(List<string> output, Expression dictionary, Expression key)
        {
            output.Add("(");
            this.Translator.TranslateExpression(output, dictionary);
            output.Add("[");
            this.Translator.TranslateExpression(output, key);
            output.Add(this.Shorten("] !== undefined)"));
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
            output.Add("C$common$dictionaryKeys(");
            this.Translator.TranslateExpression(output, dictionary);
            output.Add(")");
        }

        protected override void TranslateDictionaryGetValues(List<string> output, Expression dictionary)
        {
            output.Add("C$common$dictionaryValues(");
            this.Translator.TranslateExpression(output, dictionary);
            output.Add(")");
        }

        protected override void TranslateDictionaryRemove(List<string> output, Expression dictionary, Expression key)
        {
            output.Add("delete ");
            this.Translator.TranslateExpression(output, dictionary);
            output.Add("[");
            this.Translator.TranslateExpression(output, key);
            output.Add("]");
        }

        protected override void TranslateDictionarySet(List<string> output, Expression dictionary, Expression key, Expression value)
        {
            this.Translator.TranslateExpression(output, dictionary);
            output.Add("[");
            this.Translator.TranslateExpression(output, key);
            output.Add(this.Shorten("] = "));
            this.Translator.TranslateExpression(output, value);
        }

        protected override void TranslateDictionarySize(List<string> output, Expression dictionary)
        {
            output.Add("Object.keys(");
            this.Translator.TranslateExpression(output, dictionary);
            output.Add(").length");
        }

        protected override void TranslateDotEquals(List<string> output, Expression root, Expression compareTo)
        {
            throw new Exception("This should have been optimized out.");
        }

        protected override void TranslateEnqueueVmResume(List<string> output, Expression seconds, Expression executionContextId)
        {
            output.Add("C$common$enqueueVmResume(");
            this.Translator.TranslateExpression(output, seconds);
            output.Add(", ");
            this.Translator.TranslateExpression(output, executionContextId);
            output.Add(")");
        }

        protected override void TranslateExponent(List<string> output, Expression baseNum, Expression powerNum)
        {
            output.Add("Math.pow(");
            this.Translator.TranslateExpression(output, baseNum);
            output.Add(this.Shorten(", "));
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
            output.Add("C$common$programData");
        }

        protected override void TranslateGetRawByteCodeString(List<string> output)
        {
            output.Add("C$bytecode");
        }

        protected override void TranslateIncrement(List<string> output, Expression expression, bool increment, bool prefix)
        {
            string op = increment ? "++" : "--";
            if (prefix) output.Add(op);
            this.Translator.TranslateExpression(output, expression);
            if (!prefix) output.Add(op);
        }

        protected override void TranslateInt(List<string> output, Expression value)
        {
            output.Add("Math.floor(");
            this.Translator.TranslateExpression(output, value);
            output.Add(")");
        }

        protected override void TranslateIsValidInteger(List<string> output, Expression number)
        {
            output.Add("C$common$is_valid_integer(");
            this.Translator.TranslateExpression(output, number);
            output.Add(")");
        }

        protected override void TranslateIsWindowsProgram(List<string> output)
        {
            // TODO: make platforms that have a constant value for this also have a
            // %%% platform parameter that can be used to short circuit interpreter code.
            output.Add("false");
        }

        protected override void TranslateListClear(List<string> output, Expression list)
        {
            output.Add("C$common$clearList(");
            this.Translator.TranslateExpression(output, list);
            output.Add(")");
        }

        protected override void TranslateListConcat(List<string> output, Expression listA, Expression listB)
        {
            this.Translator.TranslateExpression(output, listA);
            output.Add(".concat(");
            this.Translator.TranslateExpression(output, listB);
            output.Add(")");
        }

        protected override void TranslateListGet(List<string> output, Expression list, Expression index)
        {
            this.Translator.TranslateExpression(output, list);
            output.Add("[");
            this.Translator.TranslateExpression(output, index);
            output.Add("]");
        }

        protected override void TranslateListInsert(List<string> output, Expression list, Expression index, Expression value)
        {
            this.Translator.TranslateExpression(output, list);
            output.Add(".splice(");
            this.Translator.TranslateExpression(output, index);
            output.Add(this.Shorten(", 0, "));
            this.Translator.TranslateExpression(output, value);
            output.Add(")");
        }

        protected override void TranslateListJoin(List<string> output, Expression list, Expression sep)
        {
            this.Translator.TranslateExpression(output, list);
            output.Add(".join(");
            this.Translator.TranslateExpression(output, sep);
            output.Add(")");
        }

        protected override void TranslateListJoinChars(List<string> output, Expression list)
        {
            this.Translator.TranslateExpression(output, list);
            output.Add(".join('')");
        }

        protected override void TranslateListLastIndex(List<string> output, Expression list)
        {
            this.Translator.TranslateExpression(output, list);
            output.Add(this.Shorten(".length - 1"));
        }

        protected override void TranslateListLength(List<string> output, Expression list)
        {
            output.Add("(");
            this.Translator.TranslateExpression(output, list);
            output.Add(").length");
        }

        protected override void TranslateListPop(List<string> output, Expression list)
        {
            this.Translator.TranslateExpression(output, list);
            output.Add(".pop()");
        }

        protected override void TranslateListPush(List<string> output, Expression list, Expression value)
        {
            this.Translator.TranslateExpression(output, list);
            output.Add(".push(");
            this.Translator.TranslateExpression(output, value);
            output.Add(")");
        }

        protected override void TranslateListRemoveAt(List<string> output, Expression list, Expression index)
        {
            this.Translator.TranslateExpression(output, list);
            output.Add(".splice(");
            this.Translator.TranslateExpression(output, index);
            output.Add(this.Shorten(", 1)"));
        }

        protected override void TranslateListReverseInPlace(List<string> output, Expression list)
        {
            this.Translator.TranslateExpression(output, list);
            output.Add(".reverse()");
        }

        protected override void TranslateListSet(List<string> output, Expression list, Expression index, Expression value)
        {
            this.Translator.TranslateExpression(output, list);
            output.Add("[");
            this.Translator.TranslateExpression(output, index);
            output.Add(this.Shorten("] = "));
            this.Translator.TranslateExpression(output, value);
        }

        protected override void TranslateListShuffleInPlace(List<string> output, Expression list)
        {
            output.Add("C$common$shuffle(");
            this.Translator.TranslateExpression(output, list);
            output.Add(")");
        }

        protected override void TranslateMathLog(List<string> output, Expression value)
        {
            output.Add("Math.log(");
            this.Translator.TranslateExpression(output, value);
            output.Add(")");
        }

        protected override void TranslateMultiplyList(List<string> output, Expression list, Expression num)
        {
            output.Add("C$common$multiplyList(");
            this.Translator.TranslateExpression(output, list);
            output.Add(this.Shorten(", "));
            this.Translator.TranslateExpression(output, num);
            output.Add(")");
        }

        protected override void TranslateNewArray(List<string> output, StringConstant type, Expression size)
        {
            this.TranslateNewListOfSize(output, type, size);
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
            if (length is IntegerConstant)
            {
                int literalLength = ((IntegerConstant)length).Value;
                switch (literalLength)
                {
                    case 0: output.Add("[]"); return;
                    case 1: output.Add("[null]"); return;
                    case 2: output.Add("[null, null]"); return;
                    case 3: output.Add("[null, null, null]"); return;
                    default: break;
                }
            }
            output.Add("C$common$createNewArray(");
            this.Translator.TranslateExpression(output, length);
            output.Add(")");
        }

        protected override void TranslateOrd(List<string> output, Expression character)
        {
            this.Translator.TranslateExpression(output, character);
            output.Add(".charCodeAt(0)");
        }

        protected override void TranslateParseFloat(List<string> output, Expression outParam, Expression rawString)
        {
            output.Add("C$common$floatParseHelper(");
            this.Translator.TranslateExpression(output, outParam);
            output.Add(", ");
            this.Translator.TranslateExpression(output, rawString);
            output.Add(")");
        }

        protected override void TranslateParseInt(List<string> output, Expression rawString)
        {
            output.Add("parseInt(");
            this.Translator.TranslateExpression(output, rawString);
            output.Add(")");
        }

        protected override void TranslateRandomFloat(List<string> output)
        {
            output.Add("Math.random()");
        }

        protected override void TranslateResourceReadText(List<string> output, Expression path)
        {
            output.Add("C$common$getTextRes('text/' + ");
            this.Translator.TranslateExpression(output, path);
            output.Add(")");
        }

        protected override void TranslateSetProgramData(List<string> output, Expression programData)
        {
            output.Add("C$common$programData = ");
            this.Translator.TranslateExpression(output, programData);
        }

        protected override void TranslateSin(List<string> output, Expression value)
        {
            output.Add("Math.sin(");
            this.Translator.TranslateExpression(output, value);
            output.Add(")");
        }

        protected override void TranslateSortedCopyOfIntArray(List<string> output, Expression list)
        {
            output.Add("C$common$sortedCopyOfArray(");
            this.Translator.TranslateExpression(output, list);
            output.Add(")");
        }

        protected override void TranslateSortedCopyOfStringArray(List<string> output, Expression list)
        {
            output.Add("C$common$sortedCopyOfArray(");
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
            if (strongCast)
            {
                output.Add(this.Shorten("('' + "));
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

        protected override void TranslateStringCharCodeAt(List<string> output, Expression stringValue, Expression index)
        {
            this.Translator.TranslateExpression(output, stringValue);
            output.Add(".charCodeAt(");
            this.Translator.TranslateExpression(output, index);
            output.Add(")");
        }

        protected override void TranslateStringCompare(List<string> output, Expression a, Expression b)
        {
            this.Translator.TranslateExpression(output, a);
            output.Add(".localeCompare(");
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

        protected override void TranslateStringContains(List<string> output, Expression haystack, Expression needle)
        {
            output.Add("(");
            this.Translator.TranslateExpression(output, haystack);
            output.Add(".indexOf(");
            this.Translator.TranslateExpression(output, needle);
            output.Add(this.Shorten(") != -1)"));
        }

        protected override void TranslateStringEndsWith(List<string> output, Expression stringExpr, Expression findMe)
        {
            output.Add("C$common$stringEndsWith(");
            this.Translator.TranslateExpression(output, stringExpr);
            output.Add(this.Shorten(", "));
            this.Translator.TranslateExpression(output, findMe);
            output.Add(")");
        }

        protected override void TranslateStringEquals(List<string> output, Expression aNonNull, Expression b)
        {
            this.Translator.TranslateExpression(output, aNonNull);
            output.Add(" == ");
            this.Translator.TranslateExpression(output, b);
        }

        protected override void TranslateStringFromCode(List<string> output, Expression characterCode)
        {
            output.Add("String.fromCharCode(");
            this.Translator.TranslateExpression(output, characterCode);
            output.Add(")");
        }

        protected override void TranslateStringIndexOf(List<string> output, Expression haystack, Expression needle, Expression optionalStartFrom)
        {
            this.Translator.TranslateExpression(output, haystack);
            output.Add(".indexOf(");
            this.Translator.TranslateExpression(output, needle);
            if (optionalStartFrom != null)
            {
                output.Add(", ");
                this.Translator.TranslateExpression(output, optionalStartFrom);
            }
            output.Add(")");
        }

        protected override void TranslateStringLength(List<string> output, Expression stringValue)
        {
            this.Translator.TranslateExpression(output, stringValue);
            output.Add(".length");
        }

        protected override void TranslateStringLower(List<string> output, Expression stringValue)
        {
            this.Translator.TranslateExpression(output, stringValue);
            output.Add(".toLowerCase()");
        }

        protected override void TranslateStringParseFloat(List<string> output, Expression stringValue)
        {
            output.Add("parseFloat(");
            this.Translator.TranslateExpression(output, stringValue);
            output.Add(")");
        }

        protected override void TranslateStringParseInt(List<string> output, Expression value)
        {
            output.Add("parseInt(");
            this.Translator.TranslateExpression(output, value);
            output.Add(")");
        }

        protected override void TranslateStringReplace(List<string> output, Expression stringValue, Expression findMe, Expression replaceWith)
        {
            this.Translator.TranslateExpression(output, stringValue);
            output.Add(".split(");
            this.Translator.TranslateExpression(output, findMe);
            output.Add(").join(");
            this.Translator.TranslateExpression(output, replaceWith);
            output.Add(")");
        }

        protected override void TranslateStringReverse(List<string> output, Expression stringValue)
        {
            this.Translator.TranslateExpression(output, stringValue);
            output.Add(".split('').reverse().join('')");
        }

        protected override void TranslateStringSplit(List<string> output, Expression stringExpr, Expression sep)
        {
            this.Translator.TranslateExpression(output, stringExpr);
            output.Add(".split(");
            this.Translator.TranslateExpression(output, sep);
            output.Add(")");
        }

        protected override void TranslateStringStartsWith(List<string> output, Expression stringExpr, Expression findMe)
        {
            output.Add("(");
            this.Translator.TranslateExpression(output, stringExpr);
            output.Add(".indexOf(");
            this.Translator.TranslateExpression(output, findMe);
            output.Add(this.Shorten(") == 0)"));
        }

        protected override void TranslateStringSubstring(List<string> output, Expression stringExpr, Expression startIndex, Expression optionalLength)
        {
            output.Add("C$common$substring(");
            this.Translator.TranslateExpression(output, stringExpr);
            output.Add(", ");
            this.Translator.TranslateExpression(output, startIndex);
            output.Add(", ");
            if (optionalLength != null)
            {
                this.Translator.TranslateExpression(output, optionalLength);
            }
            else
            {
                output.Add("null");
            }
            output.Add(")");
        }

        protected override void TranslateStringSubstringExistsAt(List<string> output, Expression stringExpr, Expression lookFor, Expression index)
        {
            output.Add("C$common$checksubstring(");
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
            this.Translator.TranslateExpression(output, stringValue);
            output.Add(".toUpperCase()");
        }

        protected override void TranslateTan(List<string> output, Expression value)
        {
            output.Add("Math.tan(");
            this.Translator.TranslateExpression(output, value);
            output.Add(")");
        }

        protected override void TranslateThreadSleep(List<string> output, Expression timeDelaySeconds)
        {
            throw new InvalidOperationException(); // Optimized out.
        }

        protected override void TranslateUnsafeFloatDivision(List<string> output, Expression numerator, Expression denominator)
        {
            this.Translator.TranslateExpression(output, numerator);
            output.Add(this.Shorten(" / "));
            this.Translator.TranslateExpression(output, denominator);
        }

        protected override void TranslateUnsafeIntegerDivision(List<string> output, Expression numerator, Expression denominator)
        {
            output.Add("Math.floor(");
            this.Translator.TranslateExpression(output, numerator);
            output.Add(this.Shorten(" / "));
            this.Translator.TranslateExpression(output, denominator);
            output.Add(")");
        }
    }
}
