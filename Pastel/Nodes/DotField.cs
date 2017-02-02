using System;
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
                if (compiler.EnumDefinitions.ContainsKey(rootName))
                {
                    InlineConstant enumValue = compiler.EnumDefinitions[rootName].GetValue(this.FieldName);
                    return enumValue.CloneWithNewToken(this.FirstToken);
                }
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

        internal override void ResolveType(VariableScope varScope, PastelCompiler compiler)
        {
            this.Root.ResolveType(varScope, compiler);

            if (this.FieldName.Value == "Length")
            {
                string rootType = this.Root.ResolvedType.RootValue;
                if (rootType == "Array" || rootType == "List")
                {
                    this.ResolvedType = PType.INT;
                    return;
                }
            }

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
                return;
            }

            this.NativeFunctionId = this.DetermineNativeFunctionId(this.Root.ResolvedType, this.FieldName.Value);
        }

        private NativeFunction DetermineNativeFunctionId(PType rootType, string field)
        {
            switch (rootType.RootValue)
            {
                case "Dictionary":
                    switch (field)
                    {
                        case "Contains": return NativeFunction.DICTIONARY_CONTAINS_KEY;
                        default: throw new ParserException(this.FieldName, "Unresolved field.");
                    }

                default:
                    throw new ParserException(this.FieldName, "Unresolved field.");
            }
        }
    }
}
