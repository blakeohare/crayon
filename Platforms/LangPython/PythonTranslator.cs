using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pastel.Nodes;

namespace LangPython
{
    public abstract class PythonTranslator : Platform.AbstractTranslator
    {
        private string TranslateOp(string originalOp)
        {
            switch (originalOp)
            {
                case "&&": return "and";
                case "||": return "or";
                default: return originalOp;
            }
        }


        // This is a hack for conveying extra information to the top-level function serializer for switch statement stuff.
        // This reference is updated in TranslateFunctionDefinition.
        private FunctionDefinition currentFunctionDefinition = null;
        private int switchCounter = 0;
        internal FunctionDefinition CurrentFunctionDefinition
        {
            get { return this.currentFunctionDefinition; }
            set
            {
                this.currentFunctionDefinition = value;
                this.switchCounter = 0;
            }
        }
        public List<PythonFakeSwitchStatement> SwitchStatements { get; private set; }

        public PythonTranslator(Platform.AbstractPlatform platform) : base(platform, "  ", "\n")
        {
            this.SwitchStatements = new List<PythonFakeSwitchStatement>();
        }

        public override void TranslateArrayGet(StringBuilder sb, Expression array, Expression index)
        {
            this.TranslateExpression(sb, array);
            sb.Append('[');
            this.TranslateExpression(sb, index);
            sb.Append(']');
        }

        public override void TranslateArrayLength(StringBuilder sb, Expression array)
        {
            sb.Append("len(");
            this.TranslateExpression(sb, array);
            sb.Append(')');
        }

        public override void TranslateArrayNew(StringBuilder sb, PType arrayType, Expression lengthExpression)
        {
            if (lengthExpression is InlineConstant)
            {
                InlineConstant ic = (InlineConstant)lengthExpression;
                int length = (int)ic.Value;
                switch (length)
                {
                    case 0: sb.Append("[]"); return;
                    case 1: sb.Append("[None]"); return;
                    case 2: sb.Append("[None, None]"); return;
                    default: break;
                }
            }
            // TODO: use a global constant that's just a list of [None] to re-use here rather than allocating a dummy list each time.
            sb.Append("([None] * ");
            this.TranslateExpression(sb, lengthExpression);
            sb.Append(")");
        }

        public override void TranslateArraySet(StringBuilder sb, Expression array, Expression index, Expression value)
        {
            this.TranslateExpression(sb, array);
            sb.Append('[');
            this.TranslateExpression(sb, index);
            sb.Append("] = ");
            this.TranslateExpression(sb, value);
        }

        public override void TranslateAssignment(StringBuilder sb, Assignment assignment)
        {
            sb.Append(this.CurrentTab);
            this.TranslateExpression(sb, assignment.Target);
            sb.Append(' ');
            sb.Append(assignment.OpToken.Value);
            sb.Append(' ');
            this.TranslateExpression(sb, assignment.Value);
            sb.Append(this.NewLine);
        }

        public override void TranslateBooleanConstant(StringBuilder sb, bool value)
        {
            sb.Append(value ? "True" : "False");
        }

        public override void TranslateBooleanNot(StringBuilder sb, UnaryOp unaryOp)
        {
            sb.Append("not (");
            this.TranslateExpression(sb, unaryOp.Expression);
            sb.Append(')');
        }

        public override void TranslateBreak(StringBuilder sb)
        {
            throw new NotImplementedException();
        }

        public override void TranslateCast(StringBuilder sb, PType type, Expression expression)
        {
            this.TranslateExpression(sb, expression);
        }

        public override void TranslateCharConstant(StringBuilder sb, char value)
        {
            sb.Append(Common.Util.ConvertStringValueToCode(value.ToString()));
        }

        public override void TranslateCharToString(StringBuilder sb, Expression charValue)
        {
            this.TranslateExpression(sb, charValue);
        }

        public override void TranslateChr(StringBuilder sb, Expression charCode)
        {
            sb.Append("chr(");
            this.TranslateExpression(sb, charCode);
            sb.Append(')');
        }

        public override void TranslateCommandLineArgs(StringBuilder sb)
        {
            sb.Append("TranslationHelper_getCommandLineArgs()");
        }

        public override void TranslateConstructorInvocation(StringBuilder sb, ConstructorInvocation constructorInvocation)
        {
            StructDefinition structDef = constructorInvocation.StructType;
            if (structDef == null) throw new NotImplementedException();
            sb.Append('[');
            int args = structDef.ArgNames.Length;
            for (int i = 0; i < args; ++i)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }
                this.TranslateExpression(sb, constructorInvocation.Args[i]);
            }
            sb.Append(']');
        }

        public override void TranslateConvertRawDictionaryValueCollectionToAReusableValueList(StringBuilder sb, Expression dictionary)
        {
            sb.Append("list(");
            this.TranslateExpression(sb, dictionary);
            sb.Append(")");
        }

        public override void TranslateCurrentTimeSeconds(StringBuilder sb)
        {
            sb.Append("time.time()");
        }

        public override void TranslateDictionaryContainsKey(StringBuilder sb, Expression dictionary, Expression key)
        {
            sb.Append('(');
            this.TranslateExpression(sb, key);
            sb.Append(" in ");
            this.TranslateExpression(sb, dictionary);
            sb.Append(')');
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
            sb.Append("list(");
            this.TranslateExpression(sb, dictionary);
            sb.Append(".keys())");
        }

        public override void TranslateDictionaryKeysToValueList(StringBuilder sb, Expression dictionary)
        {
            throw new NotImplementedException();
        }

        public override void TranslateDictionaryNew(StringBuilder sb, PType keyType, PType valueType)
        {
            sb.Append("{}");
        }

        public override void TranslateDictionaryRemove(StringBuilder sb, Expression dictionary, Expression key)
        {
            this.TranslateExpression(sb, dictionary);
            sb.Append(".pop(");
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
            sb.Append("len(");
            this.TranslateExpression(sb, dictionary);
            sb.Append(')');
        }

        public override void TranslateDictionaryValues(StringBuilder sb, Expression dictionary)
        {
            sb.Append("list(");
            this.TranslateExpression(sb, dictionary);
            sb.Append(".values())");
        }

        public override void TranslateDictionaryValuesToValueList(StringBuilder sb, Expression dictionary)
        {
            throw new NotImplementedException();
        }

        public override void TranslateEmitComment(StringBuilder sb, string value)
        {
            sb.Append("# ");
            sb.Append(value);
        }

        public override void TranslateExecutables(StringBuilder sb, Executable[] executables)
        {
            if (executables.Length == 0)
            {
                sb.Append(this.CurrentTab);
                sb.Append("pass");
                sb.Append(this.NewLine);
            }
            else
            {
                base.TranslateExecutables(sb, executables);
            }
        }

        public override void TranslateExpressionAsExecutable(StringBuilder sb, Expression expression)
        {
            sb.Append(this.CurrentTab);
            this.TranslateExpression(sb, expression);
            sb.Append(this.NewLine);
        }

        public override void TranslateFloatBuffer16(StringBuilder sb)
        {
            sb.Append("TranslationHelper_FloatBuffer16");
        }

        public override void TranslateFloatConstant(StringBuilder sb, double value)
        {
            sb.Append(Common.Util.FloatToString(value));
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
            sb.Append("int(");
            this.TranslateExpression(sb, floatExpr);
            sb.Append(")");
        }

        public override void TranslateFloatToString(StringBuilder sb, Expression floatExpr)
        {
            sb.Append("str(");
            this.TranslateExpression(sb, floatExpr);
            sb.Append(')');
        }

        public override void TranslateForceParens(StringBuilder sb, Expression expression)
        {
            sb.Append('(');
            this.TranslateExpression(sb, expression);
            sb.Append(')');
        }

        public override void TranslateFunctionInvocation(StringBuilder sb, FunctionInvocation funcInvocation)
        {
            this.TranslateExpression(sb, funcInvocation.Root);
            sb.Append('(');
            Expression[] args = funcInvocation.Args;
            int argCount = args.Length;
            for (int i = 0; i < argCount; ++i)
            {
                if (i > 0) sb.Append(", ");
                this.TranslateExpression(sb, args[i]);
            }
            sb.Append(')');
        }

        public override void TranslateFunctionReference(StringBuilder sb, FunctionReference funcRef)
        {
            sb.Append("v_");
            sb.Append(funcRef.Function.NameToken.Value);
        }

        public override void TranslateGetProgramData(StringBuilder sb)
        {
            sb.Append("TranslationHelper_getProgramData()");
        }

        public override void TranslateGetResourceManifest(StringBuilder sb)
        {
            sb.Append("TranslationHelper_getResourceManifest()");
        }

        public override void TranslateGlobalVariable(StringBuilder sb, Variable variable)
        {
            // no special syntax
            this.TranslateVariable(sb, variable);
        }

        public override void TranslateIfStatement(StringBuilder sb, IfStatement ifStatement)
        {
            sb.Append(this.CurrentTab);
            this.TranslateIfStatementNoIndent(sb, ifStatement);
        }

        private void TranslateIfStatementNoIndent(StringBuilder sb, IfStatement ifStatement)
        {
            sb.Append("if ");
            this.TranslateExpression(sb, ifStatement.Condition);
            sb.Append(':');
            sb.Append(this.NewLine);
            this.TabDepth++;
            if (ifStatement.IfCode.Length == 0)
            {
                // ideally this should be optimized out at compile-time. TODO: throw instead and do that
                sb.Append(this.CurrentTab);
                sb.Append("pass");
                sb.Append(this.NewLine);
            }
            else
            {
                this.TranslateExecutables(sb, ifStatement.IfCode);
            }
            this.TabDepth--;

            Executable[] elseCode = ifStatement.ElseCode;

            if (elseCode.Length == 0) return;

            if (elseCode.Length == 1 && elseCode[0] is IfStatement)
            {
                sb.Append(this.CurrentTab);
                sb.Append("el");
                this.TranslateIfStatementNoIndent(sb, (IfStatement)elseCode[0]);
            }
            else
            {
                sb.Append(this.CurrentTab);
                sb.Append("else:");
                sb.Append(this.NewLine);
                this.TabDepth++;
                this.TranslateExecutables(sb, elseCode);
                this.TabDepth--;
            }
        }

        public override void TranslateIntBuffer16(StringBuilder sb)
        {
            sb.Append("TranslationHelper_IntBuffer16");
        }

        public override void TranslateIntegerConstant(StringBuilder sb, int value)
        {
            sb.Append(value);
        }

        public override void TranslateIntegerDivision(StringBuilder sb, Expression integerNumerator, Expression integerDenominator)
        {
            sb.Append('(');
            this.TranslateExpression(sb, integerNumerator);
            sb.Append(") // (");
            this.TranslateExpression(sb, integerDenominator);
            sb.Append(')');
        }

        public override void TranslateIntToString(StringBuilder sb, Expression integer)
        {
            sb.Append("str(");
            this.TranslateExpression(sb, integer);
            sb.Append(')');
        }

        public override void TranslateInvokeDynamicLibraryFunction(StringBuilder sb, Expression functionId, Expression argsArray)
        {
            this.TranslateExpression(sb, functionId);
            sb.Append('(');
            this.TranslateExpression(sb, argsArray);
            sb.Append(')');
        }

        public override void TranslateIsValidInteger(StringBuilder sb, Expression stringValue)
        {
            sb.Append("TranslationHelper_isValidInteger(");
            this.TranslateExpression(sb, stringValue);
            sb.Append(')');
        }

        public override void TranslateListAdd(StringBuilder sb, Expression list, Expression item)
        {
            this.TranslateExpression(sb, list);
            sb.Append(".append(");
            this.TranslateExpression(sb, item);
            sb.Append(')');
        }

        public override void TranslateListClear(StringBuilder sb, Expression list)
        {
            sb.Append("del ");
            this.TranslateExpression(sb, list);
            sb.Append("[:]");
        }

        public override void TranslateListConcat(StringBuilder sb, Expression list, Expression items)
        {
            this.TranslateExpression(sb, list);
            sb.Append(" + ");
            this.TranslateExpression(sb, items);
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
            sb.Append(".insert(");
            this.TranslateExpression(sb, index);
            sb.Append(", ");
            this.TranslateExpression(sb, item);
            sb.Append(')');
        }

        public override void TranslateListJoinChars(StringBuilder sb, Expression list)
        {
            sb.Append("''.join(");
            this.TranslateExpression(sb, list);
            sb.Append(')');
        }

        public override void TranslateListJoinStrings(StringBuilder sb, Expression list, Expression sep)
        {
            this.TranslateExpression(sb, sep);
            sb.Append(".join(");
            this.TranslateExpression(sb, list);
            sb.Append(')');
        }

        public override void TranslateListNew(StringBuilder sb, PType type)
        {
            sb.Append("[]");
        }

        public override void TranslateListPop(StringBuilder sb, Expression list)
        {
            this.TranslateExpression(sb, list);
            sb.Append(".pop()");
        }

        public override void TranslateListRemoveAt(StringBuilder sb, Expression list, Expression index)
        {
            sb.Append("del ");
            this.TranslateExpression(sb, list);
            sb.Append('[');
            this.TranslateExpression(sb, index);
            sb.Append(']');
        }

        public override void TranslateListReverse(StringBuilder sb, Expression list)
        {
            this.TranslateExpression(sb, list);
            sb.Append(".reverse()");
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
            sb.Append("random.shuffle(");
            this.TranslateExpression(sb, list);
            sb.Append(')');
        }

        public override void TranslateListSize(StringBuilder sb, Expression list)
        {
            sb.Append("len(");
            this.TranslateExpression(sb, list);
            sb.Append(')');
        }

        public override void TranslateListToArray(StringBuilder sb, Expression list)
        {
            this.TranslateExpression(sb, list);
            sb.Append("[:]");
        }

        public override void TranslateMathArcCos(StringBuilder sb, Expression ratio)
        {
            sb.Append("math.acos(");
            this.TranslateExpression(sb, ratio);
            sb.Append(')');
        }

        public override void TranslateMathArcSin(StringBuilder sb, Expression ratio)
        {
            sb.Append("math.asin(");
            this.TranslateExpression(sb, ratio);
            sb.Append(')');
        }

        public override void TranslateMathArcTan(StringBuilder sb, Expression yComponent, Expression xComponent)
        {
            sb.Append("math.atan2(");
            this.TranslateExpression(sb, yComponent);
            sb.Append(", ");
            this.TranslateExpression(sb, xComponent);
            sb.Append(')');
        }

        public override void TranslateMathCos(StringBuilder sb, Expression thetaRadians)
        {
            sb.Append("math.cos(");
            this.TranslateExpression(sb, thetaRadians);
            sb.Append(')');
        }

        public override void TranslateMathLog(StringBuilder sb, Expression value)
        {
            sb.Append("math.log(");
            this.TranslateExpression(sb, value);
            sb.Append(')');
        }

        public override void TranslateMathPow(StringBuilder sb, Expression expBase, Expression exponent)
        {
            sb.Append('(');
            this.TranslateExpression(sb, expBase);
            sb.Append(" ** ");
            this.TranslateExpression(sb, exponent);
            sb.Append(')');
        }

        public override void TranslateMathSin(StringBuilder sb, Expression thetaRadians)
        {
            sb.Append("math.sin(");
            this.TranslateExpression(sb, thetaRadians);
            sb.Append(')');
        }

        public override void TranslateMathTan(StringBuilder sb, Expression thetaRadians)
        {
            sb.Append("math.tan(");
            this.TranslateExpression(sb, thetaRadians);
            sb.Append(')');
        }

        public override void TranslateMultiplyList(StringBuilder sb, Expression list, Expression n)
        {
            sb.Append('(');
            this.TranslateExpression(sb, list);
            sb.Append(" * (");
            this.TranslateExpression(sb, n);
            sb.Append("))");
        }

        public override void TranslateNegative(StringBuilder sb, UnaryOp unaryOp)
        {
            Expression expr = unaryOp.Expression;
            if (expr is InlineConstant || expr is Variable)
            {
                sb.Append('-');
                this.TranslateExpression(sb, expr);
            }
            else
            {
                sb.Append("-(");
                this.TranslateExpression(sb, expr);
                sb.Append(')');
            }
        }

        public override void TranslateNullConstant(StringBuilder sb)
        {
            sb.Append("None");
        }

        public override void TranslateOpChain(StringBuilder sb, OpChain opChain)
        {
            sb.Append('(');
            Expression[] expressions = opChain.Expressions;
            Pastel.Token[] ops = opChain.Ops;
            for (int i = 0; i < expressions.Length; ++i)
            {
                if (i > 0)
                {
                    // TODO: platform should have an op translator, which would just be a pass-through function for most ops.
                    sb.Append(' ');
                    sb.Append(this.TranslateOp(ops[i - 1].Value));
                    sb.Append(' ');
                }
                this.TranslateExpression(sb, expressions[i]);
            }
            sb.Append(')');
        }

        public override void TranslateParseFloat(StringBuilder sb, Expression stringValue, Expression floatOutList)
        {
            sb.Append("TranslationHelper_tryParseFloat(");
            this.TranslateExpression(sb, stringValue);
            sb.Append(", ");
            this.TranslateExpression(sb, floatOutList);
            sb.Append(')');
        }

        public override void TranslateParseFloatREDUNDANT(StringBuilder sb, Expression stringValue)
        {
            sb.Append("float(");
            this.TranslateExpression(sb, stringValue);
            sb.Append(")");
        }

        public override void TranslateParseInt(StringBuilder sb, Expression safeStringValue)
        {
            sb.Append("int(");
            this.TranslateExpression(sb, safeStringValue);
            sb.Append(')');
        }

        public override void TranslatePrintStdErr(StringBuilder sb, Expression value)
        {
            sb.Append("print(");
            this.TranslateExpression(sb, value);
            sb.Append(')');
        }

        public override void TranslatePrintStdOut(StringBuilder sb, Expression value)
        {
            sb.Append("print(");
            this.TranslateExpression(sb, value);
            sb.Append(')');
        }

        public override void TranslateRandomFloat(StringBuilder sb)
        {
            sb.Append("random.random()");
        }

        public override void TranslateReadByteCodeFile(StringBuilder sb)
        {
            sb.Append("TranslationHelper_getByteCode()");
        }

        public override void TranslateRegisterLibraryFunction(StringBuilder sb, Expression libRegObj, Expression functionName, Expression functionArgCount)
        {
            sb.Append("TranslationHelper_registerLibraryFunction(_moduleInfo, ");
            this.TranslateExpression(sb, libRegObj);
            sb.Append(", ");
            this.TranslateExpression(sb, functionName);
            sb.Append(", ");
            this.TranslateExpression(sb, functionArgCount);
            sb.Append(')');
        }

        public override void TranslateResourceReadTextFile(StringBuilder sb, Expression path)
        {
            sb.Append("ResourceReader_readTextFile('res/text/' + ");
            this.TranslateExpression(sb, path);
            sb.Append(')');
        }

        public override void TranslateReturnStatemnt(StringBuilder sb, ReturnStatement returnStatement)
        {
            sb.Append(this.CurrentTab);
            sb.Append("return ");
            this.TranslateExpression(sb, returnStatement.Expression);
            sb.Append(this.NewLine);
        }

        public override void TranslateSetProgramData(StringBuilder sb, Expression programData)
        {
            sb.Append("TranslationHelper_setProgramData(");
            this.TranslateExpression(sb, programData);
            sb.Append(')');
        }

        public override void TranslateSortedCopyOfIntArray(StringBuilder sb, Expression intArray)
        {
            sb.Append("TranslationHelper_sortedCopyOfList(");
            this.TranslateExpression(sb, intArray);
            sb.Append(')');
        }

        public override void TranslateSortedCopyOfStringArray(StringBuilder sb, Expression stringArray)
        {
            sb.Append("TranslationHelper_sortedCopyOfList(");
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
            sb.Append("TranslationHelper_StringBuffer16");
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
            sb.Append("ord(");
            this.TranslateExpression(sb, str);
            sb.Append('[');
            this.TranslateExpression(sb, index);
            sb.Append("])");
        }

        public override void TranslateStringCompareIsReverse(StringBuilder sb, Expression str1, Expression str2)
        {
            sb.Append('(');
            this.TranslateExpression(sb, str1);
            sb.Append(" > ");
            this.TranslateExpression(sb, str2);
            sb.Append(')');
        }

        public override void TranslateStringConcatAll(StringBuilder sb, Expression[] strings)
        {
            if (strings.Length == 2)
            {
                this.TranslateExpression(sb, strings[0]);
                sb.Append(" + ");
                this.TranslateExpression(sb, strings[1]);
            }
            else
            {
                sb.Append("''.join([");
                for (int i = 0; i < strings.Length; ++i)
                {
                    if (i > 0) sb.Append(", ");
                    this.TranslateExpression(sb, strings[i]);
                }
                sb.Append("])");
            }
        }

        public override void TranslateStringConstant(StringBuilder sb, string value)
        {
            sb.Append(Common.Util.ConvertStringValueToCode(value));
        }

        public override void TranslateStringContains(StringBuilder sb, Expression haystack, Expression needle)
        {
            sb.Append('(');
            this.TranslateExpression(sb, needle);
            sb.Append(" in ");
            this.TranslateExpression(sb, haystack);
            sb.Append(')');
        }

        public override void TranslateStringEndsWith(StringBuilder sb, Expression haystack, Expression needle)
        {
            this.TranslateExpression(sb, haystack);
            sb.Append(".endswith(");
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
            sb.Append("chr(");
            this.TranslateExpression(sb, charCode);
            sb.Append(')');
        }

        public override void TranslateStringIndexOf(StringBuilder sb, Expression haystack, Expression needle)
        {
            this.TranslateExpression(sb, haystack);
            sb.Append(".find(");
            this.TranslateExpression(sb, needle);
            sb.Append(')');
        }

        public override void TranslateStringLength(StringBuilder sb, Expression str)
        {
            sb.Append("len(");
            this.TranslateExpression(sb, str);
            sb.Append(')');
        }

        public override void TranslateStringReplace(StringBuilder sb, Expression haystack, Expression needle, Expression newNeedle)
        {
            this.TranslateExpression(sb, haystack);
            sb.Append(".replace(");
            this.TranslateExpression(sb, needle);
            sb.Append(", ");
            this.TranslateExpression(sb, newNeedle);
            sb.Append(')');
        }

        public override void TranslateStringReverse(StringBuilder sb, Expression str)
        {
            this.TranslateExpression(sb, str);
            sb.Append("[::-1]");
        }

        public override void TranslateStringSplit(StringBuilder sb, Expression haystack, Expression needle)
        {
            this.TranslateExpression(sb, haystack);
            sb.Append(".split(");
            this.TranslateExpression(sb, needle);
            sb.Append(')');
        }

        public override void TranslateStringStartsWith(StringBuilder sb, Expression haystack, Expression needle)
        {
            this.TranslateExpression(sb, haystack);
            sb.Append(".startswith(");
            this.TranslateExpression(sb, needle);
            sb.Append(')');
        }

        public override void TranslateStringToLower(StringBuilder sb, Expression str)
        {
            this.TranslateExpression(sb, str);
            sb.Append(".lower()");
        }

        public override void TranslateStringToUpper(StringBuilder sb, Expression str)
        {
            this.TranslateExpression(sb, str);
            sb.Append(".upper()");
        }

        public override void TranslateStringTrim(StringBuilder sb, Expression str)
        {
            this.TranslateExpression(sb, str);
            sb.Append(".strip()");
        }

        public override void TranslateStringTrimEnd(StringBuilder sb, Expression str)
        {
            this.TranslateExpression(sb, str);
            sb.Append(".rstrip()");
        }

        public override void TranslateStringTrimStart(StringBuilder sb, Expression str)
        {
            this.TranslateExpression(sb, str);
            sb.Append(".lstrip()");
        }

        public override void TranslateStrongReferenceEquality(StringBuilder sb, Expression left, Expression right)
        {
            this.TranslateExpression(sb, left);
            sb.Append(" is ");
            this.TranslateExpression(sb, right);
        }

        // TODO: fix typo: missing an e at the end of the name
        public override void TranslateStructFieldDereference(StringBuilder sb, Expression root, StructDefinition structDef, string fieldName, int fieldIndex)
        {
            this.TranslateExpression(sb, root);
            sb.Append('[');
            sb.Append(fieldIndex);
            sb.Append(']');
        }

        public override void TranslateSwitchStatement(StringBuilder sb, SwitchStatement switchStatement)
        {
            string functionName = this.CurrentFunctionDefinition.NameToken.Value;
            int switchId = this.switchCounter++;
            PythonFakeSwitchStatement fakeSwitchStatement = PythonFakeSwitchStatement.Build(switchStatement, switchId, functionName);

            sb.Append(this.CurrentTab);
            sb.Append(fakeSwitchStatement.ConditionVariableName);
            sb.Append(" = ");
            sb.Append(fakeSwitchStatement.DictionaryGlobalName);
            sb.Append(".get(");
            this.TranslateExpression(sb, switchStatement.Condition);
            sb.Append(", ");
            sb.Append(fakeSwitchStatement.DefaultId);
            sb.Append(')');
            sb.Append(this.NewLine);
            this.TranslateIfStatement(sb, fakeSwitchStatement.GenerateIfStatementBinarySearchTree());

            // This list of switch statements will be serialized at the end of the function definition as globals.
            this.SwitchStatements.Add(fakeSwitchStatement);
        }

        public override void TranslateThreadSleep(StringBuilder sb, Expression seconds)
        {
            sb.Append("time.sleep(");
            this.TranslateExpression(sb, seconds);
            sb.Append(')');
        }

        public override void TranslateVariable(StringBuilder sb, Variable variable)
        {
            if (variable.ApplyPrefix)
            {
                sb.Append("v_");
            }
            sb.Append(variable.Name);
        }

        public override void TranslateVariableDeclaration(StringBuilder sb, VariableDeclaration varDecl)
        {
            sb.Append(this.CurrentTab);
            sb.Append("v_");
            sb.Append(varDecl.VariableName.Value);
            sb.Append(" = ");
            this.TranslateExpression(sb, varDecl.Value);
            sb.Append(this.NewLine);
        }

        public override void TranslateVmDetermineLibraryAvailability(StringBuilder sb, Expression libraryName, Expression libraryVersion)
        {
            sb.Append("TranslationHelper_determineLibraryAvailability(");
            this.TranslateExpression(sb, libraryName);
            sb.Append(", ");
            this.TranslateExpression(sb, libraryVersion);
            sb.Append(')');
        }

        public override void TranslateVmEnqueueResume(StringBuilder sb, Expression seconds, Expression executionContextId)
        {
            throw new NotImplementedException();
        }

        public override void TranslateVmGetCurrentExecutionContextId(StringBuilder sb)
        {
            sb.Append("v_vm_getCurrentExecutionContextId()");
        }

        public override void TranslateVmRunLibraryManifest(
            StringBuilder sb,
            Expression libraryName,
            Expression libRegObj)
        {
            sb.Append("TranslationHelper_runLibraryManifest(");
            this.TranslateExpression(sb, libraryName);
            sb.Append(", ");
            this.TranslateExpression(sb, libRegObj);
            sb.Append(')');
        }

        public override void TranslateVmSuspend(StringBuilder sb)
        {
            sb.Append("v_vm_suspend()");
        }

        public override void TranslateWhileLoop(StringBuilder sb, WhileLoop whileLoop)
        {
            sb.Append(this.CurrentTab);
            sb.Append("while ");
            this.TranslateExpression(sb, whileLoop.Condition);
            sb.Append(':');
            sb.Append(this.NewLine);
            this.TabDepth++;
            this.TranslateExecutables(sb, whileLoop.Code);
            this.TabDepth--;
        }
    }
}
