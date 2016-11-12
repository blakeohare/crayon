using System;
using Crayon.ParseTree;

namespace Crayon.Translator.Ruby
{
    internal class RubySystemFunctionTranslator : AbstractSystemFunctionTranslator
    {
        protected override void TranslateRandomFloat(System.Collections.Generic.List<string> output)
        {
            output.Add("rand()");
        }

        protected override void TranslateGetProgramData(System.Collections.Generic.List<string> output)
        {
            output.Add("$program_data");
        }

        protected override void TranslateByteCodeGetOps(System.Collections.Generic.List<string> output)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateCommandLineArgs(System.Collections.Generic.List<string> output)
        {
            output.Add("ARGV");
        }

        protected override void TranslateIsWindowsProgram(System.Collections.Generic.List<string> output)
        {
            output.Add("crayonHelper_isWindowsProgram()");
        }

        protected override void TranslateByteCodeGetIntArgs(System.Collections.Generic.List<string> output)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateResourceGetManifest(System.Collections.Generic.List<string> output)
        {
			output.Add("resourceHelper_getManifest()");
        }

        protected override void TranslateGetRawByteCodeString(System.Collections.Generic.List<string> output)
        {
            output.Add("resourceHelper_getByteCodeString");
        }

        protected override void TranslateByteCodeGetStringArgs(System.Collections.Generic.List<string> output)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateSin(System.Collections.Generic.List<string> output, Expression value)
        {
            output.Add("Math::sin(");
            this.Translator.TranslateExpression(output, value);
            output.Add(")");
        }

        protected override void TranslateTan(System.Collections.Generic.List<string> output, Expression value)
        {
            output.Add("Math::tan(");
            this.Translator.TranslateExpression(output, value);
            output.Add(")");
        }

        protected override void TranslateCos(System.Collections.Generic.List<string> output, Expression value)
        {
            output.Add("Math::cos(");
            this.Translator.TranslateExpression(output, value);
            output.Add(")");
        }

        protected override void TranslateInt(System.Collections.Generic.List<string> output, Expression value)
        {
            output.Add("(");
            this.Translator.TranslateExpression(output, value);
			output.Add(").to_i");
        }

        protected override void TranslateArcCos(System.Collections.Generic.List<string> output, Expression value)
        {
            output.Add("Math::acos(");
            this.Translator.TranslateExpression(output, value);
            output.Add(")");
        }

        protected override void TranslateArcSin(System.Collections.Generic.List<string> output, Expression value)
        {
            output.Add("Math::asin(");
            this.Translator.TranslateExpression(output, value);
            output.Add(")");
        }

        protected override void TranslateOrd(System.Collections.Generic.List<string> output, Expression character)
        {
            this.Translator.TranslateExpression(output, character);
            output.Add(".ord");
        }

        protected override void TranslateMathLog(System.Collections.Generic.List<string> output, Expression value)
        {
            output.Add("Math::log(");
            this.Translator.TranslateExpression(output, value);
            output.Add(")");
        }

        protected override void TranslateChr(System.Collections.Generic.List<string> output, Expression asciiValue)
        {
            output.Add("(");
            this.Translator.TranslateExpression(output, asciiValue);
            output.Add(").chr");
        }

        protected override void TranslateNewList(System.Collections.Generic.List<string> output, StringConstant type)
        {
            output.Add("Array.new");
        }

        protected override void TranslateParseInt(System.Collections.Generic.List<string> output, Expression rawString)
        {
            this.Translator.TranslateExpression(output, rawString);
            output.Add(".to_i");
        }

        protected override void TranslateStringParseInt(System.Collections.Generic.List<string> output, Expression value)
        {
            // TODO: why is there TranslateStringParseInt AND TranslateParseInt? They do the same thing in all platforms.
            this.Translator.TranslateExpression(output, value);
			output.Add(".to_i");
        }

        protected override void TranslateStringConcat(System.Collections.Generic.List<string> output, Expression[] values)
        {
            output.Add("[");
            for (int i = 0; i < values.Length; ++i)
            {
				if (i > 0) output.Add(", ");
                this.Translator.TranslateExpression(output, values[i]);
            }
            output.Add("].join()");
        }

        protected override void TranslateIsValidInteger(System.Collections.Generic.List<string> output, Expression number)
        {
            output.Add("crayonHelper_isValidInteger(");
            this.Translator.TranslateExpression(output, number);
            output.Add(")");
        }

        protected override void TranslateResourceReadText(System.Collections.Generic.List<string> output, Expression path)
        {
            output.Add("resourceHelper_readTextResource(");
            this.Translator.TranslateExpression(output, path);
            output.Add(")");
        }

        protected override void TranslateStringTrim(System.Collections.Generic.List<string> output, Expression stringValue)
        {
            this.Translator.TranslateExpression(output, stringValue);
            output.Add(".strip");
        }

        protected override void TranslateForceParens(System.Collections.Generic.List<string> output, Expression expression)
        {
            output.Add("(");
            this.Translator.TranslateExpression(output, expression);
            output.Add(")");
        }

        protected override void TranslateCharToString(System.Collections.Generic.List<string> output, Expression charValue)
        {
            this.Translator.TranslateExpression(output, charValue);
        }

        protected override void TranslateStringUpper(System.Collections.Generic.List<string> output, Expression stringValue)
        {
            this.Translator.TranslateExpression(output, stringValue);
            output.Add(".upcase");
        }

        protected override void TranslateStringLower(System.Collections.Generic.List<string> output, Expression stringValue)
        {
            this.Translator.TranslateExpression(output, stringValue);
            output.Add(".downcase");
        }

        protected override void TranslateStringLtrim(System.Collections.Generic.List<string> output, Expression stringValue)
        {
            this.Translator.TranslateExpression(output, stringValue);
            output.Add(".lstrip");
        }

        protected override void TranslateStringRtrim(System.Collections.Generic.List<string> output, Expression stringValue)
        {
            this.Translator.TranslateExpression(output, stringValue);
            output.Add(".rstrip");
        }

        protected override void TranslateComment(System.Collections.Generic.List<string> output, StringConstant commentValue)
        {
            output.Add("# ");
            output.Add(commentValue.Value);
        }

        protected override void TranslateStringReverse(System.Collections.Generic.List<string> output, Expression stringValue)
        {
            this.Translator.TranslateExpression(output, stringValue);
            output.Add(".reverse");
        }

        protected override void TranslateDictionarySize(System.Collections.Generic.List<string> output, Expression dictionary)
        {
            this.Translator.TranslateExpression(output, dictionary);
            output.Add(".length");
        }

        protected override void TranslateSortedCopyOfIntArray(System.Collections.Generic.List<string> output, Expression list)
        {
            output.Add("crayonHelper_sortedCopyOfArray(");
            this.Translator.TranslateExpression(output, list);
            output.Add(")");
        }

        protected override void TranslateSetProgramData(System.Collections.Generic.List<string> output, Expression programData)
        {
            output.Add("$program_data = ");
            this.Translator.TranslateExpression(output, programData);
        }

        protected override void TranslateThreadSleep(System.Collections.Generic.List<string> output, Expression timeDelaySeconds)
        {
            output.Add("sleep(");
            this.Translator.TranslateExpression(output, timeDelaySeconds);
            output.Add(")");
        }

        protected override void TranslatePrint(System.Collections.Generic.List<string> output, Expression expression, bool isErr)
        {
            if (isErr)
            {
                output.Add("STDERR.");
            }
            output.Add("puts ");
            this.Translator.TranslateExpression(output, expression);
        }

        protected override void TranslateStringFromCode(System.Collections.Generic.List<string> output, Expression characterCode)
        {
            this.Translator.TranslateExpression(output, characterCode);
            output.Add(".chr");
        }

        protected override void TranslateStringParseFloat(System.Collections.Generic.List<string> output, Expression stringValue)
        {
            this.Translator.TranslateExpression(output, stringValue);
            output.Add(".to_f");
        }

        protected override void TranslateSortedCopyOfStringArray(System.Collections.Generic.List<string> output, Expression list)
        {
            output.Add("crayonHelper_sortedCopyOfArray(");
            this.Translator.TranslateExpression(output, list);
            output.Add(")");
        }

        protected override void TranslateDictionaryGetValues(System.Collections.Generic.List<string> output, Expression dictionary)
        {
			this.Translator.TranslateExpression(output, dictionary);
			output.Add(".values");
        }

        protected override void TranslateStringAsChar(System.Collections.Generic.List<string> output, StringConstant stringConstant)
        {
            this.Translator.TranslateExpression(output, stringConstant);
        }

        protected override void TranslateStringCast(System.Collections.Generic.List<string> output, Expression thing, bool strongCast)
        {
			this.Translator.TranslateExpression(output, thing);
			output.Add(".to_s");
        }

        protected override void TranslateArcTan(System.Collections.Generic.List<string> output, Expression dy, Expression dx)
        {
			output.Add("Math::atan2(");
			this.Translator.TranslateExpression(output, dy);
			output.Add(", ");
			this.Translator.TranslateExpression(output, dx);
			output.Add(")");
        }

        protected override void TranslateDictionaryGetKeys(System.Collections.Generic.List<string> output, string keyType, Expression dictionary)
        {
			this.Translator.TranslateExpression(output, dictionary);
			output.Add(".keys");
        }

        protected override void TranslateMultiplyList(System.Collections.Generic.List<string> output, Expression list, Expression num)
        {
			this.Translator.TranslateExpression(output, list);
			output.Add(" * ");
			this.Translator.TranslateExpression(output, num);
        }

        protected override void TranslateIncrement(System.Collections.Generic.List<string> output, Expression expression, bool increment, bool prefix)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateNewArray(System.Collections.Generic.List<string> output, StringConstant type, Expression size)
        {
            output.Add("Array.new(");
            this.Translator.TranslateExpression(output, size);
            output.Add(")");
        }

        protected override void TranslateStringEquals(System.Collections.Generic.List<string> output, Expression aNonNull, Expression b)
        {
			this.Translator.TranslateExpression(output, aNonNull);
            output.Add(" == ");
			this.Translator.TranslateExpression(output, b);
        }

        protected override void TranslateDotEquals(System.Collections.Generic.List<string> output, Expression root, Expression compareTo)
        {
            output.Add(" = ");
        }

        protected override void TranslateExponent(System.Collections.Generic.List<string> output, Expression baseNum, Expression powerNum)
        {
            output.Add("(");
            this.Translator.TranslateExpression(output, baseNum);
            output.Add(") ** (");
            this.Translator.TranslateExpression(output, powerNum);
            output.Add(")");
        }

        protected override void TranslateStringSplit(System.Collections.Generic.List<string> output, Expression stringExpr, Expression sep)
        {
            this.Translator.TranslateExpression(output, stringExpr);
            output.Add(".split(");
            this.Translator.TranslateExpression(output, sep);
            output.Add(")");
        }

        protected override void TranslateStringCompareIsReverse(System.Collections.Generic.List<string> output, Expression a, Expression b)
        {
			output.Add("(");
			this.Translator.TranslateExpression(output, a);
			output.Add(" > ");
			this.Translator.TranslateExpression(output, b);
			output.Add(")");
        }

        protected override void TranslateCast(System.Collections.Generic.List<string> output, StringConstant typeValue, Expression expression)
        {
            this.Translator.TranslateExpression(output, expression);
        }

        protected override void TranslateParseFloat(System.Collections.Generic.List<string> output, Expression outParam, Expression rawString)
        {
            output.Add("_crayonHelper_parseFloat(");
            this.Translator.TranslateExpression(output, rawString);
            output.Add(", ");
            this.Translator.TranslateExpression(output, outParam);
            output.Add(")");
        }

        protected override void TranslateNewListOfSize(System.Collections.Generic.List<string> output, StringConstant type, Expression length)
        {
            this.TranslateNewArray(output, type, length);
        }

        protected override void TranslateStringCharAt(System.Collections.Generic.List<string> output, Expression stringValue, Expression index)
        {
            this.Translator.TranslateExpression(output, stringValue);
            output.Add("[");
            this.Translator.TranslateExpression(output, index);
            output.Add("]");
        }

        protected override void TranslateStringContains(System.Collections.Generic.List<string> output, Expression haystack, Expression needle)
        {
			this.Translator.TranslateExpression(output, haystack);
			output.Add(".include?(");
			this.Translator.TranslateExpression(output, needle);
			output.Add(")");
        }

        protected override void TranslateDictionaryRemove(System.Collections.Generic.List<string> output, Expression dictionary, Expression key)
        {
            this.Translator.TranslateExpression(output, dictionary);
            output.Add(".delete(");
            this.Translator.TranslateExpression(output, key);
            output.Add(")");
        }

        protected override void TranslateStringEndsWith(System.Collections.Generic.List<string> output, Expression stringExpr, Expression findMe)
        {
            this.Translator.TranslateExpression(output, stringExpr);
            output.Add(".end_with?(");
            this.Translator.TranslateExpression(output, findMe);
            output.Add(")");
        }

        protected override void TranslateConvertListToArray(System.Collections.Generic.List<string> output, StringConstant type, Expression list)
        {
			// TODO: determine if this really needs to be a copy. For now just do a clone.
			// Ideally there should be a naming convention here to indicate whether the side effect is mandatory.
			output.Add("(");
			this.Translator.TranslateExpression(output, list);
			output.Add(" + [])");
        }

        protected override void TranslateStringAppend(System.Collections.Generic.List<string> output, Expression target, Expression valueToAppend)
        {
			this.Translator.TranslateExpression(output, target);
			output.Add(" + ");
			this.Translator.TranslateExpression(output, valueToAppend);
        }

        protected override void TranslateStringStartsWith(System.Collections.Generic.List<string> output, Expression stringExpr, Expression findMe)
        {
            this.Translator.TranslateExpression(output, stringExpr);
            output.Add(".start_with?(");
            this.Translator.TranslateExpression(output, findMe);
            output.Add(")");
        }

        protected override void TranslateStringCharCodeAt(System.Collections.Generic.List<string> output, Expression stringValue, Expression index)
        {
			this.Translator.TranslateExpression(output, stringValue);
			output.Add("[");
			this.Translator.TranslateExpression(output, index);
			output.Add("].ord");
        }

        protected override void TranslateDictionaryGetGuaranteed(System.Collections.Generic.List<string> output, Expression dictionary, Expression key)
        {
            this.Translator.TranslateExpression(output, dictionary);
            output.Add("[");
            this.Translator.TranslateExpression(output, key);
            output.Add("]");
        }

        protected override void TranslateNewDictionary(System.Collections.Generic.List<string> output, StringConstant keyType, StringConstant valueType)
        {
            output.Add("{}");
        }

        protected override void TranslateCastToList(System.Collections.Generic.List<string> output, StringConstant typeValue, Expression enumerableThing)
        {
			this.Translator.TranslateExpression(output, enumerableThing);
        }

        protected override void TranslateUnsafeFloatDivision(System.Collections.Generic.List<string> output, Expression numerator, Expression denominator)
        {
            this.Translator.TranslateExpression(output, numerator);
            output.Add(" / ");
            this.Translator.TranslateExpression(output, denominator);
        }

        protected override void TranslateEnqueueVmResume(System.Collections.Generic.List<string> output, Expression seconds, Expression executionContextId)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateUnsafeIntegerDivision(System.Collections.Generic.List<string> output, Expression numerator, Expression denominator)
        {
            this.Translator.TranslateExpression(output, numerator);
            output.Add(" / ");
            this.Translator.TranslateExpression(output, denominator);
        }

        protected override void TranslateDictionarySet(System.Collections.Generic.List<string> output, Expression dictionary, Expression key, Expression value)
        {
            this.Translator.TranslateExpression(output, dictionary);
            output.Add("[");
            this.Translator.TranslateExpression(output, key);
            output.Add("] = ");
            this.Translator.TranslateExpression(output, value);
        }

        protected override void TranslateStringReplace(System.Collections.Generic.List<string> output, Expression stringValue, Expression findMe, Expression replaceWith)
        {
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(".split(");
			this.Translator.TranslateExpression(output, findMe);
			output.Add(").join(");
			this.Translator.TranslateExpression(output, replaceWith);
			output.Add(")");
        }

        protected override void TranslateStringIndexOf(System.Collections.Generic.List<string> output, Expression haystack, Expression needle, Expression optionalStartFrom)
        {
            if (optionalStartFrom != null)
            {
                output.Add("crayonHelper_findStringAfterIndex(");
                this.Translator.TranslateExpression(output, haystack);
                output.Add(", ");
                this.Translator.TranslateExpression(output, needle);
                output.Add(", ");
                this.Translator.TranslateExpression(output, optionalStartFrom);
                output.Add(")");
            }
            else
            {
                output.Add("(");
                this.Translator.TranslateExpression(output, haystack);
                output.Add(".index(");
                this.Translator.TranslateExpression(output, needle);
                output.Add(") || -1)");
            }
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
