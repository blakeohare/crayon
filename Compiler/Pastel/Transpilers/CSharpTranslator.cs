using Pastel.Nodes;
using System;
using System.Collections.Generic;

namespace Pastel.Transpilers
{
    internal class CSharpTranslator : CurlyBraceTranslator
    {
        public CSharpTranslator() : base("    ", "\r\n", false)
        { }

        public override string TranslateType(PType type)
        {
            switch (type.RootValue)
            {
                case "int":
                case "char":
                case "bool":
                case "double":
                case "string":
                case "object":
                case "void":
                    return type.RootValue;

                case "List":
                    return "List<" + this.TranslateType(type.Generics[0]) + ">";

                case "Dictionary":
                    return "Dictionary<" + this.TranslateType(type.Generics[0]) + ", " + this.TranslateType(type.Generics[1]) + ">";

                case "Array":
                    return this.TranslateType(type.Generics[0]) + "[]";

                case "Func":
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    sb.Append("Func<");
                    for (int i = 0; i < type.Generics.Length - 1; ++i)
                    {
                        sb.Append(this.TranslateType(type.Generics[i + 1]));
                        sb.Append(", ");
                    }
                    sb.Append(this.TranslateType(type.Generics[0]));
                    sb.Append('>');
                    return sb.ToString();

                default:
                    if (type.Generics.Length > 0)
                    {
                        throw new NotImplementedException();
                    }
                    return type.RootValue;
            }
        }

        public override void TranslatePrintStdErr(TranspilerContext sb, Expression value)
        {
            sb.Append("PlatformTranslationHelper.PrintStdErr(");
            this.TranslateExpression(sb, value);
            sb.Append(')');
        }

        public override void TranslatePrintStdOut(TranspilerContext sb, Expression value)
        {
            sb.Append("PlatformTranslationHelper.PrintStdOut(");
            this.TranslateExpression(sb, value);
            sb.Append(')');
        }

        public override void TranslateThreadSleep(TranspilerContext sb, Expression seconds)
        {
            sb.Append("PlatformTranslationHelper.ThreadSleep(");
            this.TranslateExpression(sb, seconds);
            sb.Append(')');
        }

        public override void TranslateArrayGet(TranspilerContext sb, Expression array, Expression index)
        {
            this.TranslateExpression(sb, array);
            sb.Append('[');
            this.TranslateExpression(sb, index);
            sb.Append(']');
        }

        public override void TranslateArrayJoin(TranspilerContext sb, Expression array, Expression sep)
        {
            sb.Append("string.Join(");
            this.TranslateExpression(sb, sep);
            sb.Append(", ");
            this.TranslateExpression(sb, array);
            sb.Append(')');
        }

        public override void TranslateArrayLength(TranspilerContext sb, Expression array)
        {
            this.TranslateExpression(sb, array);
            sb.Append(".Length");
        }

        public override void TranslateArrayNew(TranspilerContext sb, PType arrayType, Expression lengthExpression)
        {
            int nestingLevel = 0;
            while (arrayType.RootValue == "Array")
            {
                nestingLevel++;
                arrayType = arrayType.Generics[0];
            }
            sb.Append("new ");
            sb.Append(this.TranslateType(arrayType));
            sb.Append('[');
            this.TranslateExpression(sb, lengthExpression);
            sb.Append(']');
            while (nestingLevel-- > 0)
            {
                sb.Append("[]");
            }
        }

        public override void TranslateArraySet(TranspilerContext sb, Expression array, Expression index, Expression value)
        {
            this.TranslateExpression(sb, array);
            sb.Append('[');
            this.TranslateExpression(sb, index);
            sb.Append("] = ");
            this.TranslateExpression(sb, value);
        }

        public override void TranslateBase64ToString(TranspilerContext sb, Expression base64String)
        {
            sb.Append("TranslationHelper.Base64ToString(");
            this.TranslateExpression(sb, base64String);
            sb.Append(')');
        }

        public override void TranslateCast(TranspilerContext sb, PType type, Expression expression)
        {
            sb.Append('(');
            sb.Append(this.TranslateType(type));
            sb.Append(')');
            this.TranslateExpression(sb, expression);
        }

        public override void TranslateCharConstant(TranspilerContext sb, char value)
        {
            sb.Append(PastelUtil.ConvertCharToCharConstantCode(value));
        }

        public override void TranslateCharToString(TranspilerContext sb, Expression charValue)
        {
            this.TranslateExpression(sb, charValue);
            sb.Append(".ToString()");
        }

        public override void TranslateChr(TranspilerContext sb, Expression charCode)
        {
            sb.Append("((char) ");
            this.TranslateExpression(sb, charCode);
            sb.Append(")");
        }

        public override void TranslateConvertRawDictionaryValueCollectionToAReusableValueList(TranspilerContext sb, Expression dictionary)
        {
            sb.Append("new List<Value>(");
            this.TranslateExpression(sb, dictionary);
            sb.Append(')');
        }

        public override void TranslateCurrentTimeSeconds(TranspilerContext sb)
        {
            sb.Append("TranslationHelper.CurrentTime");
        }

        public override void TranslateConstructorInvocation(TranspilerContext sb, ConstructorInvocation constructorInvocation)
        {
            sb.Append("new ");
            sb.Append(constructorInvocation.Type.RootValue);
            sb.Append('(');
            Expression[] args = constructorInvocation.Args;
            for (int i = 0; i < args.Length; ++i)
            {
                if (i > 0) sb.Append(", ");
                this.TranslateExpression(sb, args[i]);
            }
            sb.Append(')');
        }

        public override void TranslateDictionaryContainsKey(TranspilerContext sb, Expression dictionary, Expression key)
        {
            this.TranslateExpression(sb, dictionary);
            sb.Append(".ContainsKey(");
            this.TranslateExpression(sb, key);
            sb.Append(")");
        }

        public override void TranslateDictionaryGet(TranspilerContext sb, Expression dictionary, Expression key)
        {
            this.TranslateExpression(sb, dictionary);
            sb.Append('[');
            this.TranslateExpression(sb, key);
            sb.Append(']');
        }

        public override void TranslateDictionaryKeys(TranspilerContext sb, Expression dictionary)
        {
            this.TranslateExpression(sb, dictionary);
            sb.Append(".Keys.ToArray()");
        }

        public override void TranslateDictionaryKeysToValueList(TranspilerContext sb, Expression dictionary)
        {
            throw new NotImplementedException();
        }

        public override void TranslateDictionaryNew(TranspilerContext sb, PType keyType, PType valueType)
        {
            sb.Append("new Dictionary<");
            sb.Append(this.TranslateType(keyType));
            sb.Append(", ");
            sb.Append(this.TranslateType(valueType));
            sb.Append(">()");
        }

        public override void TranslateDictionaryRemove(TranspilerContext sb, Expression dictionary, Expression key)
        {
            this.TranslateExpression(sb, dictionary);
            sb.Append(".Remove(");
            this.TranslateExpression(sb, key);
            sb.Append(')');
        }

        public override void TranslateDictionarySet(TranspilerContext sb, Expression dictionary, Expression key, Expression value)
        {
            this.TranslateExpression(sb, dictionary);
            sb.Append('[');
            this.TranslateExpression(sb, key);
            sb.Append("] = ");
            this.TranslateExpression(sb, value);
        }

        public override void TranslateDictionarySize(TranspilerContext sb, Expression dictionary)
        {
            this.TranslateExpression(sb, dictionary);
            sb.Append(".Count");
        }

        public override void TranslateDictionaryValues(TranspilerContext sb, Expression dictionary)
        {
            this.TranslateExpression(sb, dictionary);
            sb.Append(".Values.ToArray()");
        }

        public override void TranslateDictionaryValuesToValueList(TranspilerContext sb, Expression dictionary)
        {
            throw new NotImplementedException();
        }

        public override void TranslateFloatBuffer16(TranspilerContext sb)
        {
            sb.Append("TranslationHelper.FloatBuffer16");
        }

        public override void TranslateFloatDivision(TranspilerContext sb, Expression floatNumerator, Expression floatDenominator)
        {
            sb.Append("(");
            this.TranslateExpression(sb, floatNumerator);
            sb.Append(") / (");
            this.TranslateExpression(sb, floatDenominator);
            sb.Append(')');
        }

        public override void TranslateFloatToInt(TranspilerContext sb, Expression floatExpr)
        {
            sb.Append("(int)");
            this.TranslateExpression(sb, floatExpr);
        }

        public override void TranslateFloatToString(TranspilerContext sb, Expression floatExpr)
        {
            sb.Append("TranslationHelper.FloatToString(");
            this.TranslateExpression(sb, floatExpr);
            sb.Append(')');
        }

        public override void TranslateGetFunction(TranspilerContext sb, Expression name)
        {
            sb.Append("TranslationHelper.GetFunctionPointer(");
            this.TranslateExpression(sb, name);
            sb.Append(')');
        }

        public override void TranslateGetProgramData(TranspilerContext sb)
        {
            sb.Append("TranslationHelper.ProgramData");
        }

        public override void TranslateGetResourceManifest(TranspilerContext sb)
        {
            sb.Append("TranslationHelper.ResourceManifest");
        }

        public override void TranslateGlobalVariable(TranspilerContext sb, Variable variable)
        {
            sb.Append("Globals.v_");
            sb.Append(variable.Name);
        }

        public override void TranslateIntBuffer16(TranspilerContext sb)
        {
            sb.Append("TranslationHelper.IntBuffer16");
        }

        public override void TranslateIntegerDivision(TranspilerContext sb, Expression integerNumerator, Expression integerDenominator)
        {
            sb.Append("(");
            this.TranslateExpression(sb, integerNumerator);
            sb.Append(") / (");
            this.TranslateExpression(sb, integerDenominator);
            sb.Append(')');
        }

        public override void TranslateIntToString(TranspilerContext sb, Expression integer)
        {
            sb.Append("(");
            this.TranslateExpression(sb, integer);
            sb.Append(").ToString()");
        }

        public override void TranslateFunctionInvocationInterpreterScoped(TranspilerContext sb, FunctionReference funcRef, Expression[] args)
        {
            sb.Append("CrayonWrapper.");
            base.TranslateFunctionInvocationInterpreterScoped(sb, funcRef, args);
        }

        public override void TranslateIsValidInteger(TranspilerContext sb, Expression stringValue)
        {
            sb.Append("TranslationHelper.IsValidInteger(");
            this.TranslateExpression(sb, stringValue);
            sb.Append(')');
        }

        public override void TranslateListAdd(TranspilerContext sb, Expression list, Expression item)
        {
            this.TranslateExpression(sb, list);
            sb.Append(".Add(");
            this.TranslateExpression(sb, item);
            sb.Append(')');
        }

        public override void TranslateListConcat(TranspilerContext sb, Expression list, Expression items)
        {
            sb.Append("TranslationHelper.ListConcat(");
            this.TranslateExpression(sb, list);
            sb.Append(", ");
            this.TranslateExpression(sb, items);
            sb.Append(")");
        }

        public override void TranslateListClear(TranspilerContext sb, Expression list)
        {
            this.TranslateExpression(sb, list);
            sb.Append(".Clear()");
        }

        public override void TranslateListGet(TranspilerContext sb, Expression list, Expression index)
        {
            this.TranslateExpression(sb, list);
            sb.Append('[');
            this.TranslateExpression(sb, index);
            sb.Append(']');
        }

        public override void TranslateListInsert(TranspilerContext sb, Expression list, Expression index, Expression item)
        {
            this.TranslateExpression(sb, list);
            sb.Append(".Insert(");
            this.TranslateExpression(sb, index);
            sb.Append(", ");
            this.TranslateExpression(sb, item);
            sb.Append(')');
        }

        public override void TranslateListJoinChars(TranspilerContext sb, Expression list)
        {
            sb.Append("new String(");
            this.TranslateExpression(sb, list);
            sb.Append(".ToArray())");
        }

        public override void TranslateListJoinStrings(TranspilerContext sb, Expression list, Expression sep)
        {
            sb.Append("string.Join(");
            this.TranslateExpression(sb, sep);
            sb.Append(", ");
            this.TranslateExpression(sb, list);
            sb.Append(')');
        }

        public override void TranslateListNew(TranspilerContext sb, PType type)
        {
            sb.Append("new List<");
            sb.Append(this.TranslateType(type));
            sb.Append(">()");
        }

        public override void TranslateListPop(TranspilerContext sb, Expression list)
        {
            // No megusta
            this.TranslateExpression(sb, list);
            sb.Append(".RemoveAt(");
            this.TranslateExpression(sb, list);
            sb.Append(".Count - 1)");
        }

        public override void TranslateListRemoveAt(TranspilerContext sb, Expression list, Expression index)
        {
            this.TranslateExpression(sb, list);
            sb.Append(".RemoveAt(");
            this.TranslateExpression(sb, index);
            sb.Append(')');
        }

        public override void TranslateListReverse(TranspilerContext sb, Expression list)
        {
            this.TranslateExpression(sb, list);
            sb.Append(".Reverse()");
        }

        public override void TranslateListSet(TranspilerContext sb, Expression list, Expression index, Expression value)
        {
            this.TranslateExpression(sb, list);
            sb.Append('[');
            this.TranslateExpression(sb, index);
            sb.Append("] = ");
            this.TranslateExpression(sb, value);
        }

        public override void TranslateListShuffle(TranspilerContext sb, Expression list)
        {
            sb.Append("TranslationHelper.ShuffleInPlace(");
            this.TranslateExpression(sb, list);
            sb.Append(')');
        }

        public override void TranslateListSize(TranspilerContext sb, Expression list)
        {
            this.TranslateExpression(sb, list);
            sb.Append(".Count");
        }

        public override void TranslateListToArray(TranspilerContext sb, Expression list)
        {
            this.TranslateExpression(sb, list);
            sb.Append(".ToArray()");
        }

        public override void TranslateMathArcCos(TranspilerContext sb, Expression ratio)
        {
            sb.Append("Math.Acos(");
            this.TranslateExpression(sb, ratio);
            sb.Append(')');
        }

        public override void TranslateMathArcSin(TranspilerContext sb, Expression ratio)
        {
            sb.Append("Math.Asin(");
            this.TranslateExpression(sb, ratio);
            sb.Append(')');
        }

        public override void TranslateMathArcTan(TranspilerContext sb, Expression yComponent, Expression xComponent)
        {
            sb.Append("Math.Atan2(");
            this.TranslateExpression(sb, yComponent);
            sb.Append(", ");
            this.TranslateExpression(sb, xComponent);
            sb.Append(')');
        }

        public override void TranslateMathCos(TranspilerContext sb, Expression thetaRadians)
        {
            sb.Append("Math.Cos(");
            this.TranslateExpression(sb, thetaRadians);
            sb.Append(')');
        }

        public override void TranslateMathLog(TranspilerContext sb, Expression value)
        {
            sb.Append("Math.Log(");
            this.TranslateExpression(sb, value);
            sb.Append(')');
        }

        public override void TranslateMathPow(TranspilerContext sb, Expression expBase, Expression exponent)
        {
            sb.Append("Math.Pow(");
            this.TranslateExpression(sb, expBase);
            sb.Append(", ");
            this.TranslateExpression(sb, exponent);
            sb.Append(")");
        }

        public override void TranslateMathSin(TranspilerContext sb, Expression thetaRadians)
        {
            sb.Append("Math.Sin(");
            this.TranslateExpression(sb, thetaRadians);
            sb.Append(')');
        }

        public override void TranslateMathTan(TranspilerContext sb, Expression thetaRadians)
        {
            sb.Append("Math.Tan(");
            this.TranslateExpression(sb, thetaRadians);
            sb.Append(')');
        }

        public override void TranslateMultiplyList(TranspilerContext sb, Expression list, Expression n)
        {
            sb.Append("TranslationHelper.MultiplyList(");
            this.TranslateExpression(sb, list);
            sb.Append(", ");
            this.TranslateExpression(sb, n);
            sb.Append(")");
        }

        public override void TranslateNullConstant(TranspilerContext sb)
        {
            sb.Append("null");
        }

        public override void TranslateOrd(TranspilerContext sb, Expression charValue)
        {
            if (charValue is InlineConstant)
            {
                // this should have been optimized out.
                // throw new Exception(); // TODO: but it isn't quite ye
            }
            sb.Append("((int)(");
            this.TranslateExpression(sb, charValue);
            sb.Append("))");
        }

        public override void TranslateParseFloatUnsafe(TranspilerContext sb, Expression stringValue)
        {
            sb.Append("double.Parse(");
            this.TranslateExpression(sb, stringValue);
            sb.Append(')');
        }

        public override void TranslateParseInt(TranspilerContext sb, Expression safeStringValue)
        {
            sb.Append("int.Parse(");
            this.TranslateExpression(sb, safeStringValue);
            sb.Append(')');
        }

        public override void TranslateRandomFloat(TranspilerContext sb)
        {
            sb.Append("TranslationHelper.Random.NextDouble()");
        }

        public override void TranslateReadByteCodeFile(TranspilerContext sb)
        {
            sb.Append("TranslationHelper.ByteCode");
        }

        public override void TranslateResourceReadTextFile(TranspilerContext sb, Expression path)
        {
            sb.Append("ResourceReader.ReadTextResource(");
            this.TranslateExpression(sb, path);
            sb.Append(")");
        }

        public override void TranslateSortedCopyOfIntArray(TranspilerContext sb, Expression intArray)
        {
            this.TranslateExpression(sb, intArray);
            sb.Append(".OrderBy<int, int>(i => i).ToArray()");
        }

        public override void TranslateSortedCopyOfStringArray(TranspilerContext sb, Expression stringArray)
        {
            this.TranslateExpression(sb, stringArray);
            sb.Append(".OrderBy<string, string>(s => s).ToArray()");
        }

        public override void TranslateStringAppend(TranspilerContext sb, Expression str1, Expression str2)
        {
            this.TranslateExpression(sb, str1);
            sb.Append(" += ");
            this.TranslateExpression(sb, str2);
        }

        public override void TranslateStringBuffer16(TranspilerContext sb)
        {
            sb.Append("TranslationHelper.StringBuffer16");
        }

        public override void TranslateStringCharAt(TranspilerContext sb, Expression str, Expression index)
        {
            this.TranslateExpression(sb, str);
            sb.Append('[');
            this.TranslateExpression(sb, index);
            sb.Append(']');
        }

        public override void TranslateStringCharCodeAt(TranspilerContext sb, Expression str, Expression index)
        {
            sb.Append("((int) ");
            this.TranslateExpression(sb, str);
            sb.Append('[');
            this.TranslateExpression(sb, index);
            sb.Append("])");
        }

        public override void TranslateStringCompareIsReverse(TranspilerContext sb, Expression str1, Expression str2)
        {
            sb.Append('(');
            this.TranslateExpression(sb, str1);
            sb.Append(".CompareTo(");
            this.TranslateExpression(sb, str2);
            sb.Append(") == 1)");
        }

        public override void TranslateStringConcatAll(TranspilerContext sb, Expression[] strings)
        {
            sb.Append("string.Join(\"\", new string[] { ");
            for (int i = 0; i < strings.Length; ++i)
            {
                if (i > 0) sb.Append(',');
                this.TranslateExpression(sb, strings[i]);
            }
            sb.Append(" })");
        }

        public override void TranslateStringConcatPair(TranspilerContext sb, Expression strLeft, Expression strRight)
        {
            this.TranslateExpression(sb, strLeft);
            sb.Append(" + ");
            this.TranslateExpression(sb, strRight);
        }

        public override void TranslateStringContains(TranspilerContext sb, Expression haystack, Expression needle)
        {
            this.TranslateExpression(sb, haystack);
            sb.Append(".Contains(");
            this.TranslateExpression(sb, needle);
            sb.Append(')');
        }

        public override void TranslateStringEndsWith(TranspilerContext sb, Expression haystack, Expression needle)
        {
            this.TranslateExpression(sb, haystack);
            sb.Append(".EndsWith(");
            this.TranslateExpression(sb, needle);
            sb.Append(')');
        }

        public override void TranslateStringEquals(TranspilerContext sb, Expression left, Expression right)
        {
            this.TranslateExpression(sb, left);
            sb.Append(" == ");
            this.TranslateExpression(sb, right);
        }

        public override void TranslateStringFromCharCode(TranspilerContext sb, Expression charCode)
        {
            sb.Append("((char) ");
            this.TranslateExpression(sb, charCode);
            sb.Append(").ToString()");
        }

        public override void TranslateStringIndexOf(TranspilerContext sb, Expression haystack, Expression needle)
        {
            this.TranslateExpression(sb, haystack);
            sb.Append(".IndexOf(");
            this.TranslateExpression(sb, needle);
            sb.Append(')');
        }

        public override void TranslateStringIndexOfWithStart(TranspilerContext sb, Expression haystack, Expression needle, Expression startIndex)
        {
            this.TranslateExpression(sb, haystack);
            sb.Append(".IndexOf(");
            this.TranslateExpression(sb, needle);
            sb.Append(", ");
            this.TranslateExpression(sb, startIndex);
            sb.Append(')');
        }

        public override void TranslateStringLength(TranspilerContext sb, Expression str)
        {
            this.TranslateExpression(sb, str);
            sb.Append(".Length");
        }

        public override void TranslateStringReplace(TranspilerContext sb, Expression haystack, Expression needle, Expression newNeedle)
        {
            this.TranslateExpression(sb, haystack);
            sb.Append(".Replace(");
            this.TranslateExpression(sb, needle);
            sb.Append(", ");
            this.TranslateExpression(sb, newNeedle);
            sb.Append(")");
        }

        public override void TranslateStringReverse(TranspilerContext sb, Expression str)
        {
            sb.Append("TranslationHelper.StringReverse(");
            this.TranslateExpression(sb, str);
            sb.Append(')');
        }

        public override void TranslateStringSplit(TranspilerContext sb, Expression haystack, Expression needle)
        {
            sb.Append("TranslationHelper.StringSplit(");
            this.TranslateExpression(sb, haystack);
            sb.Append(", ");
            this.TranslateExpression(sb, needle);
            sb.Append(')');
        }

        public override void TranslateStringStartsWith(TranspilerContext sb, Expression haystack, Expression needle)
        {
            this.TranslateExpression(sb, haystack);
            sb.Append(".StartsWith(");
            this.TranslateExpression(sb, needle);
            sb.Append(')');
        }

        public override void TranslateStringSubstring(TranspilerContext sb, Expression str, Expression start, Expression length)
        {
            this.TranslateExpression(sb, str);
            sb.Append(".Substring(");
            this.TranslateExpression(sb, start);
            sb.Append(", ");
            this.TranslateExpression(sb, length);
            sb.Append(')');
        }

        public override void TranslateStringSubstringIsEqualTo(TranspilerContext sb, Expression haystack, Expression startIndex, Expression needle)
        {
            sb.Append("TranslationHelper.SubstringIsEqualTo(");
            this.TranslateExpression(sb, haystack);
            sb.Append(", ");
            this.TranslateExpression(sb, startIndex);
            sb.Append(", ");
            this.TranslateExpression(sb, needle);
            sb.Append(')');
        }

        public override void TranslateStringToLower(TranspilerContext sb, Expression str)
        {
            this.TranslateExpression(sb, str);
            sb.Append(".ToLower()");
        }

        public override void TranslateStringToUpper(TranspilerContext sb, Expression str)
        {
            this.TranslateExpression(sb, str);
            sb.Append(".ToUpper()");
        }

        public override void TranslateStringTrim(TranspilerContext sb, Expression str)
        {
            this.TranslateExpression(sb, str);
            sb.Append(".Trim()");
        }

        public override void TranslateStringTrimEnd(TranspilerContext sb, Expression str)
        {
            this.TranslateExpression(sb, str);
            sb.Append(".TrimEnd()");
        }

        public override void TranslateStringTrimStart(TranspilerContext sb, Expression str)
        {
            this.TranslateExpression(sb, str);
            sb.Append(".TrimStart()");
        }

        public override void TranslateStrongReferenceEquality(TranspilerContext sb, Expression left, Expression right)
        {
            this.TranslateExpression(sb, left);
            sb.Append(" == ");
            this.TranslateExpression(sb, right);
        }

        public override void TranslateStructFieldDereference(TranspilerContext sb, Expression root, StructDefinition structDef, string fieldName, int fieldIndex)
        {
            this.TranslateExpression(sb, root);
            sb.Append('.');
            sb.Append(fieldName);
        }

        public override void TranslateTryParseFloat(TranspilerContext sb, Expression stringValue, Expression floatOutList)
        {
            sb.Append("TranslationHelper.ParseFloat(");
            this.TranslateExpression(sb, stringValue);
            sb.Append(", ");
            this.TranslateExpression(sb, floatOutList);
            sb.Append(')');
        }

        public override void TranslateVariableDeclaration(TranspilerContext sb, VariableDeclaration varDecl)
        {
            sb.Append(sb.CurrentTab);
            sb.Append(this.TranslateType(varDecl.Type));
            sb.Append(" v_");
            sb.Append(varDecl.VariableNameToken.Value);
            if (varDecl.Value != null)
            {
                sb.Append(" = ");
                this.TranslateExpression(sb, varDecl.Value);
            }
            sb.Append(';');
            sb.Append(this.NewLine);
        }

        public override void TranslateVmDetermineLibraryAvailability(TranspilerContext sb, Expression libraryName, Expression libraryVersion)
        {
            sb.Append("TranslationHelper.VmDetermineLibraryAvailability(");
            this.TranslateExpression(sb, libraryName);
            sb.Append(", ");
            this.TranslateExpression(sb, libraryVersion);
            sb.Append(')');
        }

        public override void TranslateVmEnqueueResume(TranspilerContext sb, Expression seconds, Expression executionContextId)
        {
            throw new NotImplementedException();
        }

        public override void TranslateVmEndProcess(TranspilerContext sb)
        {
            // Not needed. C# Finishes cleanly on all current platforms.
        }

        public override void GenerateCodeForStruct(TranspilerContext sb, StructDefinition structDef)
        {
            PType[] types = structDef.ArgTypes;
            Pastel.Token[] fieldNames = structDef.ArgNames;
            string name = structDef.NameToken.Value;
            List<string> lines = new List<string>();

            lines.Add("public class " + name);
            lines.Add("{");
            for (int i = 0; i < types.Length; ++i)
            {
                lines.Add("    public " + this.TranslateType(types[i]) + " " + fieldNames[i].Value + ";");
            }
            lines.Add("");

            System.Text.StringBuilder constructorDeclaration = new System.Text.StringBuilder();
            constructorDeclaration.Append("    public ");
            constructorDeclaration.Append(name);
            constructorDeclaration.Append('(');
            for (int i = 0; i < types.Length; ++i)
            {
                if (i > 0) constructorDeclaration.Append(", ");
                constructorDeclaration.Append(this.TranslateType(types[i]));
                constructorDeclaration.Append(' ');
                constructorDeclaration.Append(fieldNames[i].Value);
            }
            constructorDeclaration.Append(')');
            lines.Add(constructorDeclaration.ToString());
            lines.Add("    {");
            for (int i = 0; i < types.Length; ++i)
            {
                string fieldName = fieldNames[i].Value;
                lines.Add("        this." + fieldName + " = " + fieldName + ";");
            }
            lines.Add("    }");

            lines.Add("}");
            lines.Add("");

            // TODO: rewrite this function to use the string builder inline and use this.NL
            sb.Append(string.Join("\r\n", lines));
        }

        public override void GenerateCodeForFunction(TranspilerContext output, FunctionDefinition funcDef)
        {
            PType returnType = funcDef.ReturnType;
            string funcName = funcDef.NameToken.Value;
            PType[] argTypes = funcDef.ArgTypes;
            Token[] argNames = funcDef.ArgNames;

            output.Append("public static ");
            output.Append(this.TranslateType(returnType));
            output.Append(" v_");
            output.Append(funcName);
            output.Append("(");
            for (int i = 0; i < argTypes.Length; ++i)
            {
                if (i > 0) output.Append(", ");
                output.Append(this.TranslateType(argTypes[i]));
                output.Append(" v_");
                output.Append(argNames[i].Value);
            }
            output.Append(")");
            output.Append(this.NewLine);
            output.Append("{");
            output.Append(this.NewLine);
            output.TabDepth = 1;
            this.TranslateExecutables(output, funcDef.Code);
            output.TabDepth = 0;
            output.Append("}");
        }

        public override void GenerateCodeForGlobalsDefinitions(TranspilerContext output, IList<VariableDeclaration> globals)
        {
            output.Append("    public static class Globals");
            output.Append(this.NewLine);
            output.Append("    {");
            output.Append(this.NewLine);
            output.TabDepth = 0;
            foreach (VariableDeclaration vd in globals)
            {
                output.Append("        public static ");
                this.TranslateVariableDeclaration(output, vd);
            }
            output.Append("    }");
        }
    }
}
