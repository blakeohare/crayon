using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pastel.Nodes;

namespace LangC
{
    public abstract class CTranslator : Platform.CurlyBraceTranslator
    {
        public StringTableBuilder StringTableBuilder { get; set; }

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
                sb.Append('(');
                this.TranslateExpression(sb, array);
                sb.Append(")[-1]");
            }
            else
            {
                sb.Append("((int*)(");
                this.TranslateExpression(sb, array);
                sb.Append("))[-1]");
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

        public override void TranslateBooleanConstant(StringBuilder sb, bool value)
        {
            sb.Append(value ? "1" : "0");;
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
            sb.Append("TranslationHelper_get_value_struct_list_from_dictionary_values(");
            this.TranslateExpression(sb, dictionary);
            sb.Append(')');
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
            sb.Append(") != NULL");
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
            sb.Append("TranslationHelper_get_value_struct_list_from_dictionary_keys(");
            this.TranslateExpression(sb, dictionary);
            sb.Append(')');
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
            sb.Append("Dictionary_remove_");
            sb.Append(this.GetDictionaryKeyType(dictionary.ResolvedType.Generics[0]));
            sb.Append('(');
            this.TranslateExpression(sb, key);
            sb.Append(')');
        }

        private string GetDictionaryKeyType(PType type)
        {
            switch (type.RootValue)
            {
                case "int":
                    return "int";
                case "string":
                    return "str";
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
            sb.Append("LibraryHelper_invoke_function(");
            this.TranslateExpression(sb, functionId);
            sb.Append(", ");
            this.TranslateExpression(sb, argsArray);
            sb.Append(')');
        }

        public override void TranslateIsValidInteger(StringBuilder sb, Expression stringValue)
        {
            sb.Append("TranslationHelper_is_str_a_valid_int(");
            this.TranslateExpression(sb, stringValue);
            sb.Append(')');
        }

        public override void TranslateListAdd(StringBuilder sb, Expression list, Expression item)
        {
            sb.Append("List_add_");
            sb.Append(this.GetDictionaryValueType(list.ResolvedType.Generics[0]));
            sb.Append('(');
            this.TranslateExpression(sb, list);
            sb.Append(", ");
            this.TranslateExpression(sb, item);
            sb.Append(')');
        }

        public override void TranslateListClear(StringBuilder sb, Expression list)
        {
            sb.Append("List_clear(");
            this.TranslateExpression(sb, list);
            sb.Append(')');
        }

        public override void TranslateListConcat(StringBuilder sb, Expression list, Expression items)
        {
            sb.Append("List_concat(");
            this.TranslateExpression(sb, list);
            sb.Append(", ");
            this.TranslateExpression(sb, items);
            sb.Append(')');
        }

        public override void TranslateListGet(StringBuilder sb, Expression list, Expression index)
        {
            this.TranslateExpression(sb, list);
            sb.Append("->");
            sb.Append(this.GetDictionaryValueType(list.ResolvedType.Generics[0]));
            sb.Append("_items[");
            this.TranslateExpression(sb, index);
            sb.Append(']');
        }

        public override void TranslateListInsert(StringBuilder sb, Expression list, Expression index, Expression item)
        {
            sb.Append("List_insert(");
            this.TranslateExpression(sb, list);
            sb.Append(", ");
            this.TranslateExpression(sb, index);
            sb.Append(", ");
            this.TranslateExpression(sb, item);
            sb.Append(')');
        }

        public override void TranslateListJoinChars(StringBuilder sb, Expression list)
        {
            sb.Append("TranslationHelper_list_join_chars(");
            this.TranslateExpression(sb, list);
            sb.Append(')');
        }

        public override void TranslateListJoinStrings(StringBuilder sb, Expression list, Expression sep)
        {
            sb.Append("TranslationHelper_list_join_strings(");
            this.TranslateExpression(sb, list);
            sb.Append(", ");
            this.TranslateExpression(sb, sep);
            sb.Append(')');
        }

        public override void TranslateListNew(StringBuilder sb, PType type)
        {
            sb.Append("List_new(sizeof(");
            sb.Append(this.Platform.TranslateType(type));
            sb.Append("))");
        }

        public override void TranslateListPop(StringBuilder sb, Expression list)
        {
            sb.Append("List_pop(");
            this.TranslateExpression(sb, list);
            sb.Append(')');
        }

        public override void TranslateListRemoveAt(StringBuilder sb, Expression list, Expression index)
        {
            sb.Append("List_remove(");
            this.TranslateExpression(sb, list);
            sb.Append(", ");
            this.TranslateExpression(sb, index);
            sb.Append(')');
        }

        public override void TranslateListReverse(StringBuilder sb, Expression list)
        {
            sb.Append("List_reverse(");
            this.TranslateExpression(sb, list);
            sb.Append(')');
        }

        public override void TranslateListSet(StringBuilder sb, Expression list, Expression index, Expression value)
        {
            this.TranslateExpression(sb, list);
            sb.Append("->");
            sb.Append(this.GetDictionaryValueType(list.ResolvedType.Generics[0]));
            sb.Append("_items[");
            this.TranslateExpression(sb, index);
            sb.Append("] = ");
            this.TranslateExpression(sb, value);
        }

        public override void TranslateListShuffle(StringBuilder sb, Expression list)
        {
            sb.Append("TranslationHelper_list_shuffle(");
            this.TranslateExpression(sb, list);
            sb.Append(')');
        }

        public override void TranslateListSize(StringBuilder sb, Expression list)
        {
            this.TranslateExpression(sb, list);
            sb.Append("->size");
        }

        public override void TranslateListToArray(StringBuilder sb, Expression list)
        {
            sb.Append("List_to_array(");
            this.TranslateExpression(sb, list);
            sb.Append(')');
        }

        public override void TranslateMathArcCos(StringBuilder sb, Expression ratio)
        {
            sb.Append("TranslationHelper_math_arc_cos(");
            this.TranslateExpression(sb, ratio);
            sb.Append(')');
        }

        public override void TranslateMathArcSin(StringBuilder sb, Expression ratio)
        {
            sb.Append("TranslationHelper_math_arc_sin(");
            this.TranslateExpression(sb, ratio);
            sb.Append(')');
        }

        public override void TranslateMathArcTan(StringBuilder sb, Expression yComponent, Expression xComponent)
        {
            sb.Append("TranslationHelper_math_arc_tan(");
            this.TranslateExpression(sb, yComponent);
            sb.Append(", ");
            this.TranslateExpression(sb, xComponent);
            sb.Append(')');
        }

        public override void TranslateMathCos(StringBuilder sb, Expression thetaRadians)
        {
            sb.Append("TranslationHelper_math_cos(");
            this.TranslateExpression(sb, thetaRadians);
            sb.Append(')');
        }

        public override void TranslateMathLog(StringBuilder sb, Expression value)
        {
            sb.Append("TranslationHelper_math_log(");
            this.TranslateExpression(sb, value);
            sb.Append(')');
        }

        public override void TranslateMathPow(StringBuilder sb, Expression expBase, Expression exponent)
        {
            sb.Append("TranslationHelper_math_pow(");
            this.TranslateExpression(sb, expBase);
            sb.Append(", ");
            this.TranslateExpression(sb, exponent);
            sb.Append(')');
        }

        public override void TranslateMathSin(StringBuilder sb, Expression thetaRadians)
        {
            sb.Append("TranslationHelper_math_sin(");
            this.TranslateExpression(sb, thetaRadians);
            sb.Append(')');
        }

        public override void TranslateMathTan(StringBuilder sb, Expression thetaRadians)
        {
            sb.Append("TranslationHelper_math_tan(");
            this.TranslateExpression(sb, thetaRadians);
            sb.Append(')');
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

        public override void TranslateOpChain(StringBuilder sb, OpChain opChain)
        {
            if (opChain.Expressions.Length == 2)
            {
                // Avoid parenthesis. Extraneous parenthesis are actually warnings in C for these operators.
                switch (opChain.Ops[0].Value)
                {
                    case "==":
                    case "!=":
                    case ">":
                    case "<":
                    case "<=":
                    case ">=":
                        this.TranslateExpression(sb, opChain.Expressions[0]);
                        sb.Append(' ');
                        sb.Append(opChain.Ops[0].Value);
                        sb.Append(' ');
                        this.TranslateExpression(sb, opChain.Expressions[1]);
                        return;

                    default: break;
                }
            }
            base.TranslateOpChain(sb, opChain);
        }

        public override void TranslateOrd(StringBuilder sb, Expression charValue)
        {
            throw new Exception();
        }

        public override void TranslateParseFloatUnsafe(StringBuilder sb, Expression stringValue)
        {
            sb.Append("TranslationHelper_parse_float_with_trusted_input(");
            this.TranslateExpression(sb, stringValue);
            sb.Append(')');
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
            sb.Append("TranslationHeper_random_float()");
        }

        public override void TranslateReadByteCodeFile(StringBuilder sb)
        {
            sb.Append("BYTE_CODE_FILE");
        }

        public override void TranslateRegisterLibraryFunction(
            StringBuilder sb,
            Expression libRegObj,
            Expression functionName,
            Expression functionArgCount)
        {
            sb.Append("LibraryHelper_register_function(");
            this.TranslateExpression(sb, libRegObj);
            sb.Append(", ");
            this.TranslateExpression(sb, functionName);
            sb.Append(", ");
            this.TranslateExpression(sb, functionArgCount);
            sb.Append(')');
        }

        public override void TranslateResourceReadTextFile(StringBuilder sb, Expression path)
        {
            sb.Append("ResourceHelper_read_text_file(");
            this.TranslateExpression(sb, path);
            sb.Append(')');
        }

        public override void TranslateSetProgramData(StringBuilder sb, Expression programData)
        {
            sb.Append("PROGRAM_DATA = ");
            this.TranslateExpression(sb, programData);
        }

        public override void TranslateSortedCopyOfIntArray(StringBuilder sb, Expression intArray)
        {
            sb.Append("TranslationHelper_array_sorted_copy_int(");
            this.TranslateExpression(sb, intArray);
            sb.Append(')');
        }

        public override void TranslateSortedCopyOfStringArray(StringBuilder sb, Expression stringArray)
        {
            sb.Append("TranslationHelper_array_sorted_copy_int(");
            this.TranslateExpression(sb, stringArray);
            sb.Append(')');
        }

        public override void TranslateStringAppend(StringBuilder sb, Expression str1, Expression str2)
        {
            // "stringRef += newSuffix;" is not supported.
            // throw new Exception();
            sb.Append("TODO(\"Replace these calls with basic string concats\")");
        }

        public override void TranslateStringBuffer16(StringBuilder sb)
        {
            sb.Append("STRING_BUFFER_16");
        }

        public override void TranslateStringCharAt(StringBuilder sb, Expression str, Expression index)
        {
            this.TranslateExpression(sb, str);
            sb.Append('[');
            this.TranslateExpression(sb, index);
            sb.Append(']');
        }

        public override void TranslateStringCharCodeAt(StringBuilder sb, Expression str, Expression index)
        {
            this.TranslateStringCharAt(sb, str, index);
        }

        public override void TranslateStringCompareIsReverse(StringBuilder sb, Expression str1, Expression str2)
        {
            sb.Append("String_compare_is_reverse(");
            this.TranslateExpression(sb, str1);
            sb.Append(", ");
            this.TranslateExpression(sb, str2);
            sb.Append(')');
        }

        public override void TranslateStringConcatAll(StringBuilder sb, Expression[] strings)
        {
            this.TranslateStringConcatAllImpl(sb, strings, 0);
        }

        private void TranslateStringConcatAllImpl(StringBuilder sb, Expression[] strings, int startIndex)
        {
            // TODO: this is ugly and ineffective, but it's also rarely used.
            // Create larger argument versions.
            // TODO: worry about memory leaks for intermediate strings
            int total = strings.Length - startIndex;
            if (total == 1)
            {
                this.TranslateExpression(sb, strings[startIndex]);
            }
            else
            {
                sb.Append("String_concat(");
                this.TranslateExpression(sb, strings[startIndex]);
                sb.Append(", ");
                this.TranslateStringConcatAllImpl(sb, strings, startIndex + 1);
                sb.Append(')');
            }
        }

        public override void TranslateStringConcatPair(StringBuilder sb, Expression strLeft, Expression strRight)
        {
            sb.Append("String_concat(");
            this.TranslateExpression(sb, strLeft);
            sb.Append(", ");
            this.TranslateExpression(sb, strRight);
            sb.Append(')');
        }

        public override void TranslateStringConstant(StringBuilder sb, string value)
        {
            sb.Append(this.StringTableBuilder.GetId(value));
            sb.Append("/* ");
            sb.Append(Common.Util.ConvertStringValueToCode(value).Replace("*/", "* /"));
            sb.Append(" */");
        }

        public override void TranslateStringContains(StringBuilder sb, Expression haystack, Expression needle)
        {
            sb.Append("String_contains(");
            this.TranslateExpression(sb, haystack);
            sb.Append(", ");
            this.TranslateExpression(sb, needle);
            sb.Append(')');
        }

        public override void TranslateStringEndsWith(StringBuilder sb, Expression haystack, Expression needle)
        {
            sb.Append("String_ends_with(");
            this.TranslateExpression(sb, haystack);
            sb.Append(", ");
            this.TranslateExpression(sb, needle);
            sb.Append(')');
        }

        public override void TranslateStringEquals(StringBuilder sb, Expression left, Expression right)
        {
            sb.Append("String_equals(");
            this.TranslateExpression(sb, left);
            sb.Append(", ");
            this.TranslateExpression(sb, right);
            sb.Append(')');
        }

        public override void TranslateStringFromCharCode(StringBuilder sb, Expression charCode)
        {
            sb.Append("TranslationHelper_string_form_char_code(");
            this.TranslateExpression(sb, charCode);
            sb.Append(')');
        }

        public override void TranslateStringIndexOf(StringBuilder sb, Expression haystack, Expression needle)
        {
            sb.Append("String_index_of(");
            this.TranslateExpression(sb, haystack);
            sb.Append(", ");
            this.TranslateExpression(sb, needle);
            sb.Append(')');
        }

        public override void TranslateStringIndexOfWithStart(StringBuilder sb, Expression haystack, Expression needle, Expression startIndex)
        {
            sb.Append("String_index_of_with_start_index(");
            this.TranslateExpression(sb, haystack);
            sb.Append(", ");
            this.TranslateExpression(sb, needle);
            sb.Append(", ");
            this.TranslateExpression(sb, startIndex);
            sb.Append(')');
        }

        public override void TranslateStringLength(StringBuilder sb, Expression str)
        {
            this.TranslateExpression(sb, str);
            sb.Append("[-1]");
        }

        public override void TranslateStringReplace(StringBuilder sb, Expression haystack, Expression needle, Expression newNeedle)
        {
            sb.Append("String_replace(");
            this.TranslateExpression(sb, haystack);
            sb.Append(", ");
            this.TranslateExpression(sb, needle);
            sb.Append(", ");
            this.TranslateExpression(sb, newNeedle);
            sb.Append(')');
        }

        public override void TranslateStringReverse(StringBuilder sb, Expression str)
        {
            sb.Append("String_reverse(");
            this.TranslateExpression(sb, str);
            sb.Append(')');
        }

        public override void TranslateStringSplit(StringBuilder sb, Expression haystack, Expression needle)
        {
            sb.Append("String_split(");
            this.TranslateExpression(sb, haystack);
            sb.Append(", ");
            this.TranslateExpression(sb, needle);
            sb.Append(')');
        }

        public override void TranslateStringStartsWith(StringBuilder sb, Expression haystack, Expression needle)
        {
            sb.Append("String_starts_with(");
            this.TranslateExpression(sb, haystack);
            sb.Append(", ");
            this.TranslateExpression(sb, needle);
            sb.Append(')');
        }

        public override void TranslateStringSubstring(StringBuilder sb, Expression str, Expression start, Expression length)
        {
            sb.Append("String_substring(");
            this.TranslateExpression(sb, str);
            sb.Append(", ");
            this.TranslateExpression(sb, start);
            sb.Append(", ");
            this.TranslateExpression(sb, length);
            sb.Append(')');
        }

        public override void TranslateStringSubstringIsEqualTo(StringBuilder sb, Expression haystack, Expression startIndex, Expression needle)
        {
            sb.Append("String_substring_is_equal_to(");
            this.TranslateExpression(sb, haystack);
            sb.Append(", ");
            this.TranslateExpression(sb, startIndex);
            sb.Append(", ");
            this.TranslateExpression(sb, needle);
            sb.Append(')');
        }

        public override void TranslateStringToLower(StringBuilder sb, Expression str)
        {
            sb.Append("String_to_lower(");
            this.TranslateExpression(sb, str);
            sb.Append(')');
        }

        public override void TranslateStringToUpper(StringBuilder sb, Expression str)
        {
            sb.Append("String_to_upper(");
            this.TranslateExpression(sb, str);
            sb.Append(')');
        }

        public override void TranslateStringTrim(StringBuilder sb, Expression str)
        {
            sb.Append("String_trim(");
            this.TranslateExpression(sb, str);
            sb.Append(", 1, 1)");
        }

        public override void TranslateStringTrimEnd(StringBuilder sb, Expression str)
        {
            sb.Append("String_trim(");
            this.TranslateExpression(sb, str);
            sb.Append(", 0, 1)");
        }

        public override void TranslateStringTrimStart(StringBuilder sb, Expression str)
        {
            sb.Append("String_trim(");
            this.TranslateExpression(sb, str);
            sb.Append(", 1, 0)");
        }

        public override void TranslateStrongReferenceEquality(StringBuilder sb, Expression left, Expression right)
        {
            string type = left.ResolvedType.RootValue;

            if (right.ResolvedType.RootValue != type)
            {
                // this should have been stopped with a type check in the VM source
                throw new Exception();
            }

            switch (type)
            {
                case "object":
                case "Array":
                case "List":
                case "Dictionary":
                    // direct pointer comparison
                    this.TranslateExpression(sb, left);
                    sb.Append(" == ");
                    this.TranslateExpression(sb, right);
                    break;

                default:
                    // This shouldn't be necessary
                    // Strings should have used string equality comparisons
                    throw new Exception();
            }
        }

        public override void TranslateStructFieldDereference(StringBuilder sb, Expression root, StructDefinition structDef, string fieldName, int fieldIndex)
        {
            this.TranslateExpression(sb, root);
            sb.Append("->");
            sb.Append(fieldName);
        }

        public override void TranslateThreadSleep(StringBuilder sb, Expression seconds)
        {
            sb.Append("TranslationHelper_thread_sleep(");
            this.TranslateExpression(sb, seconds);
            sb.Append(')');
        }

        public override void TranslateTryParseFloat(StringBuilder sb, Expression stringValue, Expression floatOutList)
        {
            sb.Append("TranslationHelper_try_parse_float(");
            this.TranslateExpression(sb, stringValue);
            sb.Append(", ");
            this.TranslateExpression(sb, floatOutList);
            sb.Append(')');
        }

        public override void TranslateVariableDeclaration(StringBuilder sb, VariableDeclaration varDecl)
        {
            sb.Append(this.CurrentTab);
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
        }

        public override void TranslateVmDetermineLibraryAvailability(StringBuilder sb, Expression libraryName, Expression libraryVersion)
        {
            sb.Append("LibraryHelper_determine_library_availability(");
            this.TranslateExpression(sb, libraryName);
            sb.Append(", ");
            this.TranslateExpression(sb, libraryVersion);
            sb.Append(')');
        }

        public override void TranslateVmEnqueueResume(StringBuilder sb, Expression seconds, Expression executionContextId)
        {
            throw new Exception();
        }

        public override void TranslateVmRunLibraryManifest(StringBuilder sb, Expression libraryName, Expression libRegObj)
        {
            sb.Append("LibraryHelper_run_library_manifest(");
            this.TranslateExpression(sb, libraryName);
            sb.Append(", ");
            this.TranslateExpression(sb, libRegObj);
            sb.Append(')');
        }
    }
}
