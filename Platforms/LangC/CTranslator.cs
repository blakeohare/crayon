using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pastel.Nodes;

namespace LangC
{
    public abstract class CTranslator : Platform.CurlyBraceTranslator
    {
        public CTranslator(Platform.AbstractPlatform platform) : base(platform, "    ", "\n", false)
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
            sb.Append("TranslationHelper_array_join(");
            this.TranslateExpression(sb, array);
            sb.Append(", ");
            this.TranslateExpression(sb, sep);
            sb.Append(')');
        }

        public override void TranslateArrayLength(StringBuilder sb, Expression array)
        {
            PType itemType = array.ResolvedType.Generics[0];
            if (itemType.RootValue == "int")
            {
                this.TranslateExpression(sb, array);
                sb.Append("[-1]");
            }
            else
            {
                sb.Append("(int*)(");
                this.TranslateExpression(sb, array);
                sb.Append(")[-1]");
            }
        }

        public override void TranslateArrayNew(StringBuilder sb, PType arrayType, Expression lengthExpression)
        {
            // TODO: I'd like to make a distinction (in the VM source) between arrays that need to have array
            // length and arrays that don't. If array length isn't needed, the malloc can go inline and it saves
            // an extra 4 bytes of space and time.
            // Potentially there'd be a virtual function called TranslateArrayNewWithoutLength that just calls this
            // and so it won't clutter up the interface implementation for 90% of languages that don't need the
            // distinction.t
            string type = this.Platform.TranslateType(arrayType);
            sb.Append("(");
            sb.Append(type);
            sb.Append("*)TranslationHelper_array_new(sizeof(");
            sb.Append(type);
            sb.Append(") * (");
            this.TranslateExpression(sb, lengthExpression);
            sb.Append("))");

        }

        public override void TranslateArraySet(StringBuilder sb, Expression array, Expression index, Expression value)
        {
            this.TranslateExpression(sb, array);
            sb.Append('[');
            this.TranslateExpression(sb, index);
            sb.Append("] = ");
            this.TranslateExpression(sb, value);
        }

        public override void TranslateCast(StringBuilder sb, PType type, Expression expression)
        {
            sb.Append("((");
            sb.Append(this.Platform.TranslateType(type));
            sb.Append(")(");
            this.TranslateExpression(sb, expression);
            sb.Append("))");
        }

        public override void TranslateCharConstant(StringBuilder sb, char value)
        {
            sb.Append(Common.Util.ConvertCharToCharConstantCode(value));
        }

        public override void TranslateCharToString(StringBuilder sb, Expression charValue)
        {
            sb.Append("TranslationHelper_char_to_string(");
            this.TranslateExpression(sb, charValue);
            sb.Append(')');
        }

        public override void TranslateChr(StringBuilder sb, Expression charCode)
        {
            sb.Append("(int)(");
            this.TranslateExpression(sb, charCode);
            sb.Append(')');
        }

        public override void TranslateCommandLineArgs(StringBuilder sb)
        {
            sb.Append("TranslationHelper_get_command_line_args()");
        }

        public override void TranslateConstructorInvocation(StringBuilder sb, ConstructorInvocation constructorInvocation)
        {
            sb.Append(constructorInvocation.StructType.NameToken.Value);
            sb.Append("_new(");
            Expression[] args = constructorInvocation.Args;
            for (int i = 0; i < args.Length; ++i)
            {
                if (i > 0) sb.Append(", ");
                this.TranslateExpression(sb, args[i]);
            }
            sb.Append(')');
        }

        public override void TranslateConvertRawDictionaryValueCollectionToAReusableValueList(StringBuilder sb, Expression dictionary)
        {
            throw new NotImplementedException();
        }

        public override void TranslateCurrentTimeSeconds(StringBuilder sb)
        {
            sb.Append("TranslationHelper_current_time_seconds()");
        }

        public override void TranslateDictionaryContainsKey(StringBuilder sb, Expression dictionary, Expression key)
        {
            if (key.ResolvedType.RootValue == "int")
            {
                sb.Append("Dictionary_get_node_int(");
            }
            else
            {
                sb.Append("Dictionary_get_node_str(");
            }
            this.TranslateExpression(sb, dictionary);
            sb.Append(", ");
            this.TranslateExpression(sb, key);
            sb.Append(" != NULL");
        }

        public override void TranslateDictionaryGet(StringBuilder sb, Expression dictionary, Expression key)
        {
            PType dictionaryType = dictionary.ResolvedType;
            sb.Append("Dictionary_get_");
            sb.Append(this.Platform.TranslateType(dictionaryType.Generics[0]));
            sb.Append("_to_");
            sb.Append(this.Platform.TranslateType(dictionaryType.Generics[1]));
            sb.Append("(");
            this.TranslateExpression(sb, dictionary);
            sb.Append(", ");
            this.TranslateExpression(sb, key);
            sb.Append(')');
        }

        public override void TranslateDictionaryKeys(StringBuilder sb, Expression dictionary)
        {
            sb.Append("Dictionatry_get_keys_");
            sb.Append(this.GetDictionaryKeyType(dictionary.ResolvedType.Generics[0]));
            sb.Append('(');
            this.TranslateExpression(sb, dictionary);
            sb.Append(')');
        }

        public override void TranslateDictionaryKeysToValueList(StringBuilder sb, Expression dictionary)
        {
            throw new NotImplementedException();
        }

        public override void TranslateDictionaryNew(StringBuilder sb, PType keyType, PType valueType)
        {
            sb.Append("Dictionary_new(sizeof(");
            sb.Append(this.Platform.TranslateType(keyType));
            sb.Append("), sizeof(");
            sb.Append(this.Platform.TranslateType(valueType));
            sb.Append("))");
        }

        public override void TranslateDictionaryRemove(StringBuilder sb, Expression dictionary, Expression key)
        {
            throw new NotImplementedException();
        }

        private string GetDictionaryKeyType(PType type)
        {
            switch (type.RootValue)
            {
                case "int":
                case "string":
                    return type.RootValue;
                default:
                    throw new Exception("Invalid key type for dictionary.");
            }
        }

        private string GetDictionaryValueType(PType type)
        {
            switch (type.RootValue)
            {
                case "bool":
                case "int":
                    return "int";
                case "string":
                case "double":
                case "char":
                    return type.RootValue;
                default:
                    return "ptr";
            }
        }

        public override void TranslateDictionarySet(StringBuilder sb, Expression dictionary, Expression key, Expression value)
        {
            PType dictType = dictionary.ResolvedType;
            sb.Append("Dictionary_set_");
            sb.Append(this.GetDictionaryKeyType(dictType.Generics[0]));
            sb.Append("_to_");
            sb.Append(this.GetDictionaryValueType(dictType.Generics[1]));
            sb.Append('(');
            this.TranslateExpression(sb, dictionary);
            sb.Append(", ");
            this.TranslateExpression(sb, key);
            sb.Append(", ");
            this.TranslateExpression(sb, value);
            sb.Append(")");
        }

        public override void TranslateDictionarySize(StringBuilder sb, Expression dictionary)
        {
            this.TranslateExpression(sb, dictionary);
            sb.Append("->size");
        }

        public override void TranslateDictionaryValues(StringBuilder sb, Expression dictionary)
        {
            sb.Append("Dictionatry_get_values_");
            sb.Append(this.GetDictionaryValueType(dictionary.ResolvedType.Generics[1]));
            sb.Append('(');
            this.TranslateExpression(sb, dictionary);
            sb.Append(')');
        }

        public override void TranslateDictionaryValuesToValueList(StringBuilder sb, Expression dictionary)
        {
            throw new NotImplementedException();
        }

        public override void TranslateFloatBuffer16(StringBuilder sb)
        {
            sb.Append("FLOAT_BUFFER_16");
        }

        public override void TranslateFloatDivision(StringBuilder sb, Expression floatNumerator, Expression floatDenominator)
        {
            sb.Append("(1.0 * (");
            this.TranslateExpression(sb, floatNumerator);
            sb.Append(") / (");
            this.TranslateExpression(sb, floatDenominator);
            sb.Append("))");
        }

        public override void TranslateFloatToInt(StringBuilder sb, Expression floatExpr)
        {
            sb.Append("TranslationHelper_double_to_int(");
            this.TranslateExpression(sb, floatExpr);
            sb.Append(')');
        }

        public override void TranslateFloatToString(StringBuilder sb, Expression floatExpr)
        {
            sb.Append("TranslationHelper_double_to_string(");
            this.TranslateExpression(sb, floatExpr);
            sb.Append(')');
        }

        public override void TranslateGetProgramData(StringBuilder sb)
        {
            sb.Append("PROGRAM_DATA");
        }

        public override void TranslateGetResourceManifest(StringBuilder sb)
        {
            sb.Append("TranslationHelper_get_resource_manifest()");
        }

        public override void TranslateGlobalVariable(StringBuilder sb, Variable variable)
        {
            this.TranslateVariable(sb, variable);
        }

        public override void TranslateIntBuffer16(StringBuilder sb)
        {
            sb.Append("INT_BUFFER_16");
        }

        public override void TranslateIntegerDivision(StringBuilder sb, Expression integerNumerator, Expression integerDenominator)
        {
            sb.Append('(');
            this.TranslateExpression(sb, integerNumerator);
            sb.Append(") / (");
            this.TranslateExpression(sb, integerDenominator);
            sb.Append(')');
        }

        public override void TranslateIntToString(StringBuilder sb, Expression integer)
        {
            sb.Append("TranslationHelper_int_to_string(");
            this.TranslateExpression(sb, integer);
            sb.Append(')');
        }

        public override void TranslateInvokeDynamicLibraryFunction(StringBuilder sb, Expression functionId, Expression argsArray)
        {
            throw new NotImplementedException();
        }

        public override void TranslateIsValidInteger(StringBuilder sb, Expression stringValue)
        {
            sb.Append("TranslationHelper_is_str_a_valid_int(");
            this.TranslateExpression(sb, stringValue);
            sb.Append(')');
        }

        public override void TranslateListAdd(StringBuilder sb, Expression list, Expression item)
        {
            throw new NotImplementedException();
        }

        public override void TranslateListClear(StringBuilder sb, Expression list)
        {
            throw new NotImplementedException();
        }

        public override void TranslateListConcat(StringBuilder sb, Expression list, Expression items)
        {
            throw new NotImplementedException();
        }

        public override void TranslateListGet(StringBuilder sb, Expression list, Expression index)
        {
            throw new NotImplementedException();
        }

        public override void TranslateListInsert(StringBuilder sb, Expression list, Expression index, Expression item)
        {
            throw new NotImplementedException();
        }

        public override void TranslateListJoinChars(StringBuilder sb, Expression list)
        {
            throw new NotImplementedException();
        }

        public override void TranslateListJoinStrings(StringBuilder sb, Expression list, Expression sep)
        {
            throw new NotImplementedException();
        }

        public override void TranslateListNew(StringBuilder sb, PType type)
        {
            throw new NotImplementedException();
        }

        public override void TranslateListPop(StringBuilder sb, Expression list)
        {
            throw new NotImplementedException();
        }

        public override void TranslateListRemoveAt(StringBuilder sb, Expression list, Expression index)
        {
            throw new NotImplementedException();
        }

        public override void TranslateListReverse(StringBuilder sb, Expression list)
        {
            throw new NotImplementedException();
        }

        public override void TranslateListSet(StringBuilder sb, Expression list, Expression index, Expression value)
        {
            throw new NotImplementedException();
        }

        public override void TranslateListShuffle(StringBuilder sb, Expression list)
        {
            throw new NotImplementedException();
        }

        public override void TranslateListSize(StringBuilder sb, Expression list)
        {
            throw new NotImplementedException();
        }

        public override void TranslateListToArray(StringBuilder sb, Expression list)
        {
            throw new NotImplementedException();
        }

        public override void TranslateMathArcCos(StringBuilder sb, Expression ratio)
        {
            throw new NotImplementedException();
        }

        public override void TranslateMathArcSin(StringBuilder sb, Expression ratio)
        {
            throw new NotImplementedException();
        }

        public override void TranslateMathArcTan(StringBuilder sb, Expression yComponent, Expression xComponent)
        {
            throw new NotImplementedException();
        }

        public override void TranslateMathCos(StringBuilder sb, Expression thetaRadians)
        {
            throw new NotImplementedException();
        }

        public override void TranslateMathLog(StringBuilder sb, Expression value)
        {
            throw new NotImplementedException();
        }

        public override void TranslateMathPow(StringBuilder sb, Expression expBase, Expression exponent)
        {
            throw new NotImplementedException();
        }

        public override void TranslateMathSin(StringBuilder sb, Expression thetaRadians)
        {
            throw new NotImplementedException();
        }

        public override void TranslateMathTan(StringBuilder sb, Expression thetaRadians)
        {
            throw new NotImplementedException();
        }

        public override void TranslateMultiplyList(StringBuilder sb, Expression list, Expression n)
        {
            sb.Append("multiply_list(");
            this.TranslateExpression(sb, list);
            sb.Append(", ");
            this.TranslateExpression(sb, n);
            sb.Append(')');
        }

        public override void TranslateNullConstant(StringBuilder sb)
        {
            sb.Append("NULL");
        }

        public override void TranslateParseFloatUnsafe(StringBuilder sb, Expression stringValue)
        {
            throw new NotImplementedException();
        }

        public override void TranslateParseInt(StringBuilder sb, Expression safeStringValue)
        {
            sb.Append("parse_int(");
            this.TranslateExpression(sb, safeStringValue);
            sb.Append(')');
        }

        public override void TranslatePrintStdErr(StringBuilder sb, Expression value)
        {
            sb.Append("print_with_conversion(");
            this.TranslateExpression(sb, value);
            sb.Append(')');
        }

        public override void TranslatePrintStdOut(StringBuilder sb, Expression value)
        {
            sb.Append("print_with_conversion(");
            this.TranslateExpression(sb, value);
            sb.Append(')');
        }

        public override void TranslateRandomFloat(StringBuilder sb)
        {
            throw new NotImplementedException();
        }

        public override void TranslateReadByteCodeFile(StringBuilder sb)
        {
            throw new NotImplementedException();
        }

        public override void TranslateRegisterLibraryFunction(StringBuilder sb, Expression libRegObj, Expression functionName, Expression functionArgCount)
        {
            throw new NotImplementedException();
        }

        public override void TranslateResourceReadTextFile(StringBuilder sb, Expression path)
        {
            throw new NotImplementedException();
        }

        public override void TranslateSetProgramData(StringBuilder sb, Expression programData)
        {
            throw new NotImplementedException();
        }

        public override void TranslateSortedCopyOfIntArray(StringBuilder sb, Expression intArray)
        {
            throw new NotImplementedException();
        }

        public override void TranslateSortedCopyOfStringArray(StringBuilder sb, Expression stringArray)
        {
            throw new NotImplementedException();
        }

        public override void TranslateStringAppend(StringBuilder sb, Expression str1, Expression str2)
        {
            throw new NotImplementedException();
        }

        public override void TranslateStringBuffer16(StringBuilder sb)
        {
            sb.Append("STRING_BUFFER_16");
        }

        public override void TranslateStringCharAt(StringBuilder sb, Expression str, Expression index)
        {
            throw new NotImplementedException();
        }

        public override void TranslateStringCharCodeAt(StringBuilder sb, Expression str, Expression index)
        {
            throw new NotImplementedException();
        }

        public override void TranslateStringCompareIsReverse(StringBuilder sb, Expression str1, Expression str2)
        {
            throw new NotImplementedException();
        }

        public override void TranslateStringConcatAll(StringBuilder sb, Expression[] strings)
        {
            throw new NotImplementedException();
        }

        public override void TranslateStringConcatPair(StringBuilder sb, Expression strLeft, Expression strRight)
        {
            throw new NotImplementedException();
        }

        public override void TranslateStringContains(StringBuilder sb, Expression haystack, Expression needle)
        {
            throw new NotImplementedException();
        }

        public override void TranslateStringEndsWith(StringBuilder sb, Expression haystack, Expression needle)
        {
            throw new NotImplementedException();
        }

        public override void TranslateStringEquals(StringBuilder sb, Expression left, Expression right)
        {
            throw new NotImplementedException();
        }

        public override void TranslateStringFromCharCode(StringBuilder sb, Expression charCode)
        {
            throw new NotImplementedException();
        }

        public override void TranslateStringIndexOf(StringBuilder sb, Expression haystack, Expression needle)
        {
            throw new NotImplementedException();
        }

        public override void TranslateStringIndexOfWithStart(StringBuilder sb, Expression haystack, Expression needle, Expression startIndex)
        {
            throw new NotImplementedException();
        }

        public override void TranslateStringLength(StringBuilder sb, Expression str)
        {
            this.TranslateExpression(sb, str);
            sb.Append("[-1]");
        }

        public override void TranslateStringReplace(StringBuilder sb, Expression haystack, Expression needle, Expression newNeedle)
        {
            throw new NotImplementedException();
        }

        public override void TranslateStringReverse(StringBuilder sb, Expression str)
        {
            throw new NotImplementedException();
        }

        public override void TranslateStringSplit(StringBuilder sb, Expression haystack, Expression needle)
        {
            throw new NotImplementedException();
        }

        public override void TranslateStringStartsWith(StringBuilder sb, Expression haystack, Expression needle)
        {
            throw new NotImplementedException();
        }

        public override void TranslateStringSubstring(StringBuilder sb, Expression str, Expression start, Expression length)
        {
            throw new NotImplementedException();
        }

        public override void TranslateStringSubstringIsEqualTo(StringBuilder sb, Expression haystack, Expression startIndex, Expression needle)
        {
            throw new NotImplementedException();
        }

        public override void TranslateStringToLower(StringBuilder sb, Expression str)
        {
            throw new NotImplementedException();
        }

        public override void TranslateStringToUpper(StringBuilder sb, Expression str)
        {
            throw new NotImplementedException();
        }

        public override void TranslateStringTrim(StringBuilder sb, Expression str)
        {
            throw new NotImplementedException();
        }

        public override void TranslateStringTrimEnd(StringBuilder sb, Expression str)
        {
            throw new NotImplementedException();
        }

        public override void TranslateStringTrimStart(StringBuilder sb, Expression str)
        {
            throw new NotImplementedException();
        }

        public override void TranslateStrongReferenceEquality(StringBuilder sb, Expression left, Expression right)
        {
            throw new NotImplementedException();
        }

        public override void TranslateStructFieldDereference(StringBuilder sb, Expression root, StructDefinition structDef, string fieldName, int fieldIndex)
        {
            this.TranslateExpression(sb, root);
            sb.Append("->");
            sb.Append(fieldName);
        }

        public override void TranslateThreadSleep(StringBuilder sb, Expression seconds)
        {
            throw new NotImplementedException();
        }

        public override void TranslateTryParseFloat(StringBuilder sb, Expression stringValue, Expression floatOutList)
        {
            throw new NotImplementedException();
        }

        public override void TranslateVariableDeclaration(StringBuilder sb, VariableDeclaration varDecl)
        {
            sb.Append(this.Platform.TranslateType(varDecl.Type));
            sb.Append(" v_");
            sb.Append(varDecl.VariableNameToken.Value);
            if (varDecl.Value != null)
            {
                sb.Append(" = ");
                this.TranslateExpression(sb, varDecl.Value);
            }
            sb.Append(';');
            sb.Append(this.NewLine);
            sb.Append(this.CurrentTab);
        }

        public override void TranslateVmDetermineLibraryAvailability(StringBuilder sb, Expression libraryName, Expression libraryVersion)
        {
            throw new NotImplementedException();
        }

        public override void TranslateVmEnqueueResume(StringBuilder sb, Expression seconds, Expression executionContextId)
        {
            throw new NotImplementedException();
        }

        public override void TranslateVmRunLibraryManifest(StringBuilder sb, Expression libraryName, Expression libRegObj)
        {
            throw new NotImplementedException();
        }
    }
}
