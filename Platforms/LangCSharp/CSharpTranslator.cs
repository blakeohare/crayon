using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pastel.Nodes;

namespace LangCSharp
{
    public abstract class CSharpTranslator : Platform.CurlyBraceTranslator
    {
        public CSharpTranslator(Platform.AbstractPlatform platform) : base(platform, "    ", "\r\n", false)
        { }

        public override void TranslateArrayGet(StringBuilder sb, Expression array, Expression index)
        {
            this.TranslateExpression(sb, array);
            sb.Append('[');
            this.TranslateExpression(sb, index);
            sb.Append(']');
        }

        public override void TranslateArrayLength(StringBuilder sb, Expression array)
        {
            this.TranslateExpression(sb, array);
            sb.Append(".Length");
        }

        public override void TranslateArraySet(StringBuilder sb, Expression array, Expression index, Expression value)
        {
            this.TranslateExpression(sb, array);
            sb.Append('[');
            this.TranslateExpression(sb, index);
            sb.Append("] = ");
            this.TranslateExpression(sb, value);
        }

        public override void TranslateConstructorInvocation(StringBuilder sb, ConstructorInvocation constructorInvocation)
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

        public override void TranslateDictionaryContainsKey(StringBuilder sb, Expression dictionary, Expression key)
        {
            this.TranslateExpression(sb, dictionary);
            sb.Append(".ContainsKey(");
            this.TranslateExpression(sb, key);
            sb.Append(")");
        }

        public override void TranslateDictionaryGet(StringBuilder sb, Expression dictionary, Expression key)
        {
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
            this.TranslateExpression(sb, dictionary);
            sb.Append(".Count");
        }

        public override void TranslateNullConstant(StringBuilder sb)
        {
            sb.Append("null");
        }

        public override void TranslateStringLength(StringBuilder sb, Expression str)
        {
            this.TranslateExpression(sb, str);
            sb.Append(".Length");
        }

        public override void TranslateStructFieldDereferenc(StringBuilder sb, Expression root, StructDefinition structDef, string fieldName, int fieldIndex)
        {
            this.TranslateExpression(sb, root);
            sb.Append('.');
            sb.Append(fieldName);
        }

        public override void TranslateVariableDeclaration(StringBuilder sb, VariableDeclaration varDecl)
        {
            sb.Append(this.Platform.TranslateType(varDecl.Type));
            sb.Append(" v_");
            sb.Append(varDecl.VariableName);
            if (varDecl.Value != null)
            {
                sb.Append(" = ");
                this.TranslateExpression(sb, varDecl.Value);
            }
            sb.Append(';');
        }
    }
}
