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

        public override void TranslateCast(StringBuilder sb, PType type, Expression expression)
        {
            sb.Append('(');
            sb.Append(this.Platform.TranslateType(type));
            sb.Append(')');
            this.TranslateExpression(sb, expression);
        }

        public override void TranslateCharToString(StringBuilder sb, Expression charValue)
        {
            throw new NotImplementedException();
        }

        public override void TranslateChr(StringBuilder sb, Expression charCode)
        {
            throw new NotImplementedException();
        }

        public override void TranslateCommandLineArgs(StringBuilder sb)
        {
            throw new NotImplementedException();
        }

        public override void TranslateConvertRawDictionaryValueCollectionToAReusableValueList(StringBuilder sb, Expression dictionary)
        {
            sb.Append("new List<Value>(");
            this.TranslateExpression(sb, dictionary);
            sb.Append(".Values)");
        }

        public override void TranslateCurrentTimeSeconds(StringBuilder sb)
        {
            throw new NotImplementedException();
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

        public override void TranslateDictionaryKeys(StringBuilder sb, Expression dictionary)
        {
            this.TranslateExpression(sb, dictionary);
            sb.Append(".Keys.ToArray()");
        }

        public override void TranslateDictionaryKeysToValueList(StringBuilder sb, Expression dictionary)
        {
            throw new NotImplementedException();
        }

        public override void TranslateDictionaryNew(StringBuilder sb, PType keyType, PType valueType)
        {
            throw new NotImplementedException();
        }

        public override void TranslateDictionaryRemove(StringBuilder sb, Expression dictionary, Expression key)
        {
            this.TranslateExpression(sb, dictionary);
            sb.Append(".Remove(");
            this.TranslateExpression(sb, key);
            sb.Append(')');
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

        public override void TranslateDictionaryValues(StringBuilder sb, Expression dictionary)
        {
            this.TranslateExpression(sb, dictionary);
            sb.Append(".Values.ToArray()");
        }

        public override void TranslateDictionaryValuesToValueList(StringBuilder sb, Expression dictionary)
        {
            throw new NotImplementedException();
        }

        public override void TranslateFloatDivision(StringBuilder sb, Expression floatNumerator, Expression floatDenominator)
        {
            sb.Append("(");
            this.TranslateExpression(sb, floatNumerator);
            sb.Append(") / (");
            this.TranslateExpression(sb, floatDenominator);
            sb.Append(')');
        }

        public override void TranslateFloatToInt(StringBuilder sb, Expression floatExpr)
        {
            sb.Append("(int)");
            this.TranslateExpression(sb, floatExpr);
        }

        public override void TranslateGetProgramData(StringBuilder sb)
        {
            sb.Append("TranslationHelper.GetProgramData()");
        }

        public override void TranslateIntBuffer16(StringBuilder sb)
        {
            sb.Append("TranslationHelper.IntBuffer16");
        }

        public override void TranslateIntegerDivision(StringBuilder sb, Expression integerNumerator, Expression integerDenominator)
        {
            sb.Append("(");
            this.TranslateExpression(sb, integerNumerator);
            sb.Append(") / (");
            this.TranslateExpression(sb, integerDenominator);
            sb.Append(')');
        }

        public override void TranslateListAdd(StringBuilder sb, Expression list, Expression item)
        {
            this.TranslateExpression(sb, list);
            sb.Append(".Add(");
            this.TranslateExpression(sb, item);
            sb.Append(')');
        }

        public override void TranslateListAddAll(StringBuilder sb, Expression list, Expression items)
        {
            bool addParens = list is CastExpression;
            if (addParens) sb.Append('(');
            this.TranslateExpression(sb, list);
            if (addParens) sb.Append(')');
            sb.Append(".AddAll(");
            this.TranslateExpression(sb, items);
            sb.Append(")");
        }

        public override void TranslateListClear(StringBuilder sb, Expression list)
        {
            this.TranslateExpression(sb, list);
            sb.Append(".Clear()");
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
            sb.Append(".Insert(");
            this.TranslateExpression(sb, index);
            sb.Append(", ");
            this.TranslateExpression(sb, item);
            sb.Append(')');
        }

        public override void TranslateListJoinChars(StringBuilder sb, Expression list)
        {
            throw new NotImplementedException();
        }

        public override void TranslateListJoinStrings(StringBuilder sb, Expression list, Expression sep)
        {
            sb.Append("string.Join(");
            this.TranslateExpression(sb, sep);
            sb.Append(", ");
            this.TranslateExpression(sb, list);
            sb.Append(')');
        }

        public override void TranslateListNew(StringBuilder sb, PType type)
        {
            throw new NotImplementedException();
        }

        public override void TranslateListPop(StringBuilder sb, Expression list)
        {
            // No megusta
            this.TranslateExpression(sb, list);
            sb.Append(".RemoveAt(");
            this.TranslateExpression(sb, list);
            sb.Append(".Count - 1)");
        }

        public override void TranslateListRemoveAt(StringBuilder sb, Expression list, Expression index)
        {
            this.TranslateExpression(sb, list);
            sb.Append(".RemoveAt(");
            this.TranslateExpression(sb, index);
            sb.Append(')');
        }

        public override void TranslateListReverse(StringBuilder sb, Expression list)
        {
            this.TranslateExpression(sb, list);
            sb.Append(".Reverse()");
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
            sb.Append("TranslationHelper.ListShuffle(");
            this.TranslateExpression(sb, list);
            sb.Append(')');
        }

        public override void TranslateListSize(StringBuilder sb, Expression list)
        {
            this.TranslateExpression(sb, list);
            sb.Append(".Count");
        }

        public override void TranslateListToArray(StringBuilder sb, Expression list)
        {
            this.TranslateExpression(sb, list);
            sb.Append(".ToArray()");
        }

        public override void TranslateMathPow(StringBuilder sb, Expression expBase, Expression exponent)
        {
            sb.Append("Math.Pow(");
            this.TranslateExpression(sb, expBase);
            sb.Append(", ");
            this.TranslateExpression(sb, exponent);
            sb.Append(")");
        }

        public override void TranslateMultiplyList(StringBuilder sb, Expression list, Expression n)
        {
            sb.Append("TranslationHelper.MultiplyList(");
            this.TranslateExpression(sb, list);
            sb.Append(", ");
            this.TranslateExpression(sb, n);
            sb.Append(")");
        }

        public override void TranslateNullConstant(StringBuilder sb)
        {
            sb.Append("null");
        }

        public override void TranslateParseFloatREDUNDANT(StringBuilder sb, Expression stringValue)
        {
            sb.Append("double.Parse(");
            this.TranslateExpression(sb, stringValue);
            sb.Append(')');
        }

        public override void TranslateRandomFloat(StringBuilder sb)
        {
            sb.Append("TranslationHelper.Random.NextDouble()");
        }

        public override void TranslateStringAppend(StringBuilder sb, Expression str1, Expression str2)
        {
            throw new NotImplementedException();
        }

        public override void TranslateStringBuffer16(StringBuilder sb)
        {
            throw new NotImplementedException();
        }

        public override void TranslateStringCharAt(StringBuilder sb, Expression str, Expression index)
        {
            throw new NotImplementedException();
        }

        public override void TranslateStringCharCodeAt(StringBuilder sb, Expression str, Expression index)
        {
            throw new NotImplementedException();
        }

        public override void TranslateStringConcatAll(StringBuilder sb, Expression[] strings)
        {
            sb.Append("string.Join(\"\", new string[] { ");
            for (int i = 0; i < strings.Length; ++i)
            {
                if (i > 0) sb.Append(',');
                this.TranslateExpression(sb, strings[i]);
            }
            sb.Append(" })");
        }

        public override void TranslateStringContains(StringBuilder sb, Expression haystack, Expression needle)
        {
            this.TranslateExpression(sb, haystack);
            sb.Append(".Contains(");
            this.TranslateExpression(sb, needle);
            sb.Append(')');
        }

        public override void TranslateStringEndsWith(StringBuilder sb, Expression haystack, Expression needle)
        {
            this.TranslateExpression(sb, haystack);
            sb.Append(".EndsWith(");
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
            throw new NotImplementedException();
        }

        public override void TranslateStringIndexOf(StringBuilder sb, Expression haystack, Expression needle)
        {
            this.TranslateExpression(sb, haystack);
            sb.Append(".IndexOf(");
            this.TranslateExpression(sb, needle);
            sb.Append(')');
        }

        public override void TranslateStringLength(StringBuilder sb, Expression str)
        {
            this.TranslateExpression(sb, str);
            sb.Append(".Length");
        }

        public override void TranslateStringReplace(StringBuilder sb, Expression haystack, Expression needle, Expression newNeedle)
        {
            this.TranslateExpression(sb, haystack);
            sb.Append(".Replace(");
            this.TranslateExpression(sb, needle);
            sb.Append(", ");
            this.TranslateExpression(sb, newNeedle);
            sb.Append(")");
        }

        public override void TranslateStringReverse(StringBuilder sb, Expression str)
        {
            sb.Append("TranslationHelper.ReverseString(");
            this.TranslateExpression(sb, str);
            sb.Append(')');
        }

        public override void TranslateStringSplit(StringBuilder sb, Expression haystack, Expression needle)
        {
            sb.Append("TranslationHelper.SplitString(");
            this.TranslateExpression(sb, haystack);
            sb.Append(", ");
            this.TranslateExpression(sb, needle);
            sb.Append(')');
        }

        public override void TranslateStringStartsWith(StringBuilder sb, Expression haystack, Expression needle)
        {
            this.TranslateExpression(sb, haystack);
            sb.Append(".StartsWith(");
            this.TranslateExpression(sb, needle);
            sb.Append(')');
        }

        public override void TranslateStringToLower(StringBuilder sb, Expression str)
        {
            this.TranslateExpression(sb, str);
            sb.Append(".ToLower()");
        }

        public override void TranslateStringToUpper(StringBuilder sb, Expression str)
        {
            this.TranslateExpression(sb, str);
            sb.Append(".ToUpper()");
        }

        public override void TranslateStringTrim(StringBuilder sb, Expression str)
        {
            this.TranslateExpression(sb, str);
            sb.Append(".Trim()");
        }

        public override void TranslateStringTrimEnd(StringBuilder sb, Expression str)
        {
            this.TranslateExpression(sb, str);
            sb.Append(".TrimEnd()");
        }

        public override void TranslateStringTrimStart(StringBuilder sb, Expression str)
        {
            this.TranslateExpression(sb, str);
            sb.Append(".TrimStart()");
        }

        public override void TranslateStrongReferenceEquality(StringBuilder sb, Expression left, Expression right)
        {
            this.TranslateExpression(sb, left);
            sb.Append(" == ");
            this.TranslateExpression(sb, right);
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
