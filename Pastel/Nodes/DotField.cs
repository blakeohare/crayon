﻿using System;
using System.Collections.Generic;

namespace Pastel.Nodes
{
    class DotField : Expression
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
                    return new NativeFunctionReference(this.FirstToken, nativeFunction);
                }
                else if (compiler.EnumDefinitions.ContainsKey(rootName))
                {
                    InlineConstant enumValue = compiler.EnumDefinitions[rootName].GetValue(this.FieldName);
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
            this.Root = this.Root.ResolveType(varScope, compiler);

            string possibleStructName = this.Root.ResolvedType.RootValue;
            StructDefinition structDef;
            if (compiler.StructDefinitions.TryGetValue(possibleStructName, out structDef))
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
                case "CharToString": return NativeFunction.CHAR_TO_STRING;
                case "Chr": return NativeFunction.CHR;
                case "CommandLineArgs": return NativeFunction.COMMAND_LINE_ARGS;
                case "ConvertRawDictionaryValueCollectionToAReusableValueList": return NativeFunction.CONVERT_RAW_DICTIONARY_VALUE_COLLECTION_TO_A_REUSABLE_VALUE_LIST;
                case "CurrentTimeSeconds": return NativeFunction.CURRENT_TIME_SECONDS;
                case "EmitComment": return NativeFunction.EMIT_COMMENT;
                case "FloatDivision": return NativeFunction.FLOAT_DIVISION;
                case "FloatToString": return NativeFunction.FLOAT_TO_STRING;
                case "ForceParens": return NativeFunction.FORCE_PARENS;
                case "GenerateException": return NativeFunction.GENERATE_EXCEPTION;
                case "GetProgramData": return NativeFunction.GET_PROGRAM_DATA;
                case "GetResourceManifest": return NativeFunction.GET_RESOURCE_MANIFEST;
                case "Int": return NativeFunction.INT;
                case "IntegerDivision": return NativeFunction.INTEGER_DIVISION;
                case "IntToString": return NativeFunction.INT_TO_STRING;
                case "IsValidInteger": return NativeFunction.IS_VALID_INTEGER;
                case "ListToArray": return NativeFunction.LIST_TO_ARRAY;
                case "MultiplyList": return NativeFunction.MULTIPLY_LIST;
                case "Ord": return NativeFunction.ORD;
                case "ParseFloat": return NativeFunction.PARSE_FLOAT;
                case "ParseFloat_REDUNDANT": return NativeFunction.PARSE_FLOAT_REDUNDANT;
                case "ParseInt": return NativeFunction.PARSE_INT;
                case "PrintStdErr": return NativeFunction.PRINT_STDERR;
                case "PrintStdOut": return NativeFunction.PRINT_STDOUT;
                case "RandomFloat": return NativeFunction.RANDOM_FLOAT;
                case "ReadByteCodeFile": return NativeFunction.READ_BYTE_CODE_FILE;
                case "SetProgramData": return NativeFunction.SET_PROGRAM_DATA;
                case "StringAppend": return NativeFunction.STRING_APPEND;
                case "StringCompareIsReverse": return NativeFunction.STRING_COMPARE_IS_REVERSE;
                case "StringConcatAll": return NativeFunction.STRING_CONCAT_ALL;
                case "StringEquals": return NativeFunction.STRING_EQUALS;
                case "StringFromCharCode": return NativeFunction.STRING_FROM_CHAR_CODE;
                case "StrongReferenceEquality": return NativeFunction.STRONG_REFERENCE_EQUALITY;
                case "ThreadSleep": return NativeFunction.THREAD_SLEEP;

                // TODO: get this information from the parameter rather than having separate Core function
                case "SortedCopyOfStringArray": return NativeFunction.SORTED_COPY_OF_STRING_ARRAY;
                case "SortedCopyOfIntArray": return NativeFunction.SORTED_COPY_OF_INT_ARRAY;

                default:
                    throw new ParserException(this.FirstToken, "Invalid Core function.");
            }
        }

        private NativeFunction DetermineNativeFunctionId(PType rootType, string field)
        {
            switch (rootType.RootValue)
            {
                case "string":
                    switch (field)
                    {
                        case "Size": return NativeFunction.STRING_LENGTH;
                        default: throw new ParserException(this.FieldName, "Unresolved string field: " + field);
                    }

                case "Array":
                    switch (field)
                    {
                        case "Size": return NativeFunction.ARRAY_LENGTH;
                        default: throw new ParserException(this.FieldName, "Unresolved Array field: " + field);
                    }

                case "Dictionary":
                    switch (field)
                    {
                        case "Contains": return NativeFunction.DICTIONARY_CONTAINS_KEY;
                        case "Keys": return NativeFunction.DICTIONARY_KEYS;
                        case "Size": return NativeFunction.DICTIONARY_SIZE;
                        default: throw new ParserException(this.FieldName, "Unresolved Dictionary field: " + field);
                    }

                default:
                    throw new ParserException(this.FieldName, "Unresolved field.");
            }
        }
    }
}
