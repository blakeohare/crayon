using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Platform;
using Pastel.Nodes;
using Common;

namespace LangJavaScript
{
    public class JavaScriptTranslator : CurlyBraceTranslator
    {
        public JavaScriptTranslator(AbstractPlatform platform) : base(platform, "\t", "\n", true)
        { }

        public override void TranslateArrayGet(StringBuilder sb, Expression array, Expression index)
        {
            this.TranslateExpression(sb, array);
            sb.Append('[');
            this.TranslateExpression(sb, index);
            sb.Append(']');
        }

        public override void TranslateArrayJoin(StringBuilder sb, Expression array, Expression sep)
        {
            throw new NotImplementedException();
        }

        public override void TranslateArrayLength(StringBuilder sb, Expression array)
        {
            this.TranslateExpression(sb, array);
            sb.Append(".length");
        }

        public override void TranslateArrayNew(StringBuilder sb, PType arrayType, Expression lengthExpression)
        {
            sb.Append("C$common$createNewArray(");
            this.TranslateExpression(sb, lengthExpression);
            sb.Append(')');
        }

        public override void TranslateArraySet(StringBuilder sb, Expression array, Expression index, Expression value)
        {
            this.TranslateExpression(sb, array);
            sb.Append('[');
            this.TranslateExpression(sb, index);
            sb.Append("] = ");
            this.TranslateExpression(sb, value);
        }

        public override void TranslateBase64ToString(StringBuilder sb, Expression base64String)
        {
            sb.Append("C$common$base64ToString(");
            this.TranslateExpression(sb, base64String);
            sb.Append(')');
        }

        public override void TranslateCast(StringBuilder sb, PType type, Expression expression)
        {
            this.TranslateExpression(sb, expression);
        }

        public override void TranslateCharConstant(StringBuilder sb, char value)
        {
            sb.Append(Util.ConvertStringValueToCode(value.ToString()));
        }

        public override void TranslateCharToString(StringBuilder sb, Expression charValue)
        {
            this.TranslateExpression(sb, charValue);
        }

        public override void TranslateChr(StringBuilder sb, Expression charCode)
        {
            sb.Append("String.fromCharCode(");
            this.TranslateExpression(sb, charCode);
            sb.Append(')');
        }

        public override void TranslateCommandLineArgs(StringBuilder sb)
        {
            sb.Append("C$common$commandLineArgs");
        }

        public override void TranslateConstructorInvocation(StringBuilder sb, ConstructorInvocation constructorInvocation)
        {
            sb.Append('[');
            Expression[] args = constructorInvocation.Args;
            for (int i = 0; i < args.Length; ++i)
            {
                if (i > 0) sb.Append(", ");
                this.TranslateExpression(sb, args[i]);
            }
            sb.Append(']');
        }

        public override void TranslateConvertRawDictionaryValueCollectionToAReusableValueList(StringBuilder sb, Expression dictionary)
        {
            this.TranslateExpression(sb, dictionary);
        }

        public override void TranslateCurrentTimeSeconds(StringBuilder sb)
        {
            sb.Append("C$common$now()");
        }

        public override void TranslateDictionaryContainsKey(StringBuilder sb, Expression dictionary, Expression key)
        {
            sb.Append("(");
            this.TranslateExpression(sb, dictionary);
            sb.Append('[');
            this.TranslateExpression(sb, key);
            sb.Append("] !== undefined)");
        }

        public override void TranslateDictionaryGet(StringBuilder sb, Expression dictionary, Expression key)
        {
            this.TranslateExpression(sb, dictionary);
            sb.Append('[');
            this.TranslateExpression(sb, key);
            sb.Append(']');
        }

        public override void TranslateDictionaryKeys(StringBuilder sb, Expression dictionary)
        {
            sb.Append("C$common$dictionaryKeys(");
            this.TranslateExpression(sb, dictionary);
            sb.Append(')');
        }

        public override void TranslateDictionaryKeysToValueList(StringBuilder sb, Expression dictionary)
        {
            throw new NotImplementedException();
        }

        public override void TranslateDictionaryNew(StringBuilder sb, PType keyType, PType valueType)
        {
            sb.Append("{}");
        }

        public override void TranslateDictionaryRemove(StringBuilder sb, Expression dictionary, Expression key)
        {
            sb.Append("delete ");
            this.TranslateExpression(sb, dictionary);
            sb.Append('[');
            this.TranslateExpression(sb, key);
            sb.Append(']');
        }

        public override void TranslateDictionarySet(StringBuilder sb, Expression dictionary, Expression key, Expression value)
        {
            this.TranslateExpression(sb, dictionary);
            sb.Append('[');
            this.TranslateExpression(sb, key);
            sb.Append("] = ");
            this.TranslateExpression(sb, value);
        }

        public override void TranslateDictionarySize(StringBuilder sb, Expression dictionary)
        {
            sb.Append("Object.keys(");
            this.TranslateExpression(sb, dictionary);
            sb.Append(").length");
        }

        public override void TranslateDictionaryValues(StringBuilder sb, Expression dictionary)
        {
            sb.Append("C$common$dictionaryValues(");
            this.TranslateExpression(sb, dictionary);
            sb.Append(')');
        }

        public override void TranslateDictionaryValuesToValueList(StringBuilder sb, Expression dictionary)
        {
            throw new NotImplementedException();
        }

        public override void TranslateFloatBuffer16(StringBuilder sb)
        {
            sb.Append("C$common$floatBuffer16");
        }

        public override void TranslateFloatDivision(StringBuilder sb, Expression floatNumerator, Expression floatDenominator)
        {
            sb.Append('(');
            this.TranslateExpression(sb, floatNumerator);
            sb.Append(" / ");
            this.TranslateExpression(sb, floatDenominator);
            sb.Append(')');
        }

        public override void TranslateFloatToInt(StringBuilder sb, Expression floatExpr)
        {
            sb.Append("Math.floor(");
            this.TranslateExpression(sb, floatExpr);
            sb.Append(')');
        }

        public override void TranslateFloatToString(StringBuilder sb, Expression floatExpr)
        {
            sb.Append("'' + ");
            this.TranslateExpression(sb, floatExpr);
        }

        public override void TranslateGetProgramData(StringBuilder sb)
        {
            sb.Append("C$common$programData");
        }

        public override void TranslateGetResourceManifest(StringBuilder sb)
        {
            sb.Append("C$common$resourceManifest");
        }

        public override void TranslateGlobalVariable(StringBuilder sb, Variable variable)
        {
            sb.Append("v_");
            sb.Append(variable.Name);
        }

        public override void TranslateIntBuffer16(StringBuilder sb)
        {
            sb.Append("C$common$intBuffer16");
        }

        public override void TranslateIntegerDivision(StringBuilder sb, Expression integerNumerator, Expression integerDenominator)
        {
            sb.Append("Math.floor(");
            this.TranslateExpression(sb, integerNumerator);
            sb.Append(" / ");
            this.TranslateExpression(sb, integerDenominator);
            sb.Append(')');
        }

        public override void TranslateIntToString(StringBuilder sb, Expression integer)
        {
            sb.Append("('' + ");
            this.TranslateExpression(sb, integer);
            sb.Append(')');
        }

        public override void TranslateInvokeDynamicLibraryFunction(StringBuilder sb, Expression functionId, Expression argsArray)
        {
            this.TranslateExpression(sb, functionId);
            sb.Append('(');
            this.TranslateExpression(sb, argsArray);
            sb.Append(')');
        }

        public override void TranslateIsValidInteger(StringBuilder sb, Expression stringValue)
        {
            sb.Append("C$common$is_valid_integer(");
            this.TranslateExpression(sb, stringValue);
            sb.Append(')');
        }

        public override void TranslateListAdd(StringBuilder sb, Expression list, Expression item)
        {
            this.TranslateExpression(sb, list);
            sb.Append(".push(");
            this.TranslateExpression(sb, item);
            sb.Append(')');
        }

        public override void TranslateListClear(StringBuilder sb, Expression list)
        {
            sb.Append("C$common$clearList(");
            this.TranslateExpression(sb, list);
            sb.Append(')');
        }

        public override void TranslateListConcat(StringBuilder sb, Expression list, Expression items)
        {
            this.TranslateExpression(sb, list);
            sb.Append(".concat(");
            this.TranslateExpression(sb, items);
            sb.Append(")");
        }

        public override void TranslateListGet(StringBuilder sb, Expression list, Expression index)
        {
            this.TranslateExpression(sb, list);
            sb.Append('[');
            this.TranslateExpression(sb, index);
            sb.Append(']');
        }

        public override void TranslateListInsert(StringBuilder sb, Expression list, Expression index, Expression item)
        {
            this.TranslateExpression(sb, list);
            sb.Append(".splice(");
            this.TranslateExpression(sb, index);
            sb.Append(", 0, ");
            this.TranslateExpression(sb, item);
            sb.Append(')');
        }

        public override void TranslateListJoinChars(StringBuilder sb, Expression list)
        {
            this.TranslateExpression(sb, list);
            sb.Append(".join('')");
        }

        public override void TranslateListJoinStrings(StringBuilder sb, Expression list, Expression sep)
        {
            this.TranslateExpression(sb, list);
            sb.Append(".join(");
            this.TranslateExpression(sb, sep);
            sb.Append(')');
        }

        public override void TranslateListNew(StringBuilder sb, PType type)
        {
            sb.Append("[]");
        }

        public override void TranslateListPop(StringBuilder sb, Expression list)
        {
            this.TranslateExpression(sb, list);
            sb.Append(".pop()");
        }

        public override void TranslateListRemoveAt(StringBuilder sb, Expression list, Expression index)
        {
            this.TranslateExpression(sb, list);
            sb.Append(".splice(");
            this.TranslateExpression(sb, index);
            sb.Append(", 1)");
        }

        public override void TranslateListReverse(StringBuilder sb, Expression list)
        {
            this.TranslateExpression(sb, list);
            sb.Append(".reverse()");
        }

        public override void TranslateListSet(StringBuilder sb, Expression list, Expression index, Expression value)
        {
            this.TranslateExpression(sb, list);
            sb.Append('[');
            this.TranslateExpression(sb, index);
            sb.Append("] = ");
            this.TranslateExpression(sb, value);
        }

        public override void TranslateListShuffle(StringBuilder sb, Expression list)
        {
            sb.Append("C$common$shuffle(");
            this.TranslateExpression(sb, list);
            sb.Append(')');
        }

        public override void TranslateListSize(StringBuilder sb, Expression list)
        {
            this.TranslateExpression(sb, list);
            sb.Append(".length");
        }

        public override void TranslateListToArray(StringBuilder sb, Expression list)
        {
            // TODO: go through and figure out which list to array conversions are necessary to copy and which ones are just ensuring that the type is compatible
            // For example, JS and Python can just no-op in situations where a throwaway list builder is being made.
            sb.Append("C$common$multiplyList(");
            this.TranslateExpression(sb, list);
            sb.Append(", 1)");
        }

        public override void TranslateMathArcCos(StringBuilder sb, Expression ratio)
        {
            sb.Append("Math.acos(");
            this.TranslateExpression(sb, ratio);
            sb.Append(')');
        }

        public override void TranslateMathArcSin(StringBuilder sb, Expression ratio)
        {
            sb.Append("Math.asin(");
            this.TranslateExpression(sb, ratio);
            sb.Append(')');
        }

        public override void TranslateMathArcTan(StringBuilder sb, Expression yComponent, Expression xComponent)
        {
            sb.Append("Math.atan2(");
            this.TranslateExpression(sb, yComponent);
            sb.Append(", ");
            this.TranslateExpression(sb, xComponent);
            sb.Append(')');
        }

        public override void TranslateMathCos(StringBuilder sb, Expression thetaRadians)
        {
            sb.Append("Math.cos(");
            this.TranslateExpression(sb, thetaRadians);
            sb.Append(')');
        }

        public override void TranslateMathLog(StringBuilder sb, Expression value)
        {
            sb.Append("Math.log(");
            this.TranslateExpression(sb, value);
            sb.Append(')');
        }

        public override void TranslateMathPow(StringBuilder sb, Expression expBase, Expression exponent)
        {
            sb.Append("Math.pow(");
            this.TranslateExpression(sb, expBase);
            sb.Append(", ");
            this.TranslateExpression(sb, exponent);
            sb.Append(')');
        }

        public override void TranslateMathSin(StringBuilder sb, Expression thetaRadians)
        {
            sb.Append("Math.sin(");
            this.TranslateExpression(sb, thetaRadians);
            sb.Append(')');
        }

        public override void TranslateMathTan(StringBuilder sb, Expression thetaRadians)
        {
            sb.Append("Math.tan(");
            this.TranslateExpression(sb, thetaRadians);
            sb.Append(')');
        }

        public override void TranslateMultiplyList(StringBuilder sb, Expression list, Expression n)
        {
            sb.Append("C$common$multiplyList(");
            this.TranslateExpression(sb, list);
            sb.Append(", ");
            this.TranslateExpression(sb, n);
            sb.Append(')');
        }

        public override void TranslateNullConstant(StringBuilder sb)
        {
            sb.Append("null");
        }

        public override void TranslateOrd(StringBuilder sb, Expression charValue)
        {
            throw new NotImplementedException();
        }

        public override void TranslateParseFloatUnsafe(StringBuilder sb, Expression stringValue)
        {
            sb.Append("parseFloat(");
            this.TranslateExpression(sb, stringValue);
            sb.Append(')');
        }

        public override void TranslateParseInt(StringBuilder sb, Expression safeStringValue)
        {
            sb.Append("parseInt(");
            this.TranslateExpression(sb, safeStringValue);
            sb.Append(')');
        }

        public override void TranslatePrintStdErr(StringBuilder sb, Expression value)
        {
            sb.Append("C$common$print(");
            this.TranslateExpression(sb, value);
            sb.Append(')');
        }

        public override void TranslatePrintStdOut(StringBuilder sb, Expression value)
        {
            sb.Append("C$common$print(");
            this.TranslateExpression(sb, value);
            sb.Append(')');
        }

        public override void TranslateRandomFloat(StringBuilder sb)
        {
            sb.Append("Math.random()");
        }

        public override void TranslateReadByteCodeFile(StringBuilder sb)
        {
            sb.Append("C$bytecode");
        }

        public override void TranslateRegisterLibraryFunction(StringBuilder sb, Expression libRegObj, Expression functionName, Expression functionArgCount)
        {
            sb.Append("C$common$registerLibraryFunction('");
            sb.Append(this.CurrentLibraryFunctionTranslator.LibraryID.ToLower());
            sb.Append("', ");
            this.TranslateExpression(sb, libRegObj);
            sb.Append(", ");
            this.TranslateExpression(sb, functionName);
            sb.Append(", ");
            this.TranslateExpression(sb, functionArgCount);
            sb.Append(')');
        }

        public override void TranslateResourceReadTextFile(StringBuilder sb, Expression path)
        {
            sb.Append("C$common$readResourceText(");
            this.TranslateExpression(sb, path);
            sb.Append(')');
        }

        public override void TranslateSetProgramData(StringBuilder sb, Expression programData)
        {
            sb.Append("C$common$programData = ");
            this.TranslateExpression(sb, programData);
        }

        public override void TranslateSortedCopyOfIntArray(StringBuilder sb, Expression intArray)
        {
            sb.Append("C$common$sortedCopyOfArray(");
            this.TranslateExpression(sb, intArray);
            sb.Append(')');
        }

        public override void TranslateSortedCopyOfStringArray(StringBuilder sb, Expression stringArray)
        {
            sb.Append("C$common$sortedCopyOfArray(");
            this.TranslateExpression(sb, stringArray);
            sb.Append(')');
        }

        public override void TranslateStringAppend(StringBuilder sb, Expression str1, Expression str2)
        {
            this.TranslateExpression(sb, str1);
            sb.Append(" += ");
            this.TranslateExpression(sb, str2);
        }

        public override void TranslateStringBuffer16(StringBuilder sb)
        {
            sb.Append("C$common$stringBuffer16");
        }

        public override void TranslateStringCharAt(StringBuilder sb, Expression str, Expression index)
        {
            this.TranslateExpression(sb, str);
            sb.Append(".charAt(");
            this.TranslateExpression(sb, index);
            sb.Append(')');
        }

        public override void TranslateStringCharCodeAt(StringBuilder sb, Expression str, Expression index)
        {
            this.TranslateExpression(sb, str);
            sb.Append(".charCodeAt(");
            this.TranslateExpression(sb, index);
            sb.Append(')');
        }

        public override void TranslateStringCompareIsReverse(StringBuilder sb, Expression str1, Expression str2)
        {
            sb.Append("(");
            this.TranslateExpression(sb, str1);
            sb.Append(".localeCompare(");
            this.TranslateExpression(sb, str2);
            sb.Append(") > 0)");
        }

        public override void TranslateStringConcatAll(StringBuilder sb, Expression[] strings)
        {
            sb.Append("[");
            for (int i = 0; i < strings.Length; ++i)
            {
                if (i > 0) sb.Append(", ");
                this.TranslateExpression(sb, strings[i]);
            }
            sb.Append("].join('')");
        }

        public override void TranslateStringConcatPair(StringBuilder sb, Expression strLeft, Expression strRight)
        {
            this.TranslateExpression(sb, strLeft);
            sb.Append(" + ");
            this.TranslateExpression(sb, strRight);
        }

        public override void TranslateStringContains(StringBuilder sb, Expression haystack, Expression needle)
        {
            sb.Append('(');
            this.TranslateExpression(sb, haystack);
            sb.Append(".indexOf(");
            this.TranslateExpression(sb, needle);
            sb.Append(") != -1)");
        }

        public override void TranslateStringEndsWith(StringBuilder sb, Expression haystack, Expression needle)
        {
            sb.Append("C$common$stringEndsWith(");
            this.TranslateExpression(sb, haystack);
            sb.Append(", ");
            this.TranslateExpression(sb, needle);
            sb.Append(')');
        }

        public override void TranslateStringEquals(StringBuilder sb, Expression left, Expression right)
        {
            this.TranslateExpression(sb, left);
            sb.Append(" == ");
            this.TranslateExpression(sb, right);
        }

        public override void TranslateStringFromCharCode(StringBuilder sb, Expression charCode)
        {
            sb.Append("String.fromCharCode(");
            this.TranslateExpression(sb, charCode);
            sb.Append(')');
        }

        public override void TranslateStringIndexOf(StringBuilder sb, Expression haystack, Expression needle)
        {
            this.TranslateExpression(sb, haystack);
            sb.Append(".indexOf(");
            this.TranslateExpression(sb, needle);
            sb.Append(')');
        }

        public override void TranslateStringIndexOfWithStart(StringBuilder sb, Expression haystack, Expression needle, Expression startIndex)
        {
            this.TranslateExpression(sb, haystack);
            sb.Append(".indexOf(");
            this.TranslateExpression(sb, needle);
            sb.Append(", ");
            this.TranslateExpression(sb, startIndex);
            sb.Append(')');
        }

        public override void TranslateStringLength(StringBuilder sb, Expression str)
        {
            this.TranslateExpression(sb, str);
            sb.Append(".length");
        }

        public override void TranslateStringReplace(StringBuilder sb, Expression haystack, Expression needle, Expression newNeedle)
        {
            this.TranslateExpression(sb, haystack);
            sb.Append(".split(");
            this.TranslateExpression(sb, needle);
            sb.Append(").join(");
            this.TranslateExpression(sb, newNeedle);
            sb.Append(')');
        }

        public override void TranslateStringReverse(StringBuilder sb, Expression str)
        {
            this.TranslateExpression(sb, str);
            sb.Append(".split('').reverse().join('')");
        }

        public override void TranslateStringSplit(StringBuilder sb, Expression haystack, Expression needle)
        {
            this.TranslateExpression(sb, haystack);
            sb.Append(".split(");
            this.TranslateExpression(sb, needle);
            sb.Append(')');
        }

        public override void TranslateStringStartsWith(StringBuilder sb, Expression haystack, Expression needle)
        {
            sb.Append('(');
            this.TranslateExpression(sb, haystack);
            sb.Append(".indexOf(");
            this.TranslateExpression(sb, needle);
            sb.Append(") == 0)");
        }

        public override void TranslateStringSubstring(StringBuilder sb, Expression str, Expression start, Expression length)
        {
            this.TranslateExpression(sb, str);
            sb.Append(".substring(");
            this.TranslateExpression(sb, start);
            sb.Append(", ");
            this.TranslateExpression(sb, start);
            sb.Append(" + ");
            this.TranslateExpression(sb, length);
            sb.Append(')');
        }

        public override void TranslateStringSubstringIsEqualTo(StringBuilder sb, Expression haystack, Expression startIndex, Expression needle)
        {
            throw new NotImplementedException();
        }

        public override void TranslateStringToLower(StringBuilder sb, Expression str)
        {
            this.TranslateExpression(sb, str);
            sb.Append(".toLowerCase()");
        }

        public override void TranslateStringToUpper(StringBuilder sb, Expression str)
        {
            this.TranslateExpression(sb, str);
            sb.Append(".toUpperCase()");
        }

        public override void TranslateStringTrim(StringBuilder sb, Expression str)
        {
            this.TranslateExpression(sb, str);
            sb.Append(".trim()");
        }

        public override void TranslateStringTrimEnd(StringBuilder sb, Expression str)
        {
            sb.Append("C$common$stringTrimOneSide(");
            this.TranslateExpression(sb, str);
            sb.Append(", false)");
        }

        public override void TranslateStringTrimStart(StringBuilder sb, Expression str)
        {
            sb.Append("C$common$stringTrimOneSide(");
            this.TranslateExpression(sb, str);
            sb.Append(", true)");
        }

        public override void TranslateStrongReferenceEquality(StringBuilder sb, Expression left, Expression right)
        {
            throw new NotImplementedException();
        }

        public override void TranslateStructFieldDereference(StringBuilder sb, Expression root, StructDefinition structDef, string fieldName, int fieldIndex)
        {
            this.TranslateExpression(sb, root);
            sb.Append('[');
            sb.Append(fieldIndex);
            sb.Append(']');
        }

        public override void TranslateThreadSleep(StringBuilder sb, Expression seconds)
        {
            throw new NotImplementedException();
        }

        public override void TranslateTryParseFloat(StringBuilder sb, Expression stringValue, Expression floatOutList)
        {
            sb.Append("C$common$floatParseHelper(");
            this.TranslateExpression(sb, floatOutList);
            sb.Append(", ");
            this.TranslateExpression(sb, stringValue);
            sb.Append(')');
        }

        public override void TranslateVariableDeclaration(StringBuilder sb, VariableDeclaration varDecl)
        {
            sb.Append(this.CurrentTab);
            sb.Append("var v_");
            sb.Append(varDecl.VariableNameToken.Value);
            sb.Append(" = ");
            if (varDecl.Value == null)
            {
                sb.Append("null");
            }
            else
            {
                this.TranslateExpression(sb, varDecl.Value);
            }
            sb.Append(';');
            sb.Append(this.NewLine);
        }

        public override void TranslateVmDetermineLibraryAvailability(StringBuilder sb, Expression libraryName, Expression libraryVersion)
        {
            sb.Append("C$common$determineLibraryAvailability(");
            this.TranslateExpression(sb, libraryName);
            sb.Append(", ");
            this.TranslateExpression(sb, libraryVersion);
            sb.Append(')');
        }

        public override void TranslateVmEnqueueResume(StringBuilder sb, Expression seconds, Expression executionContextId)
        {
            sb.Append("C$common$enqueueVmResume(");
            this.TranslateExpression(sb, seconds);
            sb.Append(", ");
            this.TranslateExpression(sb, executionContextId);
            sb.Append(')');
        }

        public override void TranslateVmRunLibraryManifest(StringBuilder sb, Expression libraryName, Expression libRegObj)
        {
            sb.Append("C$common$runLibraryManifest(");
            this.TranslateExpression(sb, libraryName);
            sb.Append(", ");
            this.TranslateExpression(sb, libRegObj);
            sb.Append(')');
        }
    }
}
