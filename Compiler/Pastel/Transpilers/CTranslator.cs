using Pastel.Nodes;
using System;
using System.Collections.Generic;

namespace Pastel.Transpilers
{
    internal class CTranslator : CurlyBraceTranslator
    {
        public CTranslator()
            : base("    ", "\n", false)
        {
            this.UsesStringTable = true;
            this.UsesStructDeclarations = true;
            this.UsesFunctionDeclarations = true;
        }

        public override string TranslateType(PType type)
        {
            switch (type.RootValue)
            {
                case "int": return "int";
                case "string": return "int*";
                case "bool": return "int";
                case "double": return "double";
                case "object": return "void*";
                case "char": return "int";
                case "List": return "List*";
                case "Array": return this.TranslateType(type.Generics[0]) + "*";
                case "Dictionary":
                    string keyType = type.Generics[0].RootValue;
                    switch (keyType)
                    {
                        case "int":
                        case "string":
                            return "Dictionary*";
                        default:
                            throw new NotImplementedException();
                    }
                default: break;
            }

            char firstChar = type.RootValue[0];
            if (firstChar >= 'A' && firstChar <= 'Z') return type.RootValue + "*";

            throw new NotImplementedException();
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
            sb.Append("TranslationHelper_array_join(");
            this.TranslateExpression(sb, array);
            sb.Append(", ");
            this.TranslateExpression(sb, sep);
            sb.Append(')');
        }

        public override void TranslateArrayLength(TranspilerContext sb, Expression array)
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

        public override void TranslateArrayNew(TranspilerContext sb, PType arrayType, Expression lengthExpression)
        {
            // TODO: I'd like to make a distinction (in the VM source) between arrays that need to have array
            // length and arrays that don't. If array length isn't needed, the malloc can go inline and it saves
            // an extra 4 bytes of space and time.
            // Potentially there'd be a virtual function called TranslateArrayNewWithoutLength that just calls this
            // and so it won't clutter up the interface implementation for 90% of languages that don't need the
            // distinction.t
            string type = this.TranslateType(arrayType);
            sb.Append("(");
            sb.Append(type);
            sb.Append("*)TranslationHelper_array_new(sizeof(");
            sb.Append(type);
            sb.Append(") * (");
            this.TranslateExpression(sb, lengthExpression);
            sb.Append("))");

        }

        public override void TranslateArraySet(TranspilerContext sb, Expression array, Expression index, Expression value)
        {
            this.TranslateExpression(sb, array);
            sb.Append('[');
            this.TranslateExpression(sb, index);
            sb.Append("] = ");
            this.TranslateExpression(sb, value);
        }

        public override void TranslateAssignment(TranspilerContext sb, Assignment assignment)
        {
            DotField df = assignment.Target as DotField;
            if (df != null && df.FieldName.Value == "internalValue" && df.Root.ResolvedType.RootValue == "Value")
            {
                bool isVar = df.Root is Variable;
                if (!isVar) sb.Append('(');
                this.TranslateExpression(sb, df.Root);
                if (!isVar) sb.Append(')');
                sb.Append("->");
                sb.Append(this.GetValueStructInternalValueFieldName(assignment.Value.ResolvedType));
                sb.Append(' ');
                sb.Append(assignment.OpToken.Value);
                sb.Append(' ');
                this.TranslateExpression(sb, assignment.Value);
                sb.Append(';');
                sb.Append(this.NewLine);
            }
            else
            {
                base.TranslateAssignment(sb, assignment);
            }
        }

        public override void TranslateBase64ToString(TranspilerContext sb, Expression base64String)
        {
            sb.Append("TranslationHelper_base64ToString(");
            this.TranslateExpression(sb, base64String);
            sb.Append(")");
        }

        public override void TranslateBooleanConstant(TranspilerContext sb, bool value)
        {
            sb.Append(value ? "1" : "0");
        }

        private string GetValueStructInternalValueFieldName(PType expectedType)
        {
            switch (expectedType.RootValue)
            {
                case "bool": return "bool_internalValue";
                case "int": return "int_internalValue";
                case "double": return "double_internalValue";
                case "string": return "string_internalValue";
                case "List": return "list_internalValue";
                case "DictImpl": return "dict_internalValue";
                case "FunctionPointer": return "func_internalValue";
                case "ObjectInstance": return "obj_internalValue";
                case "ClassValue": return "class_internalValue";
                default:
                    throw new NotImplementedException();
            }
        }

        public override void TranslateCast(TranspilerContext sb, PType type, Expression expression)
        {
            if (expression is DotField)
            {
                DotField df = (DotField)expression;
                if (df.FieldName.Value == "internalValue" && df.Root.ResolvedType.RootValue == "Value")
                {
                    if (df.Root is Variable)
                    {
                        this.TranslateExpression(sb, df.Root);
                    }
                    else
                    {
                        sb.Append('(');
                        this.TranslateExpression(sb, df.Root);
                        sb.Append(')');
                    }
                    sb.Append("->");
                    sb.Append(this.GetValueStructInternalValueFieldName(type));
                    return;
                }
            }
            sb.Append("((");
            sb.Append(this.TranslateType(type));
            sb.Append(")(");
            this.TranslateExpression(sb, expression);
            sb.Append("))");
        }

        public override void TranslateCharConstant(TranspilerContext sb, char value)
        {
            sb.Append(PastelUtil.ConvertCharToCharConstantCode(value));
        }

        public override void TranslateCharToString(TranspilerContext sb, Expression charValue)
        {
            sb.Append("TranslationHelper_char_to_string(");
            this.TranslateExpression(sb, charValue);
            sb.Append(')');
        }

        public override void TranslateChr(TranspilerContext sb, Expression charCode)
        {
            sb.Append("(int)(");
            this.TranslateExpression(sb, charCode);
            sb.Append(')');
        }

        public override void TranslateCommandLineArgs(TranspilerContext sb)
        {
            sb.Append("TranslationHelper_get_command_line_args()");
        }

        public override void TranslateConstructorInvocation(TranspilerContext sb, ConstructorInvocation constructorInvocation)
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

        public override void TranslateConvertRawDictionaryValueCollectionToAReusableValueList(TranspilerContext sb, Expression dictionary)
        {
            sb.Append("TranslationHelper_get_value_struct_list_from_dictionary_values(");
            this.TranslateExpression(sb, dictionary);
            sb.Append(')');
        }

        public override void TranslateCurrentTimeSeconds(TranspilerContext sb)
        {
            sb.Append("TranslationHelper_current_time_seconds()");
        }

        public override void TranslateDictionaryContainsKey(TranspilerContext sb, Expression dictionary, Expression key)
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

        public override void TranslateDictionaryGet(TranspilerContext sb, Expression dictionary, Expression key)
        {
            PType dictionaryType = dictionary.ResolvedType;
            sb.Append("Dictionary_get_");
            sb.Append(this.TranslateType(dictionaryType.Generics[0]));
            sb.Append("_to_");
            sb.Append(this.TranslateType(dictionaryType.Generics[1]));
            sb.Append("(");
            this.TranslateExpression(sb, dictionary);
            sb.Append(", ");
            this.TranslateExpression(sb, key);
            sb.Append(')');
        }

        public override void TranslateDictionaryKeys(TranspilerContext sb, Expression dictionary)
        {
            sb.Append("Dictionary_get_keys_");
            sb.Append(this.GetDictionaryKeyType(dictionary.ResolvedType.Generics[0]));
            sb.Append('(');
            this.TranslateExpression(sb, dictionary);
            sb.Append(')');
        }

        public override void TranslateDictionaryKeysToValueList(TranspilerContext sb, Expression dictionary)
        {
            sb.Append("TranslationHelper_get_value_struct_list_from_dictionary_keys(");
            this.TranslateExpression(sb, dictionary);
            sb.Append(')');
        }

        public override void TranslateDictionaryNew(TranspilerContext sb, PType keyType, PType valueType)
        {
            sb.Append("Dictionary_new(sizeof(");
            sb.Append(this.TranslateType(keyType));
            sb.Append("), sizeof(");
            sb.Append(this.TranslateType(valueType));
            sb.Append("))");
        }

        public override void TranslateDictionaryRemove(TranspilerContext sb, Expression dictionary, Expression key)
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

        public override void TranslateDictionarySet(TranspilerContext sb, Expression dictionary, Expression key, Expression value)
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

        public override void TranslateDictionarySize(TranspilerContext sb, Expression dictionary)
        {
            this.TranslateExpression(sb, dictionary);
            sb.Append("->size");
        }

        public override void TranslateDictionaryValues(TranspilerContext sb, Expression dictionary)
        {
            sb.Append("Dictionary_get_values_");
            sb.Append(this.GetDictionaryValueType(dictionary.ResolvedType.Generics[1]));
            sb.Append('(');
            this.TranslateExpression(sb, dictionary);
            sb.Append(')');
        }

        public override void TranslateDictionaryValuesToValueList(TranspilerContext sb, Expression dictionary)
        {
            throw new NotImplementedException();
        }

        public override void TranslateFloatBuffer16(TranspilerContext sb)
        {
            sb.Append("FLOAT_BUFFER_16");
        }

        public override void TranslateFloatDivision(TranspilerContext sb, Expression floatNumerator, Expression floatDenominator)
        {
            sb.Append("(1.0 * (");
            this.TranslateExpression(sb, floatNumerator);
            sb.Append(") / (");
            this.TranslateExpression(sb, floatDenominator);
            sb.Append("))");
        }

        public override void TranslateFloatToInt(TranspilerContext sb, Expression floatExpr)
        {
            sb.Append("TranslationHelper_double_to_int(");
            this.TranslateExpression(sb, floatExpr);
            sb.Append(')');
        }

        public override void TranslateFloatToString(TranspilerContext sb, Expression floatExpr)
        {
            sb.Append("TranslationHelper_double_to_string(");
            this.TranslateExpression(sb, floatExpr);
            sb.Append(')');
        }

        public override void TranslateGetFunction(TranspilerContext sb, Expression name)
        {
            throw new NotImplementedException();
        }

        public override void TranslateGetProgramData(TranspilerContext sb)
        {
            sb.Append("PROGRAM_DATA");
        }

        public override void TranslateGetResourceManifest(TranspilerContext sb)
        {
            sb.Append("TranslationHelper_get_resource_manifest()");
        }

        public override void TranslateGlobalVariable(TranspilerContext sb, Variable variable)
        {
            this.TranslateVariable(sb, variable);
        }

        public override void TranslateIntBuffer16(TranspilerContext sb)
        {
            sb.Append("INT_BUFFER_16");
        }

        public override void TranslateIntegerDivision(TranspilerContext sb, Expression integerNumerator, Expression integerDenominator)
        {
            sb.Append('(');
            this.TranslateExpression(sb, integerNumerator);
            sb.Append(") / (");
            this.TranslateExpression(sb, integerDenominator);
            sb.Append(')');
        }

        public override void TranslateIntToString(TranspilerContext sb, Expression integer)
        {
            sb.Append("TranslationHelper_int_to_string(");
            this.TranslateExpression(sb, integer);
            sb.Append(')');
        }

        public override void TranslateInvokeDynamicLibraryFunction(TranspilerContext sb, Expression functionId, Expression argsArray)
        {
            sb.Append("LibraryHelper_invoke_function(");
            this.TranslateExpression(sb, functionId);
            sb.Append(", ");
            this.TranslateExpression(sb, argsArray);
            sb.Append(')');
        }

        public override void TranslateIsValidInteger(TranspilerContext sb, Expression stringValue)
        {
            sb.Append("TranslationHelper_is_str_a_valid_int(");
            this.TranslateExpression(sb, stringValue);
            sb.Append(')');
        }

        public override void TranslateListAdd(TranspilerContext sb, Expression list, Expression item)
        {
            sb.Append("List_add_");
            sb.Append(this.GetDictionaryValueType(list.ResolvedType.Generics[0]));
            sb.Append('(');
            this.TranslateExpression(sb, list);
            sb.Append(", ");
            this.TranslateExpression(sb, item);
            sb.Append(')');
        }

        public override void TranslateListClear(TranspilerContext sb, Expression list)
        {
            sb.Append("List_clear(");
            this.TranslateExpression(sb, list);
            sb.Append(')');
        }

        public override void TranslateListConcat(TranspilerContext sb, Expression list, Expression items)
        {
            sb.Append("List_concat(");
            this.TranslateExpression(sb, list);
            sb.Append(", ");
            this.TranslateExpression(sb, items);
            sb.Append(')');
        }

        public override void TranslateListGet(TranspilerContext sb, Expression list, Expression index)
        {
            this.TranslateExpression(sb, list);
            sb.Append("->");
            sb.Append(this.GetDictionaryValueType(list.ResolvedType.Generics[0]));
            sb.Append("_items[");
            this.TranslateExpression(sb, index);
            sb.Append(']');
        }

        public override void TranslateListInsert(TranspilerContext sb, Expression list, Expression index, Expression item)
        {
            sb.Append("List_insert(");
            this.TranslateExpression(sb, list);
            sb.Append(", ");
            this.TranslateExpression(sb, index);
            sb.Append(", ");
            this.TranslateExpression(sb, item);
            sb.Append(')');
        }

        public override void TranslateListJoinChars(TranspilerContext sb, Expression list)
        {
            sb.Append("TranslationHelper_list_join_chars(");
            this.TranslateExpression(sb, list);
            sb.Append(')');
        }

        public override void TranslateListJoinStrings(TranspilerContext sb, Expression list, Expression sep)
        {
            sb.Append("TranslationHelper_list_join_strings(");
            this.TranslateExpression(sb, list);
            sb.Append(", ");
            this.TranslateExpression(sb, sep);
            sb.Append(')');
        }

        public override void TranslateListNew(TranspilerContext sb, PType type)
        {
            sb.Append("List_new(sizeof(");
            sb.Append(this.TranslateType(type));
            sb.Append("))");
        }

        public override void TranslateListPop(TranspilerContext sb, Expression list)
        {
            sb.Append("List_pop(");
            this.TranslateExpression(sb, list);
            sb.Append(')');
        }

        public override void TranslateListRemoveAt(TranspilerContext sb, Expression list, Expression index)
        {
            sb.Append("List_remove(");
            this.TranslateExpression(sb, list);
            sb.Append(", ");
            this.TranslateExpression(sb, index);
            sb.Append(')');
        }

        public override void TranslateListReverse(TranspilerContext sb, Expression list)
        {
            sb.Append("List_reverse(");
            this.TranslateExpression(sb, list);
            sb.Append(')');
        }

        public override void TranslateListSet(TranspilerContext sb, Expression list, Expression index, Expression value)
        {
            this.TranslateExpression(sb, list);
            sb.Append("->");
            sb.Append(this.GetDictionaryValueType(list.ResolvedType.Generics[0]));
            sb.Append("_items[");
            this.TranslateExpression(sb, index);
            sb.Append("] = ");
            this.TranslateExpression(sb, value);
        }

        public override void TranslateListShuffle(TranspilerContext sb, Expression list)
        {
            sb.Append("TranslationHelper_list_shuffle(");
            this.TranslateExpression(sb, list);
            sb.Append(')');
        }

        public override void TranslateListSize(TranspilerContext sb, Expression list)
        {
            this.TranslateExpression(sb, list);
            sb.Append("->size");
        }

        public override void TranslateListToArray(TranspilerContext sb, Expression list)
        {
            sb.Append("List_to_array(");
            this.TranslateExpression(sb, list);
            sb.Append(')');
        }

        public override void TranslateMathArcCos(TranspilerContext sb, Expression ratio)
        {
            sb.Append("TranslationHelper_math_arc_cos(");
            this.TranslateExpression(sb, ratio);
            sb.Append(')');
        }

        public override void TranslateMathArcSin(TranspilerContext sb, Expression ratio)
        {
            sb.Append("TranslationHelper_math_arc_sin(");
            this.TranslateExpression(sb, ratio);
            sb.Append(')');
        }

        public override void TranslateMathArcTan(TranspilerContext sb, Expression yComponent, Expression xComponent)
        {
            sb.Append("TranslationHelper_math_arc_tan(");
            this.TranslateExpression(sb, yComponent);
            sb.Append(", ");
            this.TranslateExpression(sb, xComponent);
            sb.Append(')');
        }

        public override void TranslateMathCos(TranspilerContext sb, Expression thetaRadians)
        {
            sb.Append("TranslationHelper_math_cos(");
            this.TranslateExpression(sb, thetaRadians);
            sb.Append(')');
        }

        public override void TranslateMathLog(TranspilerContext sb, Expression value)
        {
            sb.Append("TranslationHelper_math_log(");
            this.TranslateExpression(sb, value);
            sb.Append(')');
        }

        public override void TranslateMathPow(TranspilerContext sb, Expression expBase, Expression exponent)
        {
            sb.Append("TranslationHelper_math_pow(");
            this.TranslateExpression(sb, expBase);
            sb.Append(", ");
            this.TranslateExpression(sb, exponent);
            sb.Append(')');
        }

        public override void TranslateMathSin(TranspilerContext sb, Expression thetaRadians)
        {
            sb.Append("TranslationHelper_math_sin(");
            this.TranslateExpression(sb, thetaRadians);
            sb.Append(')');
        }

        public override void TranslateMathTan(TranspilerContext sb, Expression thetaRadians)
        {
            sb.Append("TranslationHelper_math_tan(");
            this.TranslateExpression(sb, thetaRadians);
            sb.Append(')');
        }

        public override void TranslateMultiplyList(TranspilerContext sb, Expression list, Expression n)
        {
            sb.Append("multiply_list(");
            this.TranslateExpression(sb, list);
            sb.Append(", ");
            this.TranslateExpression(sb, n);
            sb.Append(')');
        }

        public override void TranslateNullConstant(TranspilerContext sb)
        {
            sb.Append("NULL");
        }

        public override void TranslateOpChain(TranspilerContext sb, OpChain opChain)
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

        public override void TranslateOrd(TranspilerContext sb, Expression charValue)
        {
            throw new Exception();
        }

        public override void TranslateParseFloatUnsafe(TranspilerContext sb, Expression stringValue)
        {
            sb.Append("TranslationHelper_parse_float_with_trusted_input(");
            this.TranslateExpression(sb, stringValue);
            sb.Append(')');
        }

        public override void TranslateParseInt(TranspilerContext sb, Expression safeStringValue)
        {
            sb.Append("parse_int(");
            this.TranslateExpression(sb, safeStringValue);
            sb.Append(')');
        }

        public override void TranslatePrintStdErr(TranspilerContext sb, Expression value)
        {
            sb.Append("print_with_conversion(");
            this.TranslateExpression(sb, value);
            sb.Append(')');
        }

        public override void TranslatePrintStdOut(TranspilerContext sb, Expression value)
        {
            sb.Append("print_with_conversion(");
            this.TranslateExpression(sb, value);
            sb.Append(')');
        }

        public override void TranslateRandomFloat(TranspilerContext sb)
        {
            sb.Append("TranslationHeper_random_float()");
        }

        public override void TranslateReadByteCodeFile(TranspilerContext sb)
        {
            sb.Append("BYTE_CODE_FILE");
        }

        public override void TranslateRegisterLibraryFunction(
            TranspilerContext sb,
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

        public override void TranslateResourceReadTextFile(TranspilerContext sb, Expression path)
        {
            sb.Append("ResourceHelper_read_text_file(");
            this.TranslateExpression(sb, path);
            sb.Append(')');
        }

        public override void TranslateSetProgramData(TranspilerContext sb, Expression programData)
        {
            sb.Append("PROGRAM_DATA = ");
            this.TranslateExpression(sb, programData);
        }

        public override void TranslateSortedCopyOfIntArray(TranspilerContext sb, Expression intArray)
        {
            sb.Append("TranslationHelper_array_sorted_copy_int(");
            this.TranslateExpression(sb, intArray);
            sb.Append(')');
        }

        public override void TranslateSortedCopyOfStringArray(TranspilerContext sb, Expression stringArray)
        {
            sb.Append("TranslationHelper_array_sorted_copy_int(");
            this.TranslateExpression(sb, stringArray);
            sb.Append(')');
        }

        public override void TranslateStringAppend(TranspilerContext sb, Expression str1, Expression str2)
        {
            // "stringRef += newSuffix;" is not supported.
            // throw new Exception();
            sb.Append("TODO(\"Replace these calls with basic string concats\")");
        }

        public override void TranslateStringBuffer16(TranspilerContext sb)
        {
            sb.Append("STRING_BUFFER_16");
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
            this.TranslateStringCharAt(sb, str, index);
        }

        public override void TranslateStringCompareIsReverse(TranspilerContext sb, Expression str1, Expression str2)
        {
            sb.Append("String_compare_is_reverse(");
            this.TranslateExpression(sb, str1);
            sb.Append(", ");
            this.TranslateExpression(sb, str2);
            sb.Append(')');
        }

        public override void TranslateStringConcatAll(TranspilerContext sb, Expression[] strings)
        {
            this.TranslateStringConcatAllImpl(sb, strings, 0);
        }

        private void TranslateStringConcatAllImpl(TranspilerContext sb, Expression[] strings, int startIndex)
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

        public override void TranslateStringConcatPair(TranspilerContext sb, Expression strLeft, Expression strRight)
        {
            sb.Append("String_concat(");
            this.TranslateExpression(sb, strLeft);
            sb.Append(", ");
            this.TranslateExpression(sb, strRight);
            sb.Append(')');
        }

        public override void TranslateStringConstant(TranspilerContext sb, string value)
        {
            sb.Append(sb.StringTableBuilder.GetId(value));
            sb.Append("/* ");
            sb.Append(Common.Util.ConvertStringValueToCode(value).Replace("*/", "* /"));
            sb.Append(" */");
        }

        public override void TranslateStringContains(TranspilerContext sb, Expression haystack, Expression needle)
        {
            sb.Append("String_contains(");
            this.TranslateExpression(sb, haystack);
            sb.Append(", ");
            this.TranslateExpression(sb, needle);
            sb.Append(')');
        }

        public override void TranslateStringEndsWith(TranspilerContext sb, Expression haystack, Expression needle)
        {
            sb.Append("String_ends_with(");
            this.TranslateExpression(sb, haystack);
            sb.Append(", ");
            this.TranslateExpression(sb, needle);
            sb.Append(')');
        }

        public override void TranslateStringEquals(TranspilerContext sb, Expression left, Expression right)
        {
            sb.Append("String_equals(");
            this.TranslateExpression(sb, left);
            sb.Append(", ");
            this.TranslateExpression(sb, right);
            sb.Append(')');
        }

        public override void TranslateStringFromCharCode(TranspilerContext sb, Expression charCode)
        {
            sb.Append("TranslationHelper_string_form_char_code(");
            this.TranslateExpression(sb, charCode);
            sb.Append(')');
        }

        public override void TranslateStringIndexOf(TranspilerContext sb, Expression haystack, Expression needle)
        {
            sb.Append("String_index_of(");
            this.TranslateExpression(sb, haystack);
            sb.Append(", ");
            this.TranslateExpression(sb, needle);
            sb.Append(')');
        }

        public override void TranslateStringIndexOfWithStart(TranspilerContext sb, Expression haystack, Expression needle, Expression startIndex)
        {
            sb.Append("String_index_of_with_start_index(");
            this.TranslateExpression(sb, haystack);
            sb.Append(", ");
            this.TranslateExpression(sb, needle);
            sb.Append(", ");
            this.TranslateExpression(sb, startIndex);
            sb.Append(')');
        }

        public override void TranslateStringLength(TranspilerContext sb, Expression str)
        {
            this.TranslateExpression(sb, str);
            sb.Append("[-1]");
        }

        public override void TranslateStringReplace(TranspilerContext sb, Expression haystack, Expression needle, Expression newNeedle)
        {
            sb.Append("String_replace(");
            this.TranslateExpression(sb, haystack);
            sb.Append(", ");
            this.TranslateExpression(sb, needle);
            sb.Append(", ");
            this.TranslateExpression(sb, newNeedle);
            sb.Append(')');
        }

        public override void TranslateStringReverse(TranspilerContext sb, Expression str)
        {
            sb.Append("String_reverse(");
            this.TranslateExpression(sb, str);
            sb.Append(')');
        }

        public override void TranslateStringSplit(TranspilerContext sb, Expression haystack, Expression needle)
        {
            sb.Append("String_split(");
            this.TranslateExpression(sb, haystack);
            sb.Append(", ");
            this.TranslateExpression(sb, needle);
            sb.Append(')');
        }

        public override void TranslateStringStartsWith(TranspilerContext sb, Expression haystack, Expression needle)
        {
            sb.Append("String_starts_with(");
            this.TranslateExpression(sb, haystack);
            sb.Append(", ");
            this.TranslateExpression(sb, needle);
            sb.Append(')');
        }

        public override void TranslateStringSubstring(TranspilerContext sb, Expression str, Expression start, Expression length)
        {
            sb.Append("String_substring(");
            this.TranslateExpression(sb, str);
            sb.Append(", ");
            this.TranslateExpression(sb, start);
            sb.Append(", ");
            this.TranslateExpression(sb, length);
            sb.Append(')');
        }

        public override void TranslateStringSubstringIsEqualTo(TranspilerContext sb, Expression haystack, Expression startIndex, Expression needle)
        {
            sb.Append("String_substring_is_equal_to(");
            this.TranslateExpression(sb, haystack);
            sb.Append(", ");
            this.TranslateExpression(sb, startIndex);
            sb.Append(", ");
            this.TranslateExpression(sb, needle);
            sb.Append(')');
        }

        public override void TranslateStringToLower(TranspilerContext sb, Expression str)
        {
            sb.Append("String_to_lower(");
            this.TranslateExpression(sb, str);
            sb.Append(')');
        }

        public override void TranslateStringToUpper(TranspilerContext sb, Expression str)
        {
            sb.Append("String_to_upper(");
            this.TranslateExpression(sb, str);
            sb.Append(')');
        }

        public override void TranslateStringTrim(TranspilerContext sb, Expression str)
        {
            sb.Append("String_trim(");
            this.TranslateExpression(sb, str);
            sb.Append(", 1, 1)");
        }

        public override void TranslateStringTrimEnd(TranspilerContext sb, Expression str)
        {
            sb.Append("String_trim(");
            this.TranslateExpression(sb, str);
            sb.Append(", 0, 1)");
        }

        public override void TranslateStringTrimStart(TranspilerContext sb, Expression str)
        {
            sb.Append("String_trim(");
            this.TranslateExpression(sb, str);
            sb.Append(", 1, 0)");
        }

        public override void TranslateStrongReferenceEquality(TranspilerContext sb, Expression left, Expression right)
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

                // Protection against re-wrapping
                case "Value":
                    this.TranslateExpression(sb, left);
                    sb.Append("->internalValue == ");
                    this.TranslateExpression(sb, right);
                    sb.Append("->internalValue");
                    break;

                default:
                    // This shouldn't be necessary
                    // Strings should have used string equality comparisons
                    throw new Exception();
            }
        }

        public override void TranslateStructFieldDereference(TranspilerContext sb, Expression root, StructDefinition structDef, string fieldName, int fieldIndex)
        {
            this.TranslateExpression(sb, root);
            sb.Append("->");
            if (fieldName == "internalValue" && root.ResolvedType.RootValue == "Value")
            {
                throw new Pastel.ParserException(root.FirstToken, "Cannot access Value.internalValue without a cast.");
            }
            sb.Append(fieldName);
        }

        public override void TranslateThreadSleep(TranspilerContext sb, Expression seconds)
        {
            sb.Append("TranslationHelper_thread_sleep(");
            this.TranslateExpression(sb, seconds);
            sb.Append(')');
        }

        public override void TranslateTryParseFloat(TranspilerContext sb, Expression stringValue, Expression floatOutList)
        {
            sb.Append("TranslationHelper_try_parse_float(");
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
            sb.Append("LibraryHelper_determine_library_availability(");
            this.TranslateExpression(sb, libraryName);
            sb.Append(", ");
            this.TranslateExpression(sb, libraryVersion);
            sb.Append(')');
        }

        public override void TranslateVmEnqueueResume(TranspilerContext sb, Expression seconds, Expression executionContextId)
        {
            throw new Exception();
        }

        public override void TranslateVmEndProcess(TranspilerContext sb)
        {
            throw new Exception();
        }

        public override void TranslateVmShowLibStack(TranspilerContext sb)
        {
            throw new Exception();
        }

        public override void GenerateCodeForStruct(TranspilerContext sb, StructDefinition structDef)
        {
            sb.Append("struct ");
            sb.Append(structDef.NameToken.Value);
            sb.Append(" {\n");
            for (int i = 0; i < structDef.ArgNames.Length; ++i)
            {
                string fieldName = structDef.ArgNames[i].Value;
                PType fieldType = structDef.ArgTypes[i];
                sb.Append('\t');
                sb.Append(this.TranslateType(fieldType));
                sb.Append(' ');
                sb.Append(fieldName);
                sb.Append(";\n");
            }

            sb.Append("};\n\n");
        }

        public override void GenerateCodeForFunction(TranspilerContext sb, FunctionDefinition funcDef)
        {
            sb.Append(this.TranslateType(funcDef.ReturnType));
            sb.Append(" v_");
            sb.Append(funcDef.NameToken.Value);
            sb.Append('(');
            for (int i = 0; i < funcDef.ArgNames.Length; ++i)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(this.TranslateType(funcDef.ArgTypes[i]));
                sb.Append(" v_");
                sb.Append(funcDef.ArgNames[i].Value);
            }
            sb.Append(")\n{\n");
            sb.TabDepth = 1;
            this.TranslateExecutables(sb, funcDef.Code);
            sb.TabDepth = 0;
            sb.Append("}\n");
        }

        public override void GenerateCodeForFunctionDeclaration(TranspilerContext sb, FunctionDefinition funcDef)
        {
            sb.Append(this.TranslateType(funcDef.ReturnType));
            sb.Append(" v_");
            sb.Append(funcDef.NameToken.Value);
            sb.Append('(');
            for (int i = 0; i < funcDef.ArgNames.Length; ++i)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(this.TranslateType(funcDef.ArgTypes[i]));
                sb.Append(" v_");
                sb.Append(funcDef.ArgNames[i].Value);
            }
            sb.Append(");");
        }

        public override void GenerateCodeForGlobalsDefinitions(TranspilerContext sb, IList<VariableDeclaration> globals)
        {
            throw new NotImplementedException();
        }

        public override void GenerateCodeForStringTable(TranspilerContext sb, StringTableBuilder stringTable)
        {
            List<string> names = stringTable.Names;
            List<string> values = stringTable.Values;
            int total = names.Count;
            for (int i = 0; i < total; ++i)
            {
                sb.Append("int* ");
                sb.Append(names[i]);
                sb.Append(';');
                sb.Append(this.NewLine);
            }
            sb.Append("void populate_string_table_for_");
            sb.Append(stringTable.Prefix);
            sb.Append("()");
            sb.Append(this.NewLine);
            sb.Append('{');
            sb.Append(this.NewLine);
            for (int i = 0; i < total; ++i)
            {
                sb.Append('\t');
                sb.Append(names[i]);
                sb.Append(" = String_from_utf8(");
                sb.Append(Common.Util.ConvertStringValueToCode(values[i]).Replace("%", "%%"));
                sb.Append(");");
                sb.Append(this.NewLine);
            }
            sb.Append('}');
            sb.Append(this.NewLine);
            sb.Append(this.NewLine);
        }

        public override void GenerateCodeForStructDeclaration(TranspilerContext sb, string structName)
        {
            sb.Append("typedef struct ");
            sb.Append(structName);
            sb.Append(' ');
            sb.Append(structName);
            sb.Append(';');
            sb.Append(this.NewLine);
        }
    }
}
