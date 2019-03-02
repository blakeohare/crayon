using System;
using System.Collections.Generic;

namespace Pastel.Nodes
{
    internal class DotField : Expression
    {
        public Expression Root { get; set; }
        public Token DotToken { get; set; }
        public Token FieldName { get; set; }

        public CoreFunction CoreFunctionId { get; set; }
        public StructDefinition StructType { get; set; }

        public DotField(Expression root, Token dotToken, Token fieldName) : base(root.FirstToken, root.Owner)
        {
            this.Root = root;
            this.DotToken = dotToken;
            this.FieldName = fieldName;
            this.CoreFunctionId = CoreFunction.NONE;
        }

        public override Expression ResolveNamesAndCullUnusedCode(PastelCompiler compiler)
        {
            this.Root = this.Root.ResolveNamesAndCullUnusedCode(compiler);

            if (this.Root is EnumReference)
            {
                InlineConstant enumValue = ((EnumReference)this.Root).EnumDef.GetValue(this.FieldName);
                return enumValue.CloneWithNewToken(this.FirstToken);
            }

            if (this.Root is DependencyNamespaceReference)
            {
                PastelCompiler dependencyScope = ((DependencyNamespaceReference)this.Root).Scope;
                string field = this.FieldName.Value;
                FunctionDefinition funcDef = dependencyScope.GetFunctionDefinition(field);
                if (funcDef != null)
                {
                    return new FunctionReference(this.FirstToken, funcDef, this.Owner);
                }

                EnumDefinition enumDef = dependencyScope.GetEnumDefinition(field);
                if (enumDef != null)
                {
                    return new EnumReference(this.FirstToken, enumDef, this.Owner);
                }

                InlineConstant constValue = dependencyScope.GetConstantDefinition(field);
                if (constValue != null)
                {
                    return constValue.CloneWithNewTokenAndOwner(this.FirstToken, this.Owner);
                }

                throw new ParserException(this.FieldName, "The namespace '" + ((DependencyNamespaceReference)this.Root).FirstToken.Value + "' does not have a member called '" + field + "'");
            }

            if (this.Root is CoreNamespaceReference)
            {
                CoreFunction coreFunction = this.GetCoreFunction(this.FieldName.Value);
                switch (coreFunction)
                {
                    case CoreFunction.FLOAT_BUFFER_16:
                    case CoreFunction.INT_BUFFER_16:
                    case CoreFunction.STRING_BUFFER_16:
                        return new CoreFunctionInvocation(this.FirstToken, coreFunction, new Expression[0], this.Owner);

                    default:
                        return new CoreFunctionReference(this.FirstToken, coreFunction, this.Owner);
                }
            }

            if (this.Root is ExtensibleNamespaceReference)
            {
                string name = this.FieldName.Value;
                return new ExtensibleFunctionReference(this.FirstToken, name, this.Owner);
            }

            if (this.Root is EnumReference)
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

            PType rootType = this.Root.ResolvedType;
            if (rootType.IsStruct)
            {
                rootType.FinalizeType(compiler);
                this.StructType = rootType.StructDef;
                int fieldIndex;
                if (!this.StructType.ArgIndexByName.TryGetValue(this.FieldName.Value, out fieldIndex))
                {
                    throw new ParserException(this.FieldName, "The struct '" + this.StructType.NameToken.Value + "' does not have a field called '" + this.FieldName.Value + "'");
                }
                this.ResolvedType = this.StructType.ArgTypes[fieldIndex];
                return this;
            }

            this.CoreFunctionId = this.DetermineCoreFunctionId(this.Root.ResolvedType, this.FieldName.Value);
            if (this.CoreFunctionId != CoreFunction.NONE)
            {
                CoreFunctionReference cfr = new CoreFunctionReference(this.FirstToken, this.CoreFunctionId, this.Root, this.Owner);
                cfr.ResolvedType = new PType(this.Root.FirstToken, null, "@CoreFunc");
                return cfr;
            }

            throw new NotImplementedException();
        }

        private CoreFunction GetCoreFunction(string field)
        {
            switch (field)
            {
                case "ArcCos": return CoreFunction.MATH_ARCCOS;
                case "ArcSin": return CoreFunction.MATH_ARCSIN;
                case "ArcTan": return CoreFunction.MATH_ARCTAN;
                case "Base64ToString": return CoreFunction.BASE64_TO_STRING;
                case "CharToString": return CoreFunction.CHAR_TO_STRING;
                case "Chr": return CoreFunction.CHR;
                case "Cos": return CoreFunction.MATH_COS;
                case "CurrentTimeSeconds": return CoreFunction.CURRENT_TIME_SECONDS;
                case "EmitComment": return CoreFunction.EMIT_COMMENT;
                case "FloatBuffer16": return CoreFunction.FLOAT_BUFFER_16;
                case "FloatDivision": return CoreFunction.FLOAT_DIVISION;
                case "FloatToString": return CoreFunction.FLOAT_TO_STRING;
                case "ForceParens": return CoreFunction.FORCE_PARENS;
                case "GetFunction": return CoreFunction.GET_FUNCTION;
                case "Int": return CoreFunction.INT;
                case "IntBuffer16": return CoreFunction.INT_BUFFER_16;
                case "IntegerDivision": return CoreFunction.INTEGER_DIVISION;
                case "IntToString": return CoreFunction.INT_TO_STRING;
                case "IsValidInteger": return CoreFunction.IS_VALID_INTEGER;
                case "ListConcat": return CoreFunction.LIST_CONCAT;
                case "ListToArray": return CoreFunction.LIST_TO_ARRAY;
                case "Log": return CoreFunction.MATH_LOG;
                case "MultiplyList": return CoreFunction.MULTIPLY_LIST;
                case "Ord": return CoreFunction.ORD;
                case "ParseFloatUnsafe": return CoreFunction.PARSE_FLOAT_UNSAFE;
                case "ParseInt": return CoreFunction.PARSE_INT;
                case "Pow": return CoreFunction.MATH_POW;
                case "PrintStdErr": return CoreFunction.PRINT_STDERR;
                case "PrintStdOut": return CoreFunction.PRINT_STDOUT;
                case "RandomFloat": return CoreFunction.RANDOM_FLOAT;
                case "Sin": return CoreFunction.MATH_SIN;
                case "StringAppend": return CoreFunction.STRING_APPEND;
                case "StringBuffer16": return CoreFunction.STRING_BUFFER_16;
                case "StringCompareIsReverse": return CoreFunction.STRING_COMPARE_IS_REVERSE;
                case "StringConcatAll": return CoreFunction.STRING_CONCAT_ALL;
                case "StringEquals": return CoreFunction.STRING_EQUALS;
                case "StringFromCharCode": return CoreFunction.STRING_FROM_CHAR_CODE;
                case "StrongReferenceEquality": return CoreFunction.STRONG_REFERENCE_EQUALITY;
                case "Tan": return CoreFunction.MATH_TAN;
                case "TryParseFloat": return CoreFunction.TRY_PARSE_FLOAT;

                // TODO: get this information from the parameter rather than having separate Core function
                case "SortedCopyOfStringArray": return CoreFunction.SORTED_COPY_OF_STRING_ARRAY;
                case "SortedCopyOfIntArray": return CoreFunction.SORTED_COPY_OF_INT_ARRAY;

                default:
                    throw new ParserException(this.FirstToken, "Invalid Core function: 'Core." + field + "'.");
            }
        }

        private CoreFunction DetermineCoreFunctionId(PType rootType, string field)
        {
            switch (rootType.RootValue)
            {
                case "string":
                    switch (field)
                    {
                        case "CharCodeAt": return CoreFunction.STRING_CHAR_CODE_AT;
                        case "Contains": return CoreFunction.STRING_CONTAINS;
                        case "EndsWith": return CoreFunction.STRING_ENDS_WITH;
                        case "IndexOf": return CoreFunction.STRING_INDEX_OF;
                        case "Replace": return CoreFunction.STRING_REPLACE;
                        case "Reverse": return CoreFunction.STRING_REVERSE;
                        case "Size": return CoreFunction.STRING_LENGTH;
                        case "Split": return CoreFunction.STRING_SPLIT;
                        case "StartsWith": return CoreFunction.STRING_STARTS_WITH;
                        case "SubString": return CoreFunction.STRING_SUBSTRING;
                        case "SubStringIsEqualTo": return CoreFunction.STRING_SUBSTRING_IS_EQUAL_TO;
                        case "ToLower": return CoreFunction.STRING_TO_LOWER;
                        case "ToUpper": return CoreFunction.STRING_TO_UPPER;
                        case "Trim": return CoreFunction.STRING_TRIM;
                        case "TrimEnd": return CoreFunction.STRING_TRIM_END;
                        case "TrimStart": return CoreFunction.STRING_TRIM_START;
                        default: throw new ParserException(this.FieldName, "Unresolved string method: " + field);
                    }

                case "Array":
                    switch (field)
                    {
                        case "Join": return CoreFunction.ARRAY_JOIN;
                        case "Length": return CoreFunction.ARRAY_LENGTH;
                        // TODO: deprecate this
                        case "Size": return CoreFunction.ARRAY_LENGTH;
                        default: throw new ParserException(this.FieldName, "Unresolved Array method: " + field);
                    }

                case "List":
                    switch (field)
                    {
                        case "Add": return CoreFunction.LIST_ADD;
                        case "Clear": return CoreFunction.LIST_CLEAR;
                        case "Insert": return CoreFunction.LIST_INSERT;
                        case "Join":
                            string memberType = rootType.Generics[0].RootValue;
                            switch (memberType)
                            {
                                case "string": return CoreFunction.LIST_JOIN_STRINGS;
                                case "char": return CoreFunction.LIST_JOIN_CHARS;
                                default: throw new ParserException(this.FieldName, "Unresolved List<" + memberType + "> method: " + field);
                            }

                        case "Pop": return CoreFunction.LIST_POP;
                        case "RemoveAt": return CoreFunction.LIST_REMOVE_AT;
                        case "Reverse": return CoreFunction.LIST_REVERSE;
                        case "Shuffle": return CoreFunction.LIST_SHUFFLE;
                        case "Size": return CoreFunction.LIST_SIZE;
                        default: throw new ParserException(this.FieldName, "Unresolved List method: " + field);
                    }

                case "Dictionary":
                    switch (field)
                    {
                        case "Contains": return CoreFunction.DICTIONARY_CONTAINS_KEY;
                        case "Keys": return CoreFunction.DICTIONARY_KEYS;
                        case "Remove": return CoreFunction.DICTIONARY_REMOVE;
                        case "Size": return CoreFunction.DICTIONARY_SIZE;
                        case "TryGet": return CoreFunction.DICTIONARY_TRY_GET;
                        case "Values": return CoreFunction.DICTIONARY_VALUES;
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
