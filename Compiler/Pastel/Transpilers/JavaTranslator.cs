using Pastel.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pastel.Transpilers
{
    internal class JavaTranslator : CurlyBraceTranslator
    {
        private readonly bool isJava6;

        public JavaTranslator(bool isJava6) : base("  ", "\n", true)
        {
            this.isJava6 = isJava6;
        }

        public override string HelperCodeResourcePath { get { return "Transpilers/Resources/PastelHelper.java"; } }

        public override string TranslateType(PType type)
        {
            return TranslateJavaType(type);
        }

        private bool IsJavaPrimitiveTypeBoxed(PType type)
        {
            switch (type.RootValue)
            {
                case "int":
                case "double":
                case "bool":
                case "byte":
                case "object":
                case "char":
                    return true;
                default:
                    return false;
            }
        }

        private string TranslateJavaType(PType type)
        {
            switch (type.RootValue)
            {
                case "void": return "void";
                case "byte": return "byte";
                case "int": return "int";
                case "char": return "char";
                case "double": return "double";
                case "bool": return "boolean";
                case "object": return "Object";
                case "string": return "String";

                case "Array":
                    string innerType = this.TranslateJavaType(type.Generics[0]);
                    return innerType + "[]";

                case "List":
                    return "ArrayList<" + this.TranslateJavaNestedType(type.Generics[0]) + ">";

                case "Dictionary":
                    return "HashMap<" + this.TranslateJavaNestedType(type.Generics[0]) + ", " + this.TranslateJavaNestedType(type.Generics[1]) + ">";

                case "Func":
                    return "java.lang.reflect.Method";

                case "ClassValue":
                    // java.lang.ClassValue collision
                    return "org.crayonlang.interpreter.structs.ClassValue";

                default:
                    if (type.IsStruct)
                    {
                        return type.TypeName;
                    }
                    throw new NotImplementedException();
            }
        }

        private string TranslateJavaNestedType(PType type)
        {
            switch (type.RootValue)
            {
                case "bool": return "Boolean";
                case "byte": return "Byte";
                case "char": return "Character";
                case "double": return "Double";
                case "int": return "Integer";
                default:
                    return this.TranslateJavaType(type);
            }
        }

        protected override void WrapCodeImpl(ProjectConfig config, List<string> lines, bool isForStruct)
        {
            if (!isForStruct && config.WrappingClassNameForFunctions != null)
            {
                PastelUtil.IndentLines(this.TabChar, lines);
                lines.InsertRange(0, new string[] { "public final class " + config.WrappingClassNameForFunctions + " {", "" });
                lines.Add("}");
            }

            List<string> prefixData = new List<string>();

            string nsValue = isForStruct ? config.NamespaceForStructs : config.NamespaceForFunctions;
            if (nsValue != null)
            {
                prefixData.AddRange(new string[] { "package " + nsValue + ";", "" });
            }

            if (config.Imports.Count > 0)
            {
                prefixData.AddRange(
                    config.Imports
                        .OrderBy(t => t)
                        .Select(t => "import " + t + ";")
                        .Concat(new string[] { "" }));
            }

            if (prefixData.Count > 0) lines.InsertRange(0, prefixData);
        }

        public override void TranslateFunctionPointerInvocation(TranspilerContext sb, FunctionPointerInvocation fpi)
        {
            sb.Append("((");
            sb.Append(this.TranslateType(fpi.ResolvedType));
            sb.Append(") TranslationHelper.invokeFunctionPointer(");
            this.TranslateExpression(sb, fpi.Root);
            sb.Append(", new Object[] {");
            this.TranslateCommaDelimitedExpressions(sb, fpi.Args);
            sb.Append("}))");
        }

        public override void TranslatePrintStdErr(TranspilerContext sb, Expression value)
        {
            sb.Append("PlatformTranslationHelper.printStdErr(");
            this.TranslateExpression(sb, value);
            sb.Append(')');
        }

        public override void TranslatePrintStdOut(TranspilerContext sb, Expression value)
        {
            sb.Append("PlatformTranslationHelper.printStdOut(");
            this.TranslateExpression(sb, value);
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
            sb.Append("String.join(");
            this.TranslateExpression(sb, sep);
            sb.Append(", ");
            this.TranslateExpression(sb, array);
            sb.Append(')');
        }

        public override void TranslateArrayLength(TranspilerContext sb, Expression array)
        {
            this.TranslateExpression(sb, array);
            sb.Append(".length");
        }

        public override void TranslateArrayNew(TranspilerContext sb, PType arrayType, Expression lengthExpression)
        {
            // In the event of multi-dimensional jagged arrays, the outermost array length goes in the innermost bracket.
            // Unwrap nested arrays in the type and run the code as normal, and then add that many []'s to the end.
            int bracketSuffixCount = 0;
            while (arrayType.RootValue == "Array")
            {
                arrayType = arrayType.Generics[0];
                bracketSuffixCount++;
            }

            sb.Append("new ");
            if (arrayType.RootValue == "Dictionary")
            {
                sb.Append("HashMap");
            }
            else if (arrayType.RootValue == "List")
            {
                sb.Append("ArrayList");
            }
            else
            {
                sb.Append(this.TranslateType(arrayType));
            }
            sb.Append('[');
            this.TranslateExpression(sb, lengthExpression);
            sb.Append(']');

            while (bracketSuffixCount-- > 0)
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
            sb.Append("PST_base64ToString(");
            this.TranslateExpression(sb, base64String);
            sb.Append(')');
        }

        public override void TranslateCast(TranspilerContext sb, PType type, Expression expression)
        {
            DotField dotField = expression as DotField;
            if (dotField != null &&
                dotField.Root.ResolvedType.RootValue == "Value" &&
                dotField.FieldName.Value == "internalValue")
            {
                if (type.RootValue == "int")
                {
                    this.TranslateExpression(sb, dotField.Root);
                    sb.Append(".intValue");
                    return;
                }
                else if (type.RootValue == "bool")
                {
                    sb.Append('(');
                    this.TranslateExpression(sb, dotField.Root);
                    sb.Append(".intValue == 1)");
                    return;
                }
            }

            sb.Append('(');
            if (this.isJava6) // "(int) object" vs "(Integer) object"
            {
                string castRootType = this.TranslateType(type);
                if (expression is CastExpression)
                {
                    CastExpression ce = (CastExpression)expression;
                    string outerType = castRootType;
                    string innerType = this.TranslateType(expression.ResolvedType);

                    if ((outerType == "int" || outerType == "double") &&
                        (innerType == "int" || innerType == "double"))
                    {
                        switch (outerType + "+" + innerType)
                        {
                            case "int+double":
                                sb.Append("(int) (double) (Double) ");
                                this.TranslateExpression(sb, ce.Expression);
                                sb.Append(')');
                                return;
                            case "double+int":
                                sb.Append("(double) (int) (Integer) ");
                                this.TranslateExpression(sb, ce.Expression);
                                sb.Append(')');
                                return;
                            default:
                                break;
                        }
                    }
                }

                switch (castRootType)
                {
                    case "bool":
                    case "int":
                    case "double":
                    case "char":
                        sb.Append('(');
                        sb.Append(this.TranslateType(type));
                        sb.Append(") (");
                        sb.Append(this.TranslateJavaNestedType(type));
                        sb.Append(") ");
                        this.TranslateExpression(sb, expression);
                        sb.Append(')');
                        return;
                    default:
                        break;
                }
            }
            sb.Append('(');
            sb.Append(this.TranslateType(type));
            sb.Append(") ");

            this.TranslateExpression(sb, expression);
            sb.Append(')');
        }

        public override void TranslateCharConstant(TranspilerContext sb, char value)
        {
            sb.Append(PastelUtil.ConvertCharToCharConstantCode(value));
        }

        public override void TranslateCharToString(TranspilerContext sb, Expression charValue)
        {
            sb.Append("(\"\" + ");
            this.TranslateExpression(sb, charValue);
            sb.Append(')');
        }

        public override void TranslateChr(TranspilerContext sb, Expression charCode)
        {
            sb.Append("Character.toString((char) ");
            this.TranslateExpression(sb, charCode);
            sb.Append(")");
        }

        public override void TranslateConstructorInvocation(TranspilerContext sb, ConstructorInvocation constructorInvocation)
        {
            if (constructorInvocation.StructType.NameToken.Value == "Value")
            {
                Expression firstArg = constructorInvocation.Args[0];
                if (!(firstArg is InlineConstant))
                {
                    throw new InvalidOperationException("Cannot pass in non constant for first arg of Value construction.");
                }

                int type = (int)((InlineConstant)firstArg).Value;
                if (type == 2 || type == 3)
                {
                    sb.Append("new Value(");
                    this.TranslateExpression(sb, constructorInvocation.Args[1]);
                    sb.Append(')');
                    return;
                }
            }

            sb.Append("new ");
            string structType = constructorInvocation.StructType.NameToken.Value;
            if (structType == "ClassValue")
            {
                structType = "org.crayonlang.interpreter.structs.ClassValue";
            }
            sb.Append(structType);
            sb.Append('(');
            Expression[] args = constructorInvocation.Args;
            for (int i = 0; i < args.Length; ++i)
            {
                if (i > 0) sb.Append(", ");
                this.TranslateExpression(sb, args[i]);
            }
            sb.Append(')');
        }

        public override void TranslateCurrentTimeSeconds(TranspilerContext sb)
        {
            sb.Append("System.currentTimeMillis() / 1000.0");
        }

        public override void TranslateDictionaryContainsKey(TranspilerContext sb, Expression dictionary, Expression key)
        {
            this.TranslateExpression(sb, dictionary);
            sb.Append(".containsKey(");
            this.TranslateExpression(sb, key);
            sb.Append(')');
        }

        public override void TranslateDictionaryGet(TranspilerContext sb, Expression dictionary, Expression key)
        {
            this.TranslateExpression(sb, dictionary);
            sb.Append(".get(");
            this.TranslateExpression(sb, key);
            sb.Append(')');
        }

        public override void TranslateDictionaryKeys(TranspilerContext sb, Expression dictionary)
        {
            sb.Append("PST_convert");
            switch (dictionary.ResolvedType.Generics[0].RootValue)
            {
                case "int": sb.Append("Integer"); break;
                case "string": sb.Append("String"); break;

                default:
                    // TODO: Explicitly disallow dictionaries with non-intenger or non-string keys at compile time.
                    throw new NotImplementedException();
            }
            sb.Append("SetToArray(");
            this.TranslateExpression(sb, dictionary);
            sb.Append(".keySet())");

            // TODO: do a simple .keySet().toArray(TranslationHelper.STATIC_INSTANCE_OF_ZERO_LENGTH_INT_OR_STRING_ARRAY);
        }

        public override void TranslateDictionaryNew(TranspilerContext sb, PType keyType, PType valueType)
        {
            sb.Append("new HashMap<");
            sb.Append(this.TranslateJavaNestedType(keyType));
            sb.Append(", ");
            sb.Append(this.TranslateJavaNestedType(valueType));
            sb.Append(">()");
        }

        public override void TranslateDictionaryRemove(TranspilerContext sb, Expression dictionary, Expression key)
        {
            this.TranslateExpression(sb, dictionary);
            sb.Append(".remove(");
            this.TranslateExpression(sb, key);
            sb.Append(')');
        }

        public override void TranslateDictionarySet(TranspilerContext sb, Expression dictionary, Expression key, Expression value)
        {
            this.TranslateExpression(sb, dictionary);
            sb.Append(".put(");
            this.TranslateExpression(sb, key);
            sb.Append(", ");
            this.TranslateExpression(sb, value);
            sb.Append(')');
        }

        public override void TranslateDictionarySize(TranspilerContext sb, Expression dictionary)
        {
            this.TranslateExpression(sb, dictionary);
            sb.Append(".size()");
        }

        public override void TranslateDictionaryTryGet(TranspilerContext sb, Expression dictionary, Expression key, Expression fallbackValue, Variable varOut)
        {
            PType[] dictTypes = dictionary.ResolvedType.Generics;
            PType keyType = dictTypes[0];
            PType valueType = dictTypes[1];
            bool keyTypeIsBoxed = this.IsJavaPrimitiveTypeBoxed(keyType);
            bool keyExpressionIsSimple = key is Variable || key is InlineConstant;
            string keyVar = null;
            if (!keyExpressionIsSimple)
            {
                keyVar = "dictKey" + sb.SwitchCounter++;
                sb.Append(sb.CurrentTab);
                sb.Append(this.TranslateType(keyType));
                sb.Append(" = ");
                this.TranslateExpression(sb, key);
                sb.Append(";");
                sb.Append(this.NewLine);
            }

            string lookupSuffix = "" + sb.SwitchCounter++;
            sb.Append(sb.CurrentTab);
            sb.Append(this.TranslateJavaNestedType(valueType));
            sb.Append(" dictLookup");
            sb.Append(lookupSuffix);
            sb.Append(" = ");
            this.TranslateExpression(sb, dictionary);
            sb.Append(".get(");
            if (keyExpressionIsSimple)
            {
                this.TranslateExpression(sb, key);
            }
            else
            {
                sb.Append(keyVar);
            }
            sb.Append(");");
            sb.Append(this.NewLine);
            sb.Append(sb.CurrentTab);
            sb.Append(varOut.Name);
            sb.Append(" = dictLookup");
            sb.Append(lookupSuffix);
            sb.Append(" == null ? (");

            if (!keyTypeIsBoxed)
            {
                // if the key is not a primitive, then we don't know if this null is a lack of a value or
                // if it's the actual desired value. We must explicitly call .containsKey to be certain.
                // In this specific case, we must do a double-lookup.
                this.TranslateExpression(sb, dictionary);
                sb.Append(".containsKey(");
                if (keyExpressionIsSimple) this.TranslateExpression(sb, key);
                else sb.Append(keyVar);
                sb.Append(") ? null : (");
                this.TranslateExpression(sb, fallbackValue);
                sb.Append(")");
            }
            else
            {
                this.TranslateExpression(sb, fallbackValue);
            }
            sb.Append(") : dictLookup");
            sb.Append(lookupSuffix);
            sb.Append(';');
            sb.Append(this.NewLine);
        }

        public override void TranslateDictionaryValues(TranspilerContext sb, Expression dictionary)
        {
            this.TranslateExpression(sb, dictionary);
            sb.Append(".values()");
        }

        public override void TranslateFloatBuffer16(TranspilerContext sb)
        {
            sb.Append("PST_floatBuffer16");
        }

        public override void TranslateFloatDivision(TranspilerContext sb, Expression floatNumerator, Expression floatDenominator)
        {
            this.TranslateExpression(sb, floatNumerator);
            sb.Append(" / ");
            this.TranslateExpression(sb, floatDenominator);
        }

        public override void TranslateFloatToInt(TranspilerContext sb, Expression floatExpr)
        {
            sb.Append("((int) ");
            this.TranslateExpression(sb, floatExpr);
            sb.Append(')');
        }

        public override void TranslateFloatToString(TranspilerContext sb, Expression floatExpr)
        {
            sb.Append("Double.toString(");
            this.TranslateExpression(sb, floatExpr);
            sb.Append(')');
        }

        public override void TranslateGetFunction(TranspilerContext sb, Expression name)
        {
            sb.Append("TranslationHelper.getFunction(");
            this.TranslateExpression(sb, name);
            sb.Append(')');
        }

        public override void TranslateIntBuffer16(TranspilerContext sb)
        {
            sb.Append("PST_intBuffer16");
        }

        public override void TranslateIntegerDivision(TranspilerContext sb, Expression integerNumerator, Expression integerDenominator)
        {
            this.TranslateExpression(sb, integerNumerator);
            sb.Append(" / ");
            this.TranslateExpression(sb, integerDenominator);
        }

        public override void TranslateIntToString(TranspilerContext sb, Expression integer)
        {
            sb.Append("Integer.toString(");
            this.TranslateExpression(sb, integer);
            sb.Append(')');
        }

        public override void TranslateIsValidInteger(TranspilerContext sb, Expression stringValue)
        {
            sb.Append("PST_isValidInteger(");
            this.TranslateExpression(sb, stringValue);
            sb.Append(')');
        }

        public override void TranslateListAdd(TranspilerContext sb, Expression list, Expression item)
        {
            this.TranslateExpression(sb, list);
            sb.Append(".add(");
            this.TranslateExpression(sb, item);
            sb.Append(')');
        }

        public override void TranslateListClear(TranspilerContext sb, Expression list)
        {
            this.TranslateExpression(sb, list);
            sb.Append(".clear()");
        }

        public override void TranslateListConcat(TranspilerContext sb, Expression list, Expression items)
        {
            // Fun fact: this is actually not implemented. The only place that this is used is by Value.
            sb.Append("PST_concatLists(");
            this.TranslateExpression(sb, list);
            sb.Append(", ");
            this.TranslateExpression(sb, items);
            sb.Append(')');
        }

        public override void TranslateListGet(TranspilerContext sb, Expression list, Expression index)
        {
            this.TranslateExpression(sb, list);
            sb.Append(".get(");
            this.TranslateExpression(sb, index);
            sb.Append(')');
        }

        public override void TranslateListInsert(TranspilerContext sb, Expression list, Expression index, Expression item)
        {
            this.TranslateExpression(sb, list);
            sb.Append(".add(");
            this.TranslateExpression(sb, index);
            sb.Append(", ");
            this.TranslateExpression(sb, item);
            sb.Append(')');
        }

        public override void TranslateListJoinChars(TranspilerContext sb, Expression list)
        {
            sb.Append("PST_joinChars(");
            this.TranslateExpression(sb, list);
            sb.Append(')');
        }

        public override void TranslateListJoinStrings(TranspilerContext sb, Expression list, Expression sep)
        {
            sb.Append("PST_joinList(");
            this.TranslateExpression(sb, sep);
            sb.Append(", ");
            this.TranslateExpression(sb, list);
            sb.Append(')');
        }

        public override void TranslateListNew(TranspilerContext sb, PType type)
        {
            sb.Append("new ArrayList<");
            sb.Append(this.TranslateJavaNestedType(type));
            sb.Append(">()");
        }

        public override void TranslateListPop(TranspilerContext sb, Expression list)
        {
            bool useInlineListPop =
                (list is Variable) ||
                (list is DotField && ((DotField)list).Root is Variable);

            if (useInlineListPop)
            {
                this.TranslateExpression(sb, list);
                sb.Append(".remove(");
                this.TranslateExpression(sb, list);
                sb.Append(".size() - 1)");
            }
            else
            {
                sb.Append("PST_listPop(");
                this.TranslateExpression(sb, list);
                sb.Append(')');
            }
        }

        public override void TranslateListRemoveAt(TranspilerContext sb, Expression list, Expression index)
        {
            this.TranslateExpression(sb, list);
            sb.Append(".remove(");
            this.TranslateExpression(sb, index);
            sb.Append(')');
        }

        public override void TranslateListReverse(TranspilerContext sb, Expression list)
        {
            sb.Append("java.util.Collections.reverse(");
            this.TranslateExpression(sb, list);
            sb.Append(')');
        }

        public override void TranslateListSet(TranspilerContext sb, Expression list, Expression index, Expression value)
        {
            this.TranslateExpression(sb, list);
            sb.Append(".set(");
            this.TranslateExpression(sb, index);
            sb.Append(", ");
            this.TranslateExpression(sb, value);
            sb.Append(')');
        }

        public override void TranslateListShuffle(TranspilerContext sb, Expression list)
        {
            sb.Append("PST_listShuffle(");
            // This is currently only used and implemented by Value lists.
            this.TranslateExpression(sb, list);
            sb.Append(')');
        }

        public override void TranslateListSize(TranspilerContext sb, Expression list)
        {
            this.TranslateExpression(sb, list);
            sb.Append(".size()");
        }

        public override void TranslateListToArray(TranspilerContext sb, Expression list)
        {
            PType itemType = list.ResolvedType.Generics[0];
            switch (itemType.TypeName)
            {
                case "bool":
                case "byte":
                case "int":
                case "double":
                case "char":
                    string primitiveName = itemType.TypeName;
                    sb.Append("PST_listToArray");
                    sb.Append((char)(primitiveName[0] + 'A' - 'a'));
                    sb.Append(primitiveName.Substring(1));
                    sb.Append('(');
                    this.TranslateExpression(sb, list);
                    sb.Append(')');
                    break;

                case "string":
                    this.TranslateExpression(sb, list);
                    sb.Append(".toArray(PST_emptyArrayString)");
                    break;

                case "object":
                    this.TranslateExpression(sb, list);
                    sb.Append(".toArray()");
                    break;

                case "List":
                    this.TranslateExpression(sb, list);
                    sb.Append(".toArray(PST_emptyArrayList)");
                    break;
                case "Dictionary":
                    this.TranslateExpression(sb, list);
                    sb.Append(".toArray(PST_emptyArrayMap)");
                    break;
                case "Array":
                    throw new NotImplementedException("not implemented: java list of arrays to array");
                default:
                    if (itemType.IsStruct)
                    {
                        this.TranslateExpression(sb, list);
                        sb.Append(".toArray(");
                        sb.Append(this.TranslateType(itemType));
                        sb.Append(".EMPTY_ARRAY)");
                    }
                    else
                    {
                        // I think I covered all the types that are supported.
                        throw new NotImplementedException();
                    }
                    break;
            }
        }

        public override void TranslateMathArcCos(TranspilerContext sb, Expression ratio)
        {
            sb.Append("Math.acos(");
            this.TranslateExpression(sb, ratio);
            sb.Append(')');
        }

        public override void TranslateMathArcSin(TranspilerContext sb, Expression ratio)
        {
            sb.Append("Math.asin(");
            this.TranslateExpression(sb, ratio);
            sb.Append(')');
        }

        public override void TranslateMathArcTan(TranspilerContext sb, Expression yComponent, Expression xComponent)
        {
            sb.Append("Math.atan2(");
            this.TranslateExpression(sb, yComponent);
            sb.Append(", ");
            this.TranslateExpression(sb, xComponent);
            sb.Append(')');
        }

        public override void TranslateMathCos(TranspilerContext sb, Expression thetaRadians)
        {
            sb.Append("Math.cos(");
            this.TranslateExpression(sb, thetaRadians);
            sb.Append(')');
        }

        public override void TranslateMathLog(TranspilerContext sb, Expression value)
        {
            sb.Append("Math.log(");
            this.TranslateExpression(sb, value);
            sb.Append(')');
        }

        public override void TranslateMathPow(TranspilerContext sb, Expression expBase, Expression exponent)
        {
            sb.Append("Math.pow(");
            this.TranslateExpression(sb, expBase);
            sb.Append(", ");
            this.TranslateExpression(sb, exponent);
            sb.Append(')');
        }

        public override void TranslateMathSin(TranspilerContext sb, Expression thetaRadians)
        {
            sb.Append("Math.sin(");
            this.TranslateExpression(sb, thetaRadians);
            sb.Append(')');
        }

        public override void TranslateMathTan(TranspilerContext sb, Expression thetaRadians)
        {
            sb.Append("Math.tan(");
            this.TranslateExpression(sb, thetaRadians);
            sb.Append(')');
        }

        public override void TranslateMultiplyList(TranspilerContext sb, Expression list, Expression n)
        {
            // not implemented yet because it's not used yet.
            sb.Append("PST_multiplyList(");
            this.TranslateExpression(sb, list);
            sb.Append(", ");
            this.TranslateExpression(sb, n);
            sb.Append(')');
        }

        public override void TranslateNullConstant(TranspilerContext sb)
        {
            sb.Append("null");
        }

        /*
        public override void TranslateOpChain(TranspilerContext sb, OpChain opChain)
        {
            if (this.isJava6 && opChain.Expressions.Length == 2)
            {
                string op = opChain.Ops[0].Value;
                switch (op)
                {
                    case "==":
                    case "!=":
                        Expression left = opChain.Expressions[0];
                        Expression right = opChain.Expressions[1];
                        if (left is CastExpression || right is CastExpression)
                        {
                            if (!(left is CastExpression))
                            {
                                Expression t = left;
                                left = right;
                                right = t;
                            }
                            if (op == "!=")
                            {
                                sb.Append('!');
                            }
                            sb.Append('(');
                            this.TranslateExpression(sb, left);
                            sb.Append(").equals(");
                            this.TranslateExpression(sb, right);
                            sb.Append(')');
                            return;
                        }
                        break;

                    default:
                        // fall back to regular behavior
                        break;
                }
            }

            base.TranslateOpChain(sb, opChain);
        }
        //*/

        public override void TranslateOrd(TranspilerContext sb, Expression charValue)
        {
            sb.Append("((int)(");
            this.TranslateExpression(sb, charValue);
            sb.Append("))");
        }

        public override void TranslateParseFloatUnsafe(TranspilerContext sb, Expression stringValue)
        {
            sb.Append("Double.parseDouble(");
            this.TranslateExpression(sb, stringValue);
            sb.Append(')');
        }

        public override void TranslateParseInt(TranspilerContext sb, Expression safeStringValue)
        {
            sb.Append("Integer.parseInt(");
            this.TranslateExpression(sb, safeStringValue);
            sb.Append(')');
        }

        public override void TranslateRandomFloat(TranspilerContext sb)
        {
            sb.Append("PST_random.nextDouble()");
        }

        public override void TranslateSortedCopyOfIntArray(TranspilerContext sb, Expression intArray)
        {
            sb.Append("PST_sortedCopyOfIntArray(");
            this.TranslateExpression(sb, intArray);
            sb.Append(')');
        }

        public override void TranslateSortedCopyOfStringArray(TranspilerContext sb, Expression stringArray)
        {
            sb.Append("PST_sortedCopyOfStringArray(");
            this.TranslateExpression(sb, stringArray);
            sb.Append(')');
        }

        public override void TranslateStringAppend(TranspilerContext sb, Expression str1, Expression str2)
        {
            this.TranslateExpression(sb, str1);
            sb.Append(" += ");
            this.TranslateExpression(sb, str2);
        }

        public override void TranslateStringBuffer16(TranspilerContext sb)
        {
            sb.Append("PST_stringBuffer16");
        }

        public override void TranslateStringCharAt(TranspilerContext sb, Expression str, Expression index)
        {
            this.TranslateExpression(sb, str);
            sb.Append(".charAt(");
            this.TranslateExpression(sb, index);
            sb.Append(')');
        }

        public override void TranslateStringCharCodeAt(TranspilerContext sb, Expression str, Expression index)
        {
            sb.Append("((int) ");
            this.TranslateExpression(sb, str);
            sb.Append(".charAt(");
            this.TranslateExpression(sb, index);
            sb.Append("))");
        }

        public override void TranslateStringCompareIsReverse(TranspilerContext sb, Expression str1, Expression str2)
        {
            sb.Append('(');
            this.TranslateExpression(sb, str1);
            sb.Append(".compareTo(");
            this.TranslateExpression(sb, str2);
            sb.Append(") > 0)");
        }

        public override void TranslateStringConcatAll(TranspilerContext sb, Expression[] strings)
        {
            this.TranslateExpression(sb, strings[0]);
            for (int i = 1; i < strings.Length; ++i)
            {
                sb.Append(" + ");
                this.TranslateExpression(sb, strings[i]);
            }
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
            sb.Append(".contains(");
            this.TranslateExpression(sb, needle);
            sb.Append(')');
        }

        public override void TranslateStringEndsWith(TranspilerContext sb, Expression haystack, Expression needle)
        {
            this.TranslateExpression(sb, haystack);
            sb.Append(".endsWith(");
            this.TranslateExpression(sb, needle);
            sb.Append(')');
        }

        public override void TranslateStringEquals(TranspilerContext sb, Expression left, Expression right)
        {
            this.TranslateExpression(sb, left);
            sb.Append(".equals(");
            this.TranslateExpression(sb, right);
            sb.Append(')');
        }

        public override void TranslateStringFromCharCode(TranspilerContext sb, Expression charCode)
        {
            sb.Append("Character.toString((char) ");
            this.TranslateExpression(sb, charCode);
            sb.Append(")");
        }

        public override void TranslateStringIndexOf(TranspilerContext sb, Expression haystack, Expression needle)
        {
            this.TranslateExpression(sb, haystack);
            sb.Append(".indexOf(");
            this.TranslateExpression(sb, needle);
            sb.Append(')');
        }

        public override void TranslateStringIndexOfWithStart(TranspilerContext sb, Expression haystack, Expression needle, Expression startIndex)
        {
            this.TranslateExpression(sb, haystack);
            sb.Append(".indexOf(");
            this.TranslateExpression(sb, needle);
            sb.Append(", ");
            this.TranslateExpression(sb, startIndex);
            sb.Append(')');
        }

        public override void TranslateStringLength(TranspilerContext sb, Expression str)
        {
            this.TranslateExpression(sb, str);
            sb.Append(".length()");
        }

        public override void TranslateStringReplace(TranspilerContext sb, Expression haystack, Expression needle, Expression newNeedle)
        {
            this.TranslateExpression(sb, haystack);
            sb.Append(".replace((CharSequence) ");
            this.TranslateExpression(sb, needle);
            sb.Append(", (CharSequence) ");
            this.TranslateExpression(sb, newNeedle);
            sb.Append(')');
        }

        public override void TranslateStringReverse(TranspilerContext sb, Expression str)
        {
            sb.Append("PST_reverseString(");
            this.TranslateExpression(sb, str);
            sb.Append(')');
        }

        public override void TranslateStringSplit(TranspilerContext sb, Expression haystack, Expression needle)
        {
            sb.Append("PST_literalStringSplit(");
            this.TranslateExpression(sb, haystack);
            sb.Append(", ");
            this.TranslateExpression(sb, needle);
            sb.Append(')');
        }

        public override void TranslateStringStartsWith(TranspilerContext sb, Expression haystack, Expression needle)
        {
            this.TranslateExpression(sb, haystack);
            sb.Append(".startsWith(");
            this.TranslateExpression(sb, needle);
            sb.Append(')');
        }

        public override void TranslateStringSubstring(TranspilerContext sb, Expression str, Expression start, Expression length)
        {
            this.TranslateExpression(sb, str);
            sb.Append(".substring(");
            this.TranslateExpression(sb, start);
            sb.Append(", ");
            this.TranslateExpression(sb, start);
            sb.Append(" + ");
            this.TranslateExpression(sb, length);
            sb.Append(')');
        }

        public override void TranslateStringSubstringIsEqualTo(TranspilerContext sb, Expression haystack, Expression startIndex, Expression needle)
        {
            sb.Append("PST_checkStringInString(");
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
            sb.Append(".toLowerCase()");
        }

        public override void TranslateStringToUpper(TranspilerContext sb, Expression str)
        {
            this.TranslateExpression(sb, str);
            sb.Append(".toUpperCase()");
        }

        public override void TranslateStringTrim(TranspilerContext sb, Expression str)
        {
            this.TranslateExpression(sb, str);
            sb.Append(".trim()");
        }

        public override void TranslateStringTrimEnd(TranspilerContext sb, Expression str)
        {
            sb.Append("PST_trimSide(");
            this.TranslateExpression(sb, str);
            sb.Append(", false)");
        }

        public override void TranslateStringTrimStart(TranspilerContext sb, Expression str)
        {
            sb.Append("PST_trimSide(");
            this.TranslateExpression(sb, str);
            sb.Append(", true)");
        }

        public override void TranslateStrongReferenceEquality(TranspilerContext sb, Expression left, Expression right)
        {
            throw new NotImplementedException();
        }

        public override void TranslateStructFieldDereference(TranspilerContext sb, Expression root, StructDefinition structDef, string fieldName, int fieldIndex)
        {
            this.TranslateExpression(sb, root);
            sb.Append('.');
            sb.Append(fieldName);
        }

        public override void TranslateTryParseFloat(TranspilerContext sb, Expression stringValue, Expression floatOutList)
        {
            sb.Append("PST_parseFloatOrReturnNull(");
            this.TranslateExpression(sb, floatOutList);
            sb.Append(", ");
            this.TranslateExpression(sb, stringValue);
            sb.Append(')');
        }

        public override void TranslateVariableDeclaration(TranspilerContext sb, VariableDeclaration varDecl)
        {
            sb.Append(sb.CurrentTab);
            sb.Append(this.TranslateType(varDecl.Type));
            sb.Append(' ');
            sb.Append(varDecl.VariableNameToken.Value);
            if (varDecl.Value != null)
            {
                sb.Append(" = ");
                this.TranslateExpression(sb, varDecl.Value);
            }
            sb.Append(';');
            sb.Append(this.NewLine);
        }

        public override void GenerateCodeForFunction(TranspilerContext sb, FunctionDefinition funcDef)
        {
            sb.Append(sb.CurrentTab);
            sb.Append("public static ");
            sb.Append(this.TranslateType(funcDef.ReturnType));
            sb.Append(' ');
            sb.Append(funcDef.NameToken.Value);
            sb.Append('(');
            Pastel.Token[] argNames = funcDef.ArgNames;
            PType[] argTypes = funcDef.ArgTypes;
            for (int i = 0; i < argTypes.Length; ++i)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(this.TranslateType(argTypes[i]));
                sb.Append(' ');
                sb.Append(argNames[i].Value);
            }
            sb.Append(") {");
            sb.Append(this.NewLine);
            sb.TabDepth++;
            this.TranslateExecutables(sb, funcDef.Code);
            sb.TabDepth--;
            sb.Append(sb.CurrentTab);
            sb.Append('}');
            sb.Append(this.NewLine);
        }

        public override void GenerateCodeForStruct(TranspilerContext sb, StructDefinition structDef)
        {
            string name = structDef.NameToken.Value;
            bool isValue = name == "Value";
            sb.Append("public final class ");
            sb.Append(name);
            sb.Append(" {");
            sb.Append(this.NewLine);
            string[] types = structDef.ArgTypes.Select(type => this.TranslateType(type)).ToArray();
            string[] names = structDef.ArgNames.Select(token => token.Value).ToArray();
            int fieldCount = names.Length;
            for (int i = 0; i < fieldCount; ++i)
            {
                sb.Append("  public ");
                sb.Append(types[i]);
                sb.Append(' ');
                sb.Append(names[i]);
                sb.Append(';');
                sb.Append(this.NewLine);
            }

            sb.Append("  public static final ");
            sb.Append(name);
            sb.Append("[] EMPTY_ARRAY = new ");
            sb.Append(name);
            sb.Append("[0];");
            sb.Append(this.NewLine);

            if (isValue)
            {
                // The overhead of having extra fields on each Value is much less than the overhead
                // of Java's casting. Particularly on Android.
                sb.Append("  public int intValue;");
                sb.Append(this.NewLine);
            }

            sb.Append(this.NewLine);
            sb.Append("  public ");
            sb.Append(structDef.NameToken.Value);
            sb.Append('(');
            for (int i = 0; i < fieldCount; ++i)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(types[i]);
                sb.Append(' ');
                sb.Append(names[i]);
            }
            sb.Append(") {");
            sb.Append(this.NewLine);
            for (int i = 0; i < fieldCount; ++i)
            {
                sb.Append("    this.");
                sb.Append(names[i]);
                sb.Append(" = ");
                sb.Append(names[i]);
                sb.Append(';');
                sb.Append(this.NewLine);
            }
            sb.Append("  }");

            if (isValue)
            {
                sb.Append(this.NewLine);
                sb.Append(this.NewLine);
                sb.Append("  public Value(int intValue) {");
                sb.Append(this.NewLine);
                sb.Append("    this.type = 3;");
                sb.Append(this.NewLine);
                sb.Append("    this.intValue = intValue;");
                sb.Append(this.NewLine);
                sb.Append("    this.internalValue = intValue;");
                sb.Append(this.NewLine);
                sb.Append("  }");
                sb.Append(this.NewLine);
                sb.Append(this.NewLine);
                sb.Append("  public Value(boolean boolValue) {");
                sb.Append(this.NewLine);
                sb.Append("    this.type = 2;");
                sb.Append(this.NewLine);
                sb.Append("    this.intValue = boolValue ? 1 : 0;");
                sb.Append(this.NewLine);
                sb.Append("    this.internalValue = boolValue;");
                sb.Append(this.NewLine);
                sb.Append("  }");
            }

            sb.Append(this.NewLine);
            sb.Append("}");
        }
    }
}
