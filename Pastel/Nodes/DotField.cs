using System;
using System.Collections.Generic;

namespace Pastel.Nodes
{
    public class DotField : Expression
    {
        public Expression Root { get; set; }
        public Token DotToken { get; set; }
        public Token FieldName { get; set; }

        public NativeFunction NativeFunctionId { get; set; }
        public StructDefinition StructType { get; set; }

        public DotField(Expression root, Token dotToken, Token fieldName) : base(root.FirstToken)
        {
            this.Root = root;
            this.DotToken = dotToken;
            this.FieldName = fieldName;
            this.NativeFunctionId = NativeFunction.NONE;
        }

        public override Expression ResolveNamesAndCullUnusedCode(PastelCompiler compiler)
        {
            this.Root = this.Root.ResolveNamesAndCullUnusedCode(compiler);

            Variable varRoot = this.Root as Variable;
            if (varRoot != null)
            {
                string rootName = varRoot.Name;
                if (rootName == "Core")
                {
                    NativeFunction nativeFunction = this.GetNativeCoreFunction(this.FieldName.Value);
                    switch (nativeFunction)
                    {
                        case NativeFunction.FLOAT_BUFFER_16:
                        case NativeFunction.INT_BUFFER_16:
                        case NativeFunction.STRING_BUFFER_16:
                            return new NativeFunctionInvocation(this.FirstToken, nativeFunction, new Expression[0]);

                        default:
                            return new NativeFunctionReference(this.FirstToken, nativeFunction);
                    }
                }
                EnumDefinition enumDef = compiler.GetEnumDefinition(rootName);
                if (enumDef != null)
                {
                    InlineConstant enumValue = enumDef.GetValue(this.FieldName);
                    return enumValue.CloneWithNewToken(this.FirstToken);
                }
            }
            else if (this.Root is EnumReference)
            {
                EnumDefinition enumDef = ((EnumReference)this.Root).EnumDef;
                InlineConstant enumValue = enumDef.GetValue(this.FieldName);
                return enumValue;
            }

            return this;
        }

        internal override InlineConstant DoConstantResolution(HashSet<string> cycleDetection, PastelCompiler compiler)
        {
            Variable varRoot = this.Root as Variable;
            if (varRoot == null) throw new ParserException(this.FirstToken, "Not able to resolve this constant.");
            string enumName = varRoot.Name;
            EnumDefinition enumDef;
            if (!compiler.EnumDefinitions.TryGetValue(enumName, out enumDef))
            {
                throw new ParserException(this.FirstToken, "Not able to resolve this constant.");
            }

            return enumDef.GetValue(this.FieldName);
        }

        internal override Expression ResolveType(VariableScope varScope, PastelCompiler compiler)
        {
            if (this.Root is Variable && ((Variable)this.Root).Name == "Native")
            {
                string name = this.FieldName.Value;
                return new LibraryNativeFunctionReference(this.FirstToken, name);
            }

            this.Root = this.Root.ResolveType(varScope, compiler);

            string possibleStructName = this.Root.ResolvedType.RootValue;
            StructDefinition structDef = compiler.GetStructDefinition(possibleStructName);
            if (structDef != null)
            {
                this.StructType = structDef;
                int fieldIndex;
                if (!structDef.ArgIndexByName.TryGetValue(this.FieldName.Value, out fieldIndex))
                {
                    throw new ParserException(this.FieldName, "The struct '" + structDef.NameToken.Value + "' does not have a field called '" + this.FieldName.Value + "'");
                }
                this.ResolvedType = structDef.ArgTypes[fieldIndex];
                return this;
            }

            this.NativeFunctionId = this.DetermineNativeFunctionId(this.Root.ResolvedType, this.FieldName.Value);
            if (this.NativeFunctionId != NativeFunction.NONE)
            {
                return new NativeFunctionReference(this.FirstToken, this.NativeFunctionId, this.Root);
            }

            throw new NotImplementedException();
        }

        private NativeFunction GetNativeCoreFunction(string field)
        {
            switch (field)
            {
                case "ArcCos": return NativeFunction.MATH_ARCCOS;
                case "ArcSin": return NativeFunction.MATH_ARCSIN;
                case "ArcTan": return NativeFunction.MATH_ARCTAN;
                case "Base64ToString": return NativeFunction.BASE64_TO_STRING;
                case "CharToString": return NativeFunction.CHAR_TO_STRING;
                case "Chr": return NativeFunction.CHR;
                case "CommandLineArgs": return NativeFunction.COMMAND_LINE_ARGS;
                case "ConvertRawDictionaryValueCollectionToAReusableValueList": return NativeFunction.CONVERT_RAW_DICTIONARY_VALUE_COLLECTION_TO_A_REUSABLE_VALUE_LIST;
                case "Cos": return NativeFunction.MATH_COS;
                case "CurrentTimeSeconds": return NativeFunction.CURRENT_TIME_SECONDS;
                case "EmitComment": return NativeFunction.EMIT_COMMENT;
                case "EnqueueVmResume": return NativeFunction.ENQUEUE_VM_RESUME;
                case "FloatBuffer16": return NativeFunction.FLOAT_BUFFER_16;
                case "FloatDivision": return NativeFunction.FLOAT_DIVISION;
                case "FloatToString": return NativeFunction.FLOAT_TO_STRING;
                case "ForceParens": return NativeFunction.FORCE_PARENS;
                case "GetProgramData": return NativeFunction.GET_PROGRAM_DATA;
                case "GetResourceManifest": return NativeFunction.GET_RESOURCE_MANIFEST;
                case "Int": return NativeFunction.INT;
                case "IntBuffer16": return NativeFunction.INT_BUFFER_16;
                case "IntegerDivision": return NativeFunction.INTEGER_DIVISION;
                case "IntToString": return NativeFunction.INT_TO_STRING;
                case "InvokeDynamicLibraryFunction": return NativeFunction.INVOKE_DYNAMIC_LIBRARY_FUNCTION;
                case "IsDebug": return NativeFunction.IS_DEBUG;
                case "IsValidInteger": return NativeFunction.IS_VALID_INTEGER;
                case "ListConcat": return NativeFunction.LIST_CONCAT;
                case "ListToArray": return NativeFunction.LIST_TO_ARRAY;
                case "Log": return NativeFunction.MATH_LOG;
                case "MultiplyList": return NativeFunction.MULTIPLY_LIST;
                case "Ord": return NativeFunction.ORD;
                case "ParseFloatUnsafe": return NativeFunction.PARSE_FLOAT_UNSAFE;
                case "ParseInt": return NativeFunction.PARSE_INT;
                case "Pow": return NativeFunction.MATH_POW;
                case "PrintStdErr": return NativeFunction.PRINT_STDERR;
                case "PrintStdOut": return NativeFunction.PRINT_STDOUT;
                case "RandomFloat": return NativeFunction.RANDOM_FLOAT;
                case "ReadByteCodeFile": return NativeFunction.READ_BYTE_CODE_FILE;
                case "RegisterLibraryFunction": return NativeFunction.REGISTER_LIBRARY_FUNCTION;
                case "ReadResourceTextFile": return NativeFunction.RESOURCE_READ_TEXT_FILE;
                case "SetProgramData": return NativeFunction.SET_PROGRAM_DATA;
                case "Sin": return NativeFunction.MATH_SIN;
                case "StringAppend": return NativeFunction.STRING_APPEND;
                case "StringBuffer16": return NativeFunction.STRING_BUFFER_16;
                case "StringCompareIsReverse": return NativeFunction.STRING_COMPARE_IS_REVERSE;
                case "StringConcatAll": return NativeFunction.STRING_CONCAT_ALL;
                case "StringEquals": return NativeFunction.STRING_EQUALS;
                case "StringFromCharCode": return NativeFunction.STRING_FROM_CHAR_CODE;
                case "StrongReferenceEquality": return NativeFunction.STRONG_REFERENCE_EQUALITY;
                case "Tan": return NativeFunction.MATH_TAN;
                case "ThreadSleep": return NativeFunction.THREAD_SLEEP;
                case "TryParseFloat": return NativeFunction.TRY_PARSE_FLOAT;
                case "VmDetermineLibraryAvailability": return NativeFunction.VM_DETERMINE_LIBRARY_AVAILABILITY;
                case "VmRunLibraryManifest": return NativeFunction.VM_RUN_LIBRARY_MANIFEST;

                // TODO: get this information from the parameter rather than having separate Core function
                case "SortedCopyOfStringArray": return NativeFunction.SORTED_COPY_OF_STRING_ARRAY;
                case "SortedCopyOfIntArray": return NativeFunction.SORTED_COPY_OF_INT_ARRAY;

                default:
                    throw new ParserException(this.FirstToken, "Invalid Core function: 'Core." + field + "'.");
            }
        }

        private NativeFunction DetermineNativeFunctionId(PType rootType, string field)
        {
            switch (rootType.RootValue)
            {
                case "string":
                    switch (field)
                    {
                        case "CharCodeAt": return NativeFunction.STRING_CHAR_CODE_AT;
                        case "Contains": return NativeFunction.STRING_CONTAINS;
                        case "EndsWith": return NativeFunction.STRING_ENDS_WITH;
                        case "IndexOf": return NativeFunction.STRING_INDEX_OF;
                        case "Replace": return NativeFunction.STRING_REPLACE;
                        case "Reverse": return NativeFunction.STRING_REVERSE;
                        case "Size": return NativeFunction.STRING_LENGTH;
                        case "Split": return NativeFunction.STRING_SPLIT;
                        case "StartsWith": return NativeFunction.STRING_STARTS_WITH;
                        case "SubString": return NativeFunction.STRING_SUBSTRING;
                        case "SubStringIsEqualTo": return NativeFunction.STRING_SUBSTRING_IS_EQUAL_TO;
                        case "ToLower": return NativeFunction.STRING_TO_LOWER;
                        case "ToUpper": return NativeFunction.STRING_TO_UPPER;
                        case "Trim": return NativeFunction.STRING_TRIM;
                        case "TrimEnd": return NativeFunction.STRING_TRIM_END;
                        case "TrimStart": return NativeFunction.STRING_TRIM_START;
                        default: throw new ParserException(this.FieldName, "Unresolved string method: " + field);
                    }

                case "Array":
                    switch (field)
                    {
                        case "Join": return NativeFunction.ARRAY_JOIN;
                        case "Size": return NativeFunction.ARRAY_LENGTH;
                        default: throw new ParserException(this.FieldName, "Unresolved Array method: " + field);
                    }

                case "List":
                    switch (field)
                    {
                        case "Add": return NativeFunction.LIST_ADD;
                        case "Clear": return NativeFunction.LIST_CLEAR;
                        case "Insert": return NativeFunction.LIST_INSERT;
                        case "Join":
                            string memberType = rootType.Generics[0].RootValue;
                            switch (memberType)
                            {
                                case "string": return NativeFunction.LIST_JOIN_STRINGS;
                                case "char": return NativeFunction.LIST_JOIN_CHARS;
                                default: throw new ParserException(this.FieldName, "Unresolved List<" + memberType + "> method: " + field);
                            }

                        case "Pop": return NativeFunction.LIST_POP;
                        case "RemoveAt": return NativeFunction.LIST_REMOVE_AT;
                        case "Reverse": return NativeFunction.LIST_REVERSE;
                        case "Shuffle": return NativeFunction.LIST_SHUFFLE;
                        case "Size": return NativeFunction.LIST_SIZE;
                        default: throw new ParserException(this.FieldName, "Unresolved List method: " + field);
                    }

                case "Dictionary":
                    switch (field)
                    {
                        case "Contains": return NativeFunction.DICTIONARY_CONTAINS_KEY;
                        case "Keys": return NativeFunction.DICTIONARY_KEYS;
                        case "Remove": return NativeFunction.DICTIONARY_REMOVE;
                        case "Size": return NativeFunction.DICTIONARY_SIZE;
                        case "Values": return NativeFunction.DICTIONARY_VALUES;
                        default: throw new ParserException(this.FieldName, "Unresolved Dictionary method: " + field);
                    }

                default:
                    throw new ParserException(this.FieldName, "Unresolved field.");
            }
        }

        internal override Expression ResolveWithTypeContext(PastelCompiler compiler)
        {
            this.Root = this.Root.ResolveWithTypeContext(compiler);
            return this;
        }
    }
}
