using Common;
using Pastel.Nodes;
using System;
using System.Text;

namespace LangJava
{
    public abstract class JavaTranslator : Platform.CurlyBraceTranslator
    {
        private bool isJava6;

        public JavaTranslator(Platform.AbstractPlatform platform) : base(platform, "  ", "\n", true)
        {
            this.isJava6 = platform.Name == "java-app-android"; // TODO: be more ashamed of this.
        }

        public override void TranslateArrayGet(StringBuilder sb, Expression array, Expression index)
        {
            this.TranslateExpression(sb, array);
            sb.Append('[');
            this.TranslateExpression(sb, index);
            sb.Append(']');
        }

        public override void TranslateArrayJoin(StringBuilder sb, Expression array, Expression sep)
        {
            throw new NotImplementedException();
        }

        public override void TranslateArrayLength(StringBuilder sb, Expression array)
        {
            this.TranslateExpression(sb, array);
            sb.Append(".length");
        }

        public override void TranslateArrayNew(StringBuilder sb, PType arrayType, Expression lengthExpression)
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
                sb.Append(this.Platform.TranslateType(arrayType));
            }
            sb.Append('[');
            this.TranslateExpression(sb, lengthExpression);
            sb.Append(']');

            while (bracketSuffixCount-- > 0)
            {
                sb.Append("[]");
            }
        }

        public override void TranslateArraySet(StringBuilder sb, Expression array, Expression index, Expression value)
        {
            this.TranslateExpression(sb, array);
            sb.Append('[');
            this.TranslateExpression(sb, index);
            sb.Append("] = ");
            this.TranslateExpression(sb, value);
        }

        public override void TranslateBase64ToString(StringBuilder sb, Expression base64String)
        {
            sb.Append("TranslationHelper.base64ToString(");
            this.TranslateExpression(sb, base64String);
            sb.Append(')');
        }

        public override void TranslateCast(StringBuilder sb, PType type, Expression expression)
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
                else if (type.RootValue == "List")
                {
                    this.TranslateExpression(sb, dotField.Root);
                    sb.Append(".listValue");
                    return;
                }
            }

            sb.Append('(');
            if (this.isJava6) // "(int) object" vs "(Integer) object"
            {
                string castRootType = type.RootValue;
                if (expression is CastExpression)
                {
                    CastExpression ce = (CastExpression)expression;
                    string outerType = castRootType;
                    string innerType = expression.ResolvedType.RootValue;

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
                        sb.Append(this.Platform.TranslateType(type));
                        sb.Append(") (");
                        sb.Append(LangJava.PlatformImpl.TranslateJavaNestedType(type));
                        sb.Append(") ");
                        this.TranslateExpression(sb, expression);
                        sb.Append(')');
                        return;
                    default:
                        break;
                }
            }
            sb.Append('(');
            sb.Append(this.Platform.TranslateType(type));
            sb.Append(") ");

            this.TranslateExpression(sb, expression);
            sb.Append(')');
        }

        public override void TranslateCharConstant(StringBuilder sb, char value)
        {
            sb.Append(Util.ConvertCharToCharConstantCode(value));
        }

        public override void TranslateCharToString(StringBuilder sb, Expression charValue)
        {
            sb.Append("(\"\" + ");
            this.TranslateExpression(sb, charValue);
            sb.Append(')');
        }

        public override void TranslateChr(StringBuilder sb, Expression charCode)
        {
            sb.Append("Character.toString((char) ");
            this.TranslateExpression(sb, charCode);
            sb.Append(")");
        }

        public override void TranslateConstructorInvocation(StringBuilder sb, ConstructorInvocation constructorInvocation)
        {
            if (constructorInvocation.StructType.NameToken.Value == "Value")
            {
                Expression firstArg = constructorInvocation.Args[0];
                if (!(firstArg is InlineConstant))
                {
                    throw new InvalidOperationException("Cannot pass in non constant for first arg of Value construction.");
                }

                int type = (int)((InlineConstant)firstArg).Value;
                if (type == 2 || type == 3 || type == 6)
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

        public override void TranslateConvertRawDictionaryValueCollectionToAReusableValueList(StringBuilder sb, Expression dictionary)
        {
            TODO.PleaseRenameThisFunction();

            sb.Append("new FastList().initializeValueCollection(");
            this.TranslateExpression(sb, dictionary);
            sb.Append(')');
        }

        public override void TranslateCurrentTimeSeconds(StringBuilder sb)
        {
            sb.Append("System.currentTimeMillis() / 1000.0");
        }

        public override void TranslateDictionaryContainsKey(StringBuilder sb, Expression dictionary, Expression key)
        {
            this.TranslateExpression(sb, dictionary);
            sb.Append(".containsKey(");
            this.TranslateExpression(sb, key);
            sb.Append(')');
        }

        public override void TranslateDictionaryGet(StringBuilder sb, Expression dictionary, Expression key)
        {
            this.TranslateExpression(sb, dictionary);
            sb.Append(".get(");
            this.TranslateExpression(sb, key);
            sb.Append(')');
        }

        public override void TranslateDictionaryKeys(StringBuilder sb, Expression dictionary)
        {
            sb.Append("TranslationHelper.convert");
            switch (dictionary.ResolvedType.Generics[0].RootValue)
            {
                case "int": sb.Append("Integer"); break;
                case "string": sb.Append("String"); break;

                default:
                    TODO.ExplicitlyDisallowThisAtCompileTime();
                    throw new NotImplementedException();
            }
            sb.Append("SetToArray(");
            this.TranslateExpression(sb, dictionary);
            sb.Append(".keySet())");

            // TODO: do a simple .keySet().toArray(TranslationHelper.STATIC_INSTANCE_OF_ZERO_LENGTH_INT_OR_STRING_ARRAY);
        }

        public override void TranslateDictionaryKeysToValueList(StringBuilder sb, Expression dictionary)
        {
            throw new NotImplementedException();
        }

        public override void TranslateDictionaryNew(StringBuilder sb, PType keyType, PType valueType)
        {
            sb.Append("new HashMap<");
            sb.Append(LangJava.PlatformImpl.TranslateJavaNestedType(keyType));
            sb.Append(", ");
            sb.Append(LangJava.PlatformImpl.TranslateJavaNestedType(valueType));
            sb.Append(">()");
        }

        public override void TranslateDictionaryRemove(StringBuilder sb, Expression dictionary, Expression key)
        {
            this.TranslateExpression(sb, dictionary);
            sb.Append(".remove(");
            this.TranslateExpression(sb, key);
            sb.Append(')');
        }

        public override void TranslateDictionarySet(StringBuilder sb, Expression dictionary, Expression key, Expression value)
        {
            this.TranslateExpression(sb, dictionary);
            sb.Append(".put(");
            this.TranslateExpression(sb, key);
            sb.Append(", ");
            this.TranslateExpression(sb, value);
            sb.Append(')');
        }

        public override void TranslateDictionarySize(StringBuilder sb, Expression dictionary)
        {
            this.TranslateExpression(sb, dictionary);
            sb.Append(".size()");
        }

        public override void TranslateDictionaryValues(StringBuilder sb, Expression dictionary)
        {
            this.TranslateExpression(sb, dictionary);
            sb.Append(".values()");
        }

        public override void TranslateDictionaryValuesToValueList(StringBuilder sb, Expression dictionary)
        {
            sb.Append("new FastList().initializeValueCollection(");
            this.TranslateExpression(sb, dictionary);
            sb.Append(".values())");
        }

        public override void TranslateFloatBuffer16(StringBuilder sb)
        {
            sb.Append("TranslationHelper.FLOAT_BUFFER_16");
        }

        public override void TranslateFloatDivision(StringBuilder sb, Expression floatNumerator, Expression floatDenominator)
        {
            this.TranslateExpression(sb, floatNumerator);
            sb.Append(" / ");
            this.TranslateExpression(sb, floatDenominator);
        }

        public override void TranslateFloatToInt(StringBuilder sb, Expression floatExpr)
        {
            sb.Append("((int) ");
            this.TranslateExpression(sb, floatExpr);
            sb.Append(')');
        }

        public override void TranslateFloatToString(StringBuilder sb, Expression floatExpr)
        {
            sb.Append("Double.toString(");
            this.TranslateExpression(sb, floatExpr);
            sb.Append(')');
        }

        public override void TranslateFunctionInvocationInterpreterScoped(StringBuilder sb, FunctionReference funcRef, Expression[] args)
        {
            sb.Append("Interpreter.");
            base.TranslateFunctionInvocationInterpreterScoped(sb, funcRef, args);
        }

        public override void TranslateGetProgramData(StringBuilder sb)
        {
            sb.Append("TranslationHelper.getProgramData()");
        }

        public override void TranslateGetResourceManifest(StringBuilder sb)
        {
            sb.Append("TranslationHelper.getResourceManifest()");
        }

        public override void TranslateGlobalVariable(StringBuilder sb, Variable variable)
        {
            sb.Append("VmGlobal.");
            sb.Append(variable.Name);
        }

        public override void TranslateIntBuffer16(StringBuilder sb)
        {
            sb.Append("TranslationHelper.INT_BUFFER_16");
        }

        public override void TranslateIntegerDivision(StringBuilder sb, Expression integerNumerator, Expression integerDenominator)
        {
            this.TranslateExpression(sb, integerNumerator);
            sb.Append(" / ");
            this.TranslateExpression(sb, integerDenominator);
        }

        public override void TranslateIntToString(StringBuilder sb, Expression integer)
        {
            sb.Append("Integer.toString(");
            this.TranslateExpression(sb, integer);
            sb.Append(')');
        }

        public override void TranslateInvokeDynamicLibraryFunction(StringBuilder sb, Expression functionId, Expression argsArray)
        {
            sb.Append("((LibraryFunctionPointer)");
            this.TranslateExpression(sb, functionId);
            sb.Append(").invoke(");
            this.TranslateExpression(sb, argsArray);
            sb.Append(')');
        }

        public override void TranslateIsValidInteger(StringBuilder sb, Expression stringValue)
        {
            sb.Append("TranslationHelper.isValidInteger(");
            this.TranslateExpression(sb, stringValue);
            sb.Append(')');
        }

        public override void TranslateListAdd(StringBuilder sb, Expression list, Expression item)
        {
            this.TranslateExpression(sb, list);
            sb.Append(".add(");
            this.TranslateExpression(sb, item);
            sb.Append(')');
        }

        public override void TranslateListClear(StringBuilder sb, Expression list)
        {
            this.TranslateExpression(sb, list);
            sb.Append(".clear()");
        }

        public override void TranslateListConcat(StringBuilder sb, Expression list, Expression items)
        {
            if (list.ResolvedType.Generics[0].RootValue == "Value")
            {
                this.TranslateExpression(sb, list);
                sb.Append(".concat(");
                this.TranslateExpression(sb, items);
                sb.Append(')');
            }
            else
            {
                sb.Append("TranslationHelper.concatLists(");
                this.TranslateExpression(sb, list);
                sb.Append(", ");
                this.TranslateExpression(sb, items);
                sb.Append(')');
            }
        }

        public override void TranslateListGet(StringBuilder sb, Expression list, Expression index)
        {
            if (list.ResolvedType.Generics[0].RootValue == "Value")
            {
                this.TranslateExpression(sb, list);
                sb.Append(".items[");
                this.TranslateExpression(sb, index);
                sb.Append(']');
            }
            else
            {
                this.TranslateExpression(sb, list);
                sb.Append(".get(");
                this.TranslateExpression(sb, index);
                sb.Append(')');
            }
        }

        public override void TranslateListInsert(StringBuilder sb, Expression list, Expression index, Expression item)
        {

            if (list.ResolvedType.Generics[0].RootValue == "Value")
            {
                this.TranslateExpression(sb, list);
                sb.Append(".insert(");
                this.TranslateExpression(sb, index);
                sb.Append(", ");
                this.TranslateExpression(sb, item);
                sb.Append(')');
            }
            else
            {
                this.TranslateExpression(sb, list);
                sb.Append(".add(");
                this.TranslateExpression(sb, index);
                sb.Append(", ");
                this.TranslateExpression(sb, item);
                sb.Append(')');
            }
        }

        public override void TranslateListJoinChars(StringBuilder sb, Expression list)
        {
            sb.Append("TranslationHelper.joinChars(");
            this.TranslateExpression(sb, list);
            sb.Append(')');
        }

        public override void TranslateListJoinStrings(StringBuilder sb, Expression list, Expression sep)
        {
            sb.Append("TranslationHelper.joinList(");
            this.TranslateExpression(sb, sep);
            sb.Append(", ");
            this.TranslateExpression(sb, list);
            sb.Append(')');
        }

        public override void TranslateListNew(StringBuilder sb, PType type)
        {
            if (type.RootValue == "Value")
            {
                sb.Append("new FastList()");
            }
            else
            {
                sb.Append("new ArrayList<");
                sb.Append(LangJava.PlatformImpl.TranslateJavaNestedType(type));
                sb.Append(">()");
            }
        }

        public override void TranslateListPop(StringBuilder sb, Expression list)
        {
            if (list.ResolvedType.Generics[0].RootValue == "Value")
            {
                this.TranslateExpression(sb, list);
                sb.Append(".pop()");
            }
            else
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
                    sb.Append("TranslationHelper.listPop(");
                    this.TranslateExpression(sb, list);
                    sb.Append(')');
                }
            }
        }

        public override void TranslateListRemoveAt(StringBuilder sb, Expression list, Expression index)
        {
            this.TranslateExpression(sb, list);
            sb.Append(".remove(");
            this.TranslateExpression(sb, index);
            sb.Append(')');
        }

        public override void TranslateListReverse(StringBuilder sb, Expression list)
        {
            if (list.ResolvedType.Generics[0].RootValue == "Value")
            {
                this.TranslateExpression(sb, list);
                sb.Append(".reverse()");
            }
            else
            {
                sb.Append("TranslationHelper.reverseList(");
                this.TranslateExpression(sb, list);
                sb.Append(')');
            }
        }

        public override void TranslateListSet(StringBuilder sb, Expression list, Expression index, Expression value)
        {
            if (list.ResolvedType.Generics[0].RootValue == "Value")
            {
                this.TranslateExpression(sb, list);
                sb.Append(".items[");
                this.TranslateExpression(sb, index);
                sb.Append("] = ");
                this.TranslateExpression(sb, value);
            }
            else
            {
                this.TranslateExpression(sb, list);
                sb.Append(".set(");
                this.TranslateExpression(sb, index);
                sb.Append(", ");
                this.TranslateExpression(sb, value);
                sb.Append(')');
            }
        }

        public override void TranslateListShuffle(StringBuilder sb, Expression list)
        {
            sb.Append("TranslationHelper.shuffleInPlace(");
            this.TranslateExpression(sb, list);
            sb.Append(')');
        }

        public override void TranslateListSize(StringBuilder sb, Expression list)
        {
            this.TranslateExpression(sb, list);

            if (list.ResolvedType.Generics[0].RootValue == "Value")
            {
                sb.Append(".length");
            }
            else
            {
                sb.Append(".size()");
            }
        }

        public override void TranslateListToArray(StringBuilder sb, Expression list)
        {
            PType itemType = list.ResolvedType.Generics[0];
            if (itemType.RootValue == "object")
            {
                this.TranslateExpression(sb, list);
                sb.Append(".toArray()");
                return;
            }

            string rootType = itemType.RootValue;
            switch (itemType.RootValue)
            {
                case "bool":
                case "byte":
                case "int":
                case "double":
                case "char":
                    sb.Append("TranslationHelper.listToArray");
                    sb.Append((char)(rootType[0] + 'A' - 'a'));
                    sb.Append(rootType.Substring(1));
                    sb.Append('(');
                    this.TranslateExpression(sb, list);
                    sb.Append(')');
                    break;

                case "string":
                    this.TranslateExpression(sb, list);
                    sb.Append(".toArray(TranslationHelper.EMPTY_ARRAY_STRING)");
                    break;
                case "Value":
                    this.TranslateExpression(sb, list);
                    sb.Append(".toArray()");
                    break;
                case "List":
                    this.TranslateExpression(sb, list);
                    sb.Append(".toArray(TranslationHelper.EMPTY_ARRAY_LIST)");
                    break;
                case "Dictionary":
                    this.TranslateExpression(sb, list);
                    sb.Append(".toArray(TranslationHelper.EMPTY_ARRAY_MAP)");
                    break;
                case "Array":
                    throw NYI.JavaListOfArraysConvertToArray();
                default:
                    string javaType = this.Platform.TranslateType(itemType);
                    char firstChar = javaType[0];
                    if (firstChar >= 'A' && firstChar <= 'Z')
                    {
                        this.TranslateExpression(sb, list);
                        sb.Append(".toArray((");
                        sb.Append(javaType);
                        sb.Append("[]) TranslationHelper.EMPTY_ARRAY_OBJECT)");
                    }
                    else
                    {
                        // I think I covered all the primitive types that are supported.
                        throw new NotImplementedException();
                    }
                    break;
            }
        }

        public override void TranslateMathArcCos(StringBuilder sb, Expression ratio)
        {
            sb.Append("Math.acos(");
            this.TranslateExpression(sb, ratio);
            sb.Append(')');
        }

        public override void TranslateMathArcSin(StringBuilder sb, Expression ratio)
        {
            sb.Append("Math.asin(");
            this.TranslateExpression(sb, ratio);
            sb.Append(')');
        }

        public override void TranslateMathArcTan(StringBuilder sb, Expression yComponent, Expression xComponent)
        {
            sb.Append("Math.atan2(");
            this.TranslateExpression(sb, yComponent);
            sb.Append(", ");
            this.TranslateExpression(sb, xComponent);
            sb.Append(')');
        }

        public override void TranslateMathCos(StringBuilder sb, Expression thetaRadians)
        {
            sb.Append("Math.cos(");
            this.TranslateExpression(sb, thetaRadians);
            sb.Append(')');
        }

        public override void TranslateMathLog(StringBuilder sb, Expression value)
        {
            sb.Append("Math.log(");
            this.TranslateExpression(sb, value);
            sb.Append(')');
        }

        public override void TranslateMathPow(StringBuilder sb, Expression expBase, Expression exponent)
        {
            sb.Append("Math.pow(");
            this.TranslateExpression(sb, expBase);
            sb.Append(", ");
            this.TranslateExpression(sb, exponent);
            sb.Append(')');
        }

        public override void TranslateMathSin(StringBuilder sb, Expression thetaRadians)
        {
            sb.Append("Math.sin(");
            this.TranslateExpression(sb, thetaRadians);
            sb.Append(')');
        }

        public override void TranslateMathTan(StringBuilder sb, Expression thetaRadians)
        {
            sb.Append("Math.tan(");
            this.TranslateExpression(sb, thetaRadians);
            sb.Append(')');
        }

        public override void TranslateMultiplyList(StringBuilder sb, Expression list, Expression n)
        {
            if (list.ResolvedType.Generics[0].RootValue == "Value")
            {
                this.TranslateExpression(sb, list);
                sb.Append(".multiply(");
                this.TranslateExpression(sb, n);
                sb.Append(')');
            }
            else
            {
                sb.Append("TranslationHelper.multiplyList(");
                this.TranslateExpression(sb, list);
                sb.Append(", ");
                this.TranslateExpression(sb, n);
                sb.Append(')');
            }
        }

        public override void TranslateNullConstant(StringBuilder sb)
        {
            sb.Append("null");
        }

        /*
        public override void TranslateOpChain(StringBuilder sb, OpChain opChain)
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

        public override void TranslateOrd(StringBuilder sb, Expression charValue)
        {
            throw new NotImplementedException();
        }

        public override void TranslateParseFloatUnsafe(StringBuilder sb, Expression stringValue)
        {
            sb.Append("Double.parseDouble(");
            this.TranslateExpression(sb, stringValue);
            sb.Append(')');
        }

        public override void TranslateParseInt(StringBuilder sb, Expression safeStringValue)
        {
            sb.Append("Integer.parseInt(");
            this.TranslateExpression(sb, safeStringValue);
            sb.Append(')');
        }

        public override void TranslateRandomFloat(StringBuilder sb)
        {
            sb.Append("TranslationHelper.random.nextDouble()");
        }

        public override void TranslateRegisterLibraryFunction(StringBuilder sb, Expression libRegObj, Expression functionName, Expression functionArgCount)
        {
            Platform.LibraryForExport lfe = this.CurrentLibraryFunctionTranslator.Library;
            string functionNameString = ((InlineConstant)functionName).Value.ToString();
            string className = "FP_lib_" + lfe.Name.ToLower() + "_function_" + functionNameString;

            sb.Append("TranslationHelper.registerLibraryFunction(LibraryWrapper.class, ");
            this.TranslateExpression(sb, libRegObj);
            sb.Append(", new ");
            sb.Append(className);
            sb.Append("(), ");
            this.TranslateExpression(sb, functionName);
            sb.Append(", ");
            this.TranslateExpression(sb, functionArgCount);
            sb.Append(')');
        }

        public override void TranslateResourceReadTextFile(StringBuilder sb, Expression path)
        {
            sb.Append("ResourceReader.readFileText(\"resources/text/\" + ");
            this.TranslateExpression(sb, path);
            sb.Append(')');
        }

        public override void TranslateSetProgramData(StringBuilder sb, Expression programData)
        {
            sb.Append("TranslationHelper.setProgramData(");
            this.TranslateExpression(sb, programData);
            sb.Append(')');
        }

        public override void TranslateSortedCopyOfIntArray(StringBuilder sb, Expression intArray)
        {
            sb.Append("TranslationHelper.sortedCopyOfIntArray(");
            this.TranslateExpression(sb, intArray);
            sb.Append(')');
        }

        public override void TranslateSortedCopyOfStringArray(StringBuilder sb, Expression stringArray)
        {
            sb.Append("TranslationHelper.sortedCopyOfStringArray(");
            this.TranslateExpression(sb, stringArray);
            sb.Append(')');
        }

        public override void TranslateStringAppend(StringBuilder sb, Expression str1, Expression str2)
        {
            this.TranslateExpression(sb, str1);
            sb.Append(" += ");
            this.TranslateExpression(sb, str2);
        }

        public override void TranslateStringBuffer16(StringBuilder sb)
        {
            sb.Append("TranslationHelper.STRING_BUFFER_16");
        }

        public override void TranslateStringCharAt(StringBuilder sb, Expression str, Expression index)
        {
            this.TranslateExpression(sb, str);
            sb.Append(".charAt(");
            this.TranslateExpression(sb, index);
            sb.Append(')');
        }

        public override void TranslateStringCharCodeAt(StringBuilder sb, Expression str, Expression index)
        {
            sb.Append("((int) ");
            this.TranslateExpression(sb, str);
            sb.Append(".charAt(");
            this.TranslateExpression(sb, index);
            sb.Append("))");
        }

        public override void TranslateStringCompareIsReverse(StringBuilder sb, Expression str1, Expression str2)
        {
            sb.Append('(');
            this.TranslateExpression(sb, str1);
            sb.Append(".compareTo(");
            this.TranslateExpression(sb, str2);
            sb.Append(") > 0)");
        }

        public override void TranslateStringConcatAll(StringBuilder sb, Expression[] strings)
        {
            this.TranslateExpression(sb, strings[0]);
            for (int i = 1; i < strings.Length; ++i)
            {
                sb.Append(" + ");
                this.TranslateExpression(sb, strings[i]);
            }
        }

        public override void TranslateStringConcatPair(StringBuilder sb, Expression strLeft, Expression strRight)
        {
            this.TranslateExpression(sb, strLeft);
            sb.Append(" + ");
            this.TranslateExpression(sb, strRight);
        }

        public override void TranslateStringContains(StringBuilder sb, Expression haystack, Expression needle)
        {
            this.TranslateExpression(sb, haystack);
            sb.Append(".contains(");
            this.TranslateExpression(sb, needle);
            sb.Append(')');
        }

        public override void TranslateStringEndsWith(StringBuilder sb, Expression haystack, Expression needle)
        {
            this.TranslateExpression(sb, haystack);
            sb.Append(".endsWith(");
            this.TranslateExpression(sb, needle);
            sb.Append(')');
        }

        public override void TranslateStringEquals(StringBuilder sb, Expression left, Expression right)
        {
            this.TranslateExpression(sb, left);
            sb.Append(".equals(");
            this.TranslateExpression(sb, right);
            sb.Append(')');
        }

        public override void TranslateStringFromCharCode(StringBuilder sb, Expression charCode)
        {
            sb.Append("Character.toString((char) ");
            this.TranslateExpression(sb, charCode);
            sb.Append(")");
        }

        public override void TranslateStringIndexOf(StringBuilder sb, Expression haystack, Expression needle)
        {
            this.TranslateExpression(sb, haystack);
            sb.Append(".indexOf(");
            this.TranslateExpression(sb, needle);
            sb.Append(')');
        }

        public override void TranslateStringIndexOfWithStart(StringBuilder sb, Expression haystack, Expression needle, Expression startIndex)
        {
            this.TranslateExpression(sb, haystack);
            sb.Append(".indexOf(");
            this.TranslateExpression(sb, needle);
            sb.Append(", ");
            this.TranslateExpression(sb, startIndex);
            sb.Append(')');
        }

        public override void TranslateStringLength(StringBuilder sb, Expression str)
        {
            this.TranslateExpression(sb, str);
            sb.Append(".length()");
        }

        public override void TranslateStringReplace(StringBuilder sb, Expression haystack, Expression needle, Expression newNeedle)
        {
            this.TranslateExpression(sb, haystack);
            sb.Append(".replace((CharSequence) ");
            this.TranslateExpression(sb, needle);
            sb.Append(", (CharSequence) ");
            this.TranslateExpression(sb, newNeedle);
            sb.Append(')');
        }

        public override void TranslateStringReverse(StringBuilder sb, Expression str)
        {
            sb.Append("TranslationHelper.reverseString(");
            this.TranslateExpression(sb, str);
            sb.Append(')');
        }

        public override void TranslateStringSplit(StringBuilder sb, Expression haystack, Expression needle)
        {
            sb.Append("TranslationHelper.literalStringSplit(");
            this.TranslateExpression(sb, haystack);
            sb.Append(", ");
            this.TranslateExpression(sb, needle);
            sb.Append(')');
        }

        public override void TranslateStringStartsWith(StringBuilder sb, Expression haystack, Expression needle)
        {
            this.TranslateExpression(sb, haystack);
            sb.Append(".startsWith(");
            this.TranslateExpression(sb, needle);
            sb.Append(')');
        }

        public override void TranslateStringSubstring(StringBuilder sb, Expression str, Expression start, Expression length)
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

        public override void TranslateStringSubstringIsEqualTo(StringBuilder sb, Expression haystack, Expression startIndex, Expression needle)
        {
            throw new NotImplementedException();
        }

        public override void TranslateStringToLower(StringBuilder sb, Expression str)
        {
            this.TranslateExpression(sb, str);
            sb.Append(".toLowerCase()");
        }

        public override void TranslateStringToUpper(StringBuilder sb, Expression str)
        {
            this.TranslateExpression(sb, str);
            sb.Append(".toUpperCase()");
        }

        public override void TranslateStringTrim(StringBuilder sb, Expression str)
        {
            this.TranslateExpression(sb, str);
            sb.Append(".trim()");
        }

        public override void TranslateStringTrimEnd(StringBuilder sb, Expression str)
        {
            sb.Append("TranslationHelper.trimSide(");
            this.TranslateExpression(sb, str);
            sb.Append(", false)");
        }

        public override void TranslateStringTrimStart(StringBuilder sb, Expression str)
        {
            sb.Append("TranslationHelper.trimSide(");
            this.TranslateExpression(sb, str);
            sb.Append(", true)");
        }

        public override void TranslateStrongReferenceEquality(StringBuilder sb, Expression left, Expression right)
        {
            throw new NotImplementedException();
        }

        public override void TranslateStructFieldDereference(StringBuilder sb, Expression root, StructDefinition structDef, string fieldName, int fieldIndex)
        {
            this.TranslateExpression(sb, root);
            sb.Append('.');
            sb.Append(fieldName);
        }

        public override void TranslateThreadSleep(StringBuilder sb, Expression seconds)
        {
            sb.Append("TranslationHelper.sleep(");
            this.TranslateExpression(sb, seconds);
            sb.Append(')');
        }

        public override void TranslateTryParseFloat(StringBuilder sb, Expression stringValue, Expression floatOutList)
        {
            sb.Append("TranslationHelper.parseFloatOrReturnNull(");
            this.TranslateExpression(sb, floatOutList);
            sb.Append(", ");
            this.TranslateExpression(sb, stringValue);
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
            sb.Append("TranslationHelper.checkLibraryAvaialability(");
            this.TranslateExpression(sb, libraryName);
            sb.Append(", ");
            this.TranslateExpression(sb, libraryVersion);
            sb.Append(')');
        }

        public override void TranslateVmEnqueueResume(StringBuilder sb, Expression seconds, Expression executionContextId)
        {
            throw new NotImplementedException();
        }

        public override void TranslateVmRunLibraryManifest(StringBuilder sb, Expression libraryName, Expression libRegObj)
        {
            sb.Append("TranslationHelper.runLibraryManifest(");
            this.TranslateExpression(sb, libraryName);
            sb.Append(", ");
            this.TranslateExpression(sb, libRegObj);
            sb.Append(')');
        }
    }
}
