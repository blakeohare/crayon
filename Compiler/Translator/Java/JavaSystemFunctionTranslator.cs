using System;
using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon.Translator.Java
{
    internal abstract class JavaSystemFunctionTranslator : AbstractSystemFunctionTranslator
    {
        public JavaPlatform JavaPlatform { get { return (JavaPlatform)this.Platform; } }

        protected override void TranslateAsyncMessageQueuePump(List<string> output)
        {
            output.Add("AsyncMessageQueue.pumpMessages()");
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
            output.Add("] = ");
            this.Translator.TranslateExpression(output, value);
        }

        protected override void TranslateAssert(List<string> output, Expression message)
        {
            output.Add("TranslationHelper.assertion(");
            this.Translator.TranslateExpression(output, message);
            output.Add(")");
        }

        protected override void TranslateBeginFrame(List<string> output)
        {
            // Nope
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
            output.Add("((");
            output.Add(this.JavaPlatform.GetTypeStringFromString(typeValue.Value, false, false));
            output.Add(") ");
            this.Translator.TranslateExpression(output, expression);
            output.Add(")");
        }

        protected override void TranslateCastToList(List<string> output, StringConstant typeValue, Expression enumerableThing)
        {
            output.Add("new ArrayList<");
            output.Add(this.JavaPlatform.GetTypeStringFromString(typeValue.Value, false, false));
            output.Add(">(");
            this.Translator.TranslateExpression(output, enumerableThing);
            output.Add(")");
        }

        protected override void TranslateCharToString(List<string> output, Expression charValue)
        {
            output.Add("\"\" + ");
            this.Translator.TranslateExpression(output, charValue);
        }

        protected override void TranslateChr(List<string> output, Expression asciiValue)
        {
            output.Add("Character.toString((char) ");
            this.Translator.TranslateExpression(output, asciiValue);
            output.Add(")");
        }

        protected override void TranslateComment(List<string> output, StringConstant commentValue)
        {
#if DEBUG
            output.Add("// " + commentValue.Value);
#endif
        }

        protected override void TranslateConvertListToArray(List<string> output, StringConstant type, Expression list)
        {
            string typeString = this.JavaPlatform.GetTypeStringFromString(type.Value, false, true);
            if (typeString == "int")
            {
                output.Add("TranslationHelper.createIntArray(");
                this.Translator.TranslateExpression(output, list);
                output.Add(")");
            }
            else
            {
                this.Translator.TranslateExpression(output, list);
                output.Add(".toArray(");
                List<string> sizeValue = new List<string>();
                this.Translator.TranslateExpression(sizeValue, list);
                sizeValue.Add(".size()");

                this.CreateNewArrayOfSize(output, typeString, string.Join("", sizeValue));
                output.Add(")");
            }
        }

        protected override void TranslateCos(List<string> output, Expression value)
        {
            output.Add("Math.cos(");
            this.Translator.TranslateExpression(output, value);
            output.Add(")");
        }

        protected override void TranslateCurrentTimeSeconds(List<string> output)
        {
            output.Add("System.currentTimeMillis() / 1000.0");
        }

        protected override void TranslateDictionaryContains(List<string> output, Expression dictionary, Expression key)
        {
            this.Translator.TranslateExpression(output, dictionary);
            output.Add(".containsKey(");
            this.Translator.TranslateExpression(output, key);
            output.Add(")");
        }

        protected override void TranslateDictionaryGetGuaranteed(List<string> output, Expression dictionary, Expression key)
        {
            this.Translator.TranslateExpression(output, dictionary);
            output.Add(".get(");
            this.Translator.TranslateExpression(output, key);
            output.Add(")");
        }

        protected override void TranslateDictionaryGetKeys(List<string> output, string keyType, Expression dictionary)
        {
            string functionName;
            switch (keyType)
            {
                case "int": functionName = "Integer"; break;
                case "string": functionName = "String"; break;
                default: throw new Exception("If you see this exception, you may want to consider a more extensible way of doing this.");
            }
            output.Add("TranslationHelper.convert" + functionName + "SetToArray(");
            this.Translator.TranslateExpression(output, dictionary);
            output.Add(".keySet())");
        }

        protected override void TranslateDictionaryGetValues(List<string> output, Expression dictionary)
        {
            this.Translator.TranslateExpression(output, dictionary);
            output.Add(".values()");
        }

        protected override void TranslateDictionaryRemove(List<string> output, Expression dictionary, Expression key)
        {
            this.Translator.TranslateExpression(output, dictionary);
            output.Add(".remove(");
            this.Translator.TranslateExpression(output, key);
            output.Add(")");
        }

        protected override void TranslateDictionarySet(List<string> output, Expression dictionary, Expression key, Expression value)
        {
            this.Translator.TranslateExpression(output, dictionary);
            output.Add(".put(");
            this.Translator.TranslateExpression(output, key);
            output.Add(", ");
            this.Translator.TranslateExpression(output, value);
            output.Add(")");
        }

        protected override void TranslateDictionarySize(List<string> output, Expression dictionary)
        {
            this.Translator.TranslateExpression(output, dictionary);
            output.Add(".size()");
        }

        protected override void TranslateDotEquals(List<string> output, Expression root, Expression compareTo)
        {
            this.Translator.TranslateExpression(output, root);
            output.Add(".equals(");
            this.Translator.TranslateExpression(output, compareTo);
            output.Add(")");
        }

        protected override void TranslateEnqueueVmResume(List<string> output, Expression seconds, Expression executionContextId)
        {
            throw new InvalidOperationException(); // optimized out.
        }

        protected override void TranslateExponent(List<string> output, Expression baseNum, Expression powerNum)
        {
            output.Add("Math.pow(");
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
            output.Add("TranslationHelper.getProgramData()");
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
            output.Add("((int)");
            this.Translator.TranslateExpression(output, value);
            output.Add(")");
        }

        protected override void TranslateIsValidInteger(List<string> output, Expression number)
        {
            output.Add("TranslationHelper.isValidInteger(");
            this.Translator.TranslateExpression(output, number);
            output.Add(")"); // meh
        }

        protected override void TranslateListClear(List<string> output, Expression list)
        {
            this.Translator.TranslateExpression(output, list);
            output.Add(".clear()");
        }

        protected override void TranslateListConcat(List<string> output, Expression listA, Expression listB)
        {
            output.Add("TranslationHelper.concatLists(");
            this.Translator.TranslateExpression(output, listA);
            output.Add(", ");
            this.Translator.TranslateExpression(output, listB);
            output.Add(")");
        }

        protected override void TranslateListGet(List<string> output, Expression list, Expression index)
        {
            this.Translator.TranslateExpression(output, list);
            output.Add(".get(");
            this.Translator.TranslateExpression(output, index);
            output.Add(")");
        }

        protected override void TranslateListInsert(List<string> output, Expression list, Expression index, Expression value)
        {
            this.Translator.TranslateExpression(output, list);
            output.Add(".add(");
            this.Translator.TranslateExpression(output, index);
            output.Add(", ");
            this.Translator.TranslateExpression(output, value);
            output.Add(")");
        }

        protected override void TranslateListJoin(List<string> output, Expression list, Expression sep)
        {
            output.Add("TranslationHelper.joinList(");
            this.Translator.TranslateExpression(output, sep);
            output.Add(", ");
            this.Translator.TranslateExpression(output, list);
            output.Add(")");
        }

        protected override void TranslateListJoinChars(List<string> output, Expression list)
        {
            output.Add("TranslationHelper.joinChars(");
            this.Translator.TranslateExpression(output, list);
            output.Add(")");
        }

        protected override void TranslateListLastIndex(List<string> output, Expression list)
        {
            this.Translator.TranslateExpression(output, list);
            output.Add(".size() - 1");
        }

        protected override void TranslateListLength(List<string> output, Expression list)
        {
            this.Translator.TranslateExpression(output, list);
            output.Add(".size()");
        }

        protected override void TranslateListPop(List<string> output, Expression list)
        {
            this.Translator.TranslateExpression(output, list);
            output.Add(".remove(");
            this.Translator.TranslateExpression(output, list);
            output.Add(".size() - 1)");
        }

        protected override void TranslateListPush(List<string> output, Expression list, Expression value)
        {
            this.Translator.TranslateExpression(output, list);
            output.Add(".add(");
            this.Translator.TranslateExpression(output, value);
            output.Add(")");
        }

        protected override void TranslateListRemoveAt(List<string> output, Expression list, Expression index)
        {
            this.Translator.TranslateExpression(output, list);
            output.Add(".remove(");
            this.Translator.TranslateExpression(output, index);
            output.Add(")");
        }

        protected override void TranslateListReverseInPlace(List<string> output, Expression list)
        {
            output.Add("TranslationHelper.reverseList(");
            this.Translator.TranslateExpression(output, list);
            output.Add(")");
        }

        protected override void TranslateListSet(List<string> output, Expression list, Expression index, Expression value)
        {
            this.Translator.TranslateExpression(output, list);
            output.Add(".set(");
            this.Translator.TranslateExpression(output, index);
            output.Add(", ");
            this.Translator.TranslateExpression(output, value);
            output.Add(")");
        }

        protected override void TranslateListShuffleInPlace(List<string> output, Expression list)
        {
            output.Add("TranslationHelper.shuffleInPlace(");
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
            output.Add("TranslationHelper.multiplyList(");
            this.Translator.TranslateExpression(output, list);
            output.Add(", ");
            this.Translator.TranslateExpression(output, num);
            output.Add(")");
        }

        protected override void TranslateNewArray(List<string> output, StringConstant type, Expression size)
        {
            string javaType = this.JavaPlatform.GetTypeStringFromAnnotation(type.FirstToken, type.Value, false, true);
            List<string> sizeValue = new List<string>();
            this.Translator.TranslateExpression(sizeValue, size);
            CreateNewArrayOfSize(output, javaType, string.Join("", sizeValue));
        }

        protected override void TranslateNewDictionary(List<string> output, StringConstant keyType, StringConstant valueType)
        {
            output.Add("new HashMap<");
            output.Add(this.JavaPlatform.GetTypeStringFromString(keyType.Value, true, false));
            output.Add(", ");
            output.Add(this.JavaPlatform.GetTypeStringFromString(valueType.Value, true, false));
            output.Add(">()");
        }

        protected override void TranslateNewList(List<string> output, StringConstant type)
        {
            output.Add("new ArrayList<");
            output.Add(this.JavaPlatform.GetTypeStringFromString(type.Value, true, false));
            output.Add(">()");
        }

        protected override void TranslateNewListOfSize(List<string> output, StringConstant type, Expression length)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateOrd(List<string> output, Expression character)
        {
            output.Add("((int) ");
            this.Translator.TranslateExpression(output, character);
            output.Add(".charAt(0))");
        }

        protected override void TranslateParseFloat(List<string> output, Expression outParam, Expression rawString)
        {
            output.Add("TranslationHelper.parseFloatOrReturnNull(");
            this.Translator.TranslateExpression(output, outParam);
            output.Add(", ");
            this.Translator.TranslateExpression(output, rawString);
            output.Add(")");
        }

        protected override void TranslateParseInt(List<string> output, Expression rawString)
        {
            output.Add("Integer.parseInt(");
            this.Translator.TranslateExpression(output, rawString);
            output.Add(")");
        }

        protected override void TranslateRandomFloat(List<string> output)
        {
            output.Add("TranslationHelper.random.nextDouble()");
        }

        protected override void TranslateSetProgramData(List<string> output, Expression programData)
        {
            output.Add("TranslationHelper.setProgramData(");
            this.Translator.TranslateExpression(output, programData);
            output.Add(")");
        }

        protected override void TranslateSin(List<string> output, Expression value)
        {
            output.Add("Math.sin(");
            this.Translator.TranslateExpression(output, value);
            output.Add(")");
        }

        protected override void TranslateSortedCopyOfIntArray(List<string> output, Expression list)
        {
            output.Add("TranslationHelper.sortedCopyOfIntArray(");
            this.Translator.TranslateExpression(output, list);
            output.Add(")");
        }

        protected override void TranslateSortedCopyOfStringArray(List<string> output, Expression list)
        {
            output.Add("TranslationHelper.sortedCopyOfStringArray(");
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
            char c = stringConstant.Value[0];
            string value;
            switch (c)
            {
                case '\'': value = "'\\''"; break;
                case '\\': value = "'\\\\'"; break;
                case '\n': value = "'\\n'"; break;
                case '\r': value = "'\\r'"; break;
                case '\0': value = "'\\0'"; break;
                case '\t': value = "'\\t'"; break;
                default: value = "'" + c + "'"; break;
            }
            output.Add(value);
        }

        protected override void TranslateStringCast(List<string> output, Expression thing, bool strongCast)
        {
            if (strongCast)
            {
                output.Add("(\"\" + ");
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
            output.Add(".charAt(");
            this.Translator.TranslateExpression(output, index);
            output.Add(")");
        }

        protected override void TranslateStringCompare(List<string> output, Expression a, Expression b)
        {
            this.Translator.TranslateExpression(output, a);
            output.Add(".compareTo(");
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
            this.Translator.TranslateExpression(output, haystack);
            output.Add(".contains(");
            this.Translator.TranslateExpression(output, needle);
            output.Add(")");
        }

        protected override void TranslateStringEndsWith(List<string> output, Expression stringExpr, Expression findMe)
        {
            this.Translator.TranslateExpression(output, stringExpr);
            output.Add(".endsWith(");
            this.Translator.TranslateExpression(output, findMe);
            output.Add(")");
        }

        protected override void TranslateStringEquals(List<string> output, Expression aNonNull, Expression b)
        {
            this.Translator.TranslateExpression(output, aNonNull);
            output.Add(".equals(");
            this.Translator.TranslateExpression(output, b);
            output.Add(")");
        }

        protected override void TranslateStringFromCode(List<string> output, Expression characterCode)
        {
            output.Add("Character.toString((char)");
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
            output.Add(".length()");
        }

        protected override void TranslateStringLower(List<string> output, Expression stringValue)
        {
            this.Translator.TranslateExpression(output, stringValue);
            output.Add(".toLowerCase()");
        }

        protected override void TranslateStringParseFloat(List<string> output, Expression stringValue)
        {
            output.Add("Double.parseDouble(");
            this.Translator.TranslateExpression(output, stringValue);
            output.Add(")");
        }

        protected override void TranslateStringParseInt(List<string> output, Expression value)
        {
            output.Add("Integer.parseInt(");
            this.Translator.TranslateExpression(output, value);
            output.Add(")");
        }

        protected override void TranslateStringReplace(List<string> output, Expression stringValue, Expression findMe, Expression replaceWith)
        {
            this.Translator.TranslateExpression(output, stringValue);
            output.Add(".replace((CharSequence)");
            this.Translator.TranslateExpression(output, findMe);
            output.Add(", (CharSequence)");
            this.Translator.TranslateExpression(output, replaceWith);
            output.Add(")");
        }

        protected override void TranslateStringReverse(List<string> output, Expression stringValue)
        {
            output.Add("TranslationHelper.reverseString(");
            this.Translator.TranslateExpression(output, stringValue);
            output.Add(")");
        }

        protected override void TranslateStringSplit(List<string> output, Expression stringExpr, Expression sep)
        {
            output.Add("TranslationHelper.literalStringSplit(");
            this.Translator.TranslateExpression(output, stringExpr);
            output.Add(", ");
            this.Translator.TranslateExpression(output, sep);
            output.Add(")");
        }

        protected override void TranslateStringStartsWith(List<string> output, Expression stringExpr, Expression findMe)
        {
            this.Translator.TranslateExpression(output, stringExpr);
            output.Add(".startsWith(");
            this.Translator.TranslateExpression(output, findMe);
            output.Add(")");
        }

        protected override void TranslateStringSubstring(List<string> output, Expression stringExpr, Expression startIndex, Expression optionalLength)
        {
            output.Add("TranslationHelper.substring(");
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
            output.Add("TranslationHelper.checkStringSlice(");
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
            output.Add("TranslationHelper.sleep(");
            this.Translator.TranslateExpression(output, timeDelaySeconds);
            output.Add(")");
        }

        protected override void TranslateUnsafeFloatDivision(List<string> output, Expression numerator, Expression denominator)
        {
            this.Translator.TranslateExpression(output, numerator);
            output.Add(" / ");
            this.Translator.TranslateExpression(output, denominator);
        }

        protected override void TranslateUnsafeIntegerDivision(List<string> output, Expression numerator, Expression denominator)
        {
            this.Translator.TranslateExpression(output, numerator);
            output.Add(" / ");
            this.Translator.TranslateExpression(output, denominator);
        }

        private void CreateNewArrayOfSize(List<string> output, string rawType, string size)
        {
            output.Add("new ");
            // Delightful hack...
            int padding = 0;
            while (rawType.EndsWith("[]"))
            {
                padding++;
                rawType = rawType.Substring(0, rawType.Length - 2);
            }
            output.Add(rawType);
            output.Add("[");
            output.Add(size);
            output.Add("]");
            while (padding-- > 0)
            {
                output.Add("[]");
            }
        }
    }
}
