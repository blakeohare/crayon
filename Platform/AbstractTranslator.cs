using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pastel.Nodes;

namespace Platform
{
    public abstract class AbstractTranslator
    {
        private int currentTab = 0;
        private string tabChar;
        private string[] tabs;
        public string NewLine { get; private set; }
        public int TabDepth { get; set; }
        public Platform.AbstractPlatform Platform { get; private set; }

        public AbstractTranslator(Platform.AbstractPlatform platform, string tab, string newLine)
        {
            this.Platform = platform;
            this.TabDepth = 0;
            this.NewLine = newLine;
            this.tabChar = tab;
            this.tabs = new string[20];
            this.tabs[0] = "";
            for (int i = 1; i < 20; ++i)
            {
                this.tabs[i] = this.tabs[i - 1] + this.tabChar;
            }
        }

        public string CurrentTab
        {
            get
            {
                while (this.currentTab >= this.tabs.Length)
                {
                    // Conciseness, not efficiency. Deeply nested stuff is rare.
                    List<string> tabs = new List<string>(this.tabs);
                    for (int i = 0; i < 20; ++i)
                    {
                        tabs.Add(tabs[tabs.Count - 1] + this.tabChar);
                    }
                    this.tabs = tabs.ToArray();
                }
                return this.tabs[this.currentTab];
            }
        }

        public void TranslateExecutables(StringBuilder sb, Executable[] executables)
        {
            for (int i = 0; i < executables.Length; ++i)
            {
                this.TranslateExecutable(sb, executables[i]);
            }
        }

        public void TranslateExecutable(StringBuilder sb, Executable executable)
        {
            string typeName = executable.GetType().Name;
            switch (typeName)
            {
                case "Assignment": this.TranslateAssignment(sb, (Assignment)executable); break;
                case "BreakStatement": this.TranslateBreak(sb); break;
                case "ExpressionAsExecutable": this.TranslateExpressionAsExecutable(sb, ((ExpressionAsExecutable)executable).Expression); break;
                case "IfStatement": this.TranslateIfStatement(sb, (IfStatement)executable); break;
                case "ReturnStatement": this.TranslateReturnStatemnt(sb, (ReturnStatement)executable); break;
                case "SwitchStatement": this.TranslateSwitchStatement(sb, (SwitchStatement)executable); break;
                case "VariableDeclaration": this.TranslateVariableDeclaration(sb, (VariableDeclaration)executable); break;
                case "WhileLoop": this.TranslateWhileLoop(sb, (WhileLoop)executable); break;

                case "ExecutableBatch":
                    Executable[] execs = ((ExecutableBatch)executable).Executables;
                    for (int i = 0; i < execs.Length; ++i)
                    {
                        this.TranslateExecutable(sb, execs[i]);
                    }
                    break;

                default:
                    throw new NotImplementedException(typeName);
            }
        }

        public void TranslateExpression(StringBuilder sb, Expression expression)
        {
            string typeName = expression.GetType().Name;
            switch (typeName)
            {
                case "CastExpression": this.TranslateCast(sb, ((CastExpression)expression).Type, ((CastExpression)expression).Expression); break;
                case "ConstructorInvocation": this.TranslateConstructorInvocation(sb, (ConstructorInvocation)expression); break;
                case "FunctionInvocation": this.TranslateFunctionInvocation(sb, (FunctionInvocation)expression); break;
                case "FunctionReference": this.TranslateFunctionReference(sb, (FunctionReference)expression); break;
                case "NativeFunctionInvocation": this.TranslateNativeFunctionInvocation(sb, (NativeFunctionInvocation)expression); break;
                case "OpChain": this.TranslateOpChain(sb, (OpChain)expression); break;
                case "Variable": this.TranslateVariable(sb, (Variable)expression); break;

                case "DotField":
                    DotField df = (DotField)expression;
                    StructDefinition structDef = df.StructType;
                    if (structDef == null) throw new InvalidOperationException(); // should have been thrown by the compiler
                    string fieldName = df.FieldName.Value;
                    int fieldIndex = structDef.ArgIndexByName[fieldName];
                    this.TranslateStructFieldDereferenc(sb, df.Root, structDef, fieldName, fieldIndex);
                    break;

                case "InlineConstant":
                    InlineConstant ic = (InlineConstant)expression;
                    switch (ic.ResolvedType.RootValue)
                    {
                        case "bool": this.TranslateBooleanConstant(sb, (bool)ic.Value); break;
                        case "char": this.TranslateCharConstant(sb, ((string)ic.Value)[0]); break;
                        case "double": this.TranslateFloatConstant(sb, (double)ic.Value); break;
                        case "int": this.TranslateIntegerConstant(sb, (int)ic.Value); break;
                        case "null": this.TranslateNullConstant(sb); break;
                        case "string": this.TranslateStringConstant(sb, (string)ic.Value); break;
                        default: throw new NotImplementedException();
                    }
                    break;

                case "UnaryOp":
                    UnaryOp uo = (UnaryOp)expression;
                    if (uo.OpToken.Value == "-") this.TranslateNegative(sb, uo);
                    else this.TranslateBooleanNot(sb, uo);
                    break;

                default: throw new NotImplementedException(typeName);
            }
        }

        public void TranslateNativeFunctionInvocation(StringBuilder sb, NativeFunctionInvocation nativeFuncInvocation)
        {
            Expression[] args = nativeFuncInvocation.Args;
            switch (nativeFuncInvocation.Function)
            {
                case Pastel.NativeFunction.ARRAY_GET: this.TranslateArrayGet(sb, args[0], args[1]); break;
                case Pastel.NativeFunction.ARRAY_LENGTH: this.TranslateArrayLength(sb, args[0]); break;
                case Pastel.NativeFunction.ARRAY_SET: this.TranslateArraySet(sb, args[0], args[1], args[2]); break;
                case Pastel.NativeFunction.CHAR_TO_STRING: this.TranslateCharToString(sb, args[0]); break;
                case Pastel.NativeFunction.CHR: this.TranslateChr(sb, args[0]); break;
                case Pastel.NativeFunction.COMMAND_LINE_ARGS: this.TranslateCommandLineArgs(sb); break;
                case Pastel.NativeFunction.CONVERT_RAW_DICTIONARY_VALUE_COLLECTION_TO_A_REUSABLE_VALUE_LIST: this.TranslateConvertRawDictionaryValueCollectionToAReusableValueList(sb, args[0]); break;
                case Pastel.NativeFunction.CURRENT_TIME_SECONDS: this.TranslateCurrentTimeSeconds(sb); break;
                case Pastel.NativeFunction.DICTIONARY_CONTAINS_KEY: this.TranslateDictionaryContainsKey(sb, args[0], args[1]); break;
                case Pastel.NativeFunction.DICTIONARY_GET: this.TranslateDictionaryGet(sb, args[0], args[1]); break;
                case Pastel.NativeFunction.DICTIONARY_KEYS: this.TranslateDictionaryKeys(sb, args[0]); break;
                case Pastel.NativeFunction.DICTINOARY_KEYS_TO_VALUE_LIST: this.TranslateDictionaryKeysToValueList(sb, args[0]); break;
                case Pastel.NativeFunction.DICTIONARY_NEW: this.TranslateDictionaryNew(sb, nativeFuncInvocation.ResolvedType.Generics[0], nativeFuncInvocation.ResolvedType.Generics[1]); break;
                case Pastel.NativeFunction.DICTIONARY_REMOVE: this.TranslateDictionaryRemove(sb, args[0], args[1]); break;
                case Pastel.NativeFunction.DICTIONARY_SET: this.TranslateDictionarySet(sb, args[0], args[1], args[2]); break;
                case Pastel.NativeFunction.DICTIONARY_SIZE: this.TranslateDictionarySize(sb, args[0]); break;
                case Pastel.NativeFunction.DICTIONARY_VALUES: this.TranslateDictionaryValues(sb, args[0]); break;
                case Pastel.NativeFunction.DICTIONARY_VALUES_TO_VALUE_LIST: this.TranslateDictionaryValues(sb, args[0]); break;
                case Pastel.NativeFunction.EMIT_COMMENT: this.TranslateEmitComment(sb, ((InlineConstant)args[0]).Value.ToString()); break;
                case Pastel.NativeFunction.FLOAT_DIVISION: this.TranslateFloatDivision(sb, args[0], args[1]); break;
                case Pastel.NativeFunction.FLOAT_TO_STRING: this.TranslateFloatToString(sb, args[0]); break;
                case Pastel.NativeFunction.FORCE_PARENS: this.TranslateForceParens(sb, args[0]); break;
                case Pastel.NativeFunction.GET_PROGRAM_DATA: this.TranslateGetProgramData(sb); break;
                case Pastel.NativeFunction.GET_RESOURCE_MANIFEST: this.TranslateGetResourceManifest(sb); break;
                case Pastel.NativeFunction.INT: this.TranslateFloatToInt(sb, args[0]); break;
                case Pastel.NativeFunction.INT_BUFFER_16: this.TranslateIntBuffer16(sb); break;
                case Pastel.NativeFunction.INT_TO_STRING: this.TranslateIntToString(sb, args[0]); break;
                case Pastel.NativeFunction.INTEGER_DIVISION: this.TranslateIntegerDivision(sb, args[0], args[1]); break;
                case Pastel.NativeFunction.INVOKE_DYNAMIC_LIBRARY_FUNCTION: this.TranslateInvokeDynamicLibraryFunction(sb, args[0], args[1]); break;
                case Pastel.NativeFunction.IS_VALID_INTEGER: this.TranslateIsValidInteger(sb, args[0]); break;
                case Pastel.NativeFunction.LIST_ADD: this.TranslateListAdd(sb, args[0], args[1]); break;
                case Pastel.NativeFunction.LIST_ADD_ALL: this.TranslateListAddAll(sb, args[0], args[1]); break;
                case Pastel.NativeFunction.LIST_CLEAR: this.TranslateListClear(sb, args[0]); break;
                case Pastel.NativeFunction.LIST_GET: this.TranslateListGet(sb, args[0], args[1]); break;
                case Pastel.NativeFunction.LIST_INSERT: this.TranslateListInsert(sb, args[0], args[1], args[2]); break;
                case Pastel.NativeFunction.LIST_JOIN_CHARS: this.TranslateListJoinChars(sb, args[0]); break;
                case Pastel.NativeFunction.LIST_JOIN_STRINGS: this.TranslateListJoinStrings(sb, args[0], args[1]); break;
                case Pastel.NativeFunction.LIST_NEW: this.TranslateListNew(sb, nativeFuncInvocation.ResolvedType.Generics[0]); break;
                case Pastel.NativeFunction.LIST_POP: this.TranslateListPop(sb, args[0]); break;
                case Pastel.NativeFunction.LIST_REMOVE_AT: this.TranslateListRemoveAt(sb, args[0], args[1]); break;
                case Pastel.NativeFunction.LIST_REVERSE: this.TranslateListReverse(sb, args[0]); break;
                case Pastel.NativeFunction.LIST_SET: this.TranslateListSet(sb, args[0], args[1], args[2]); break;
                case Pastel.NativeFunction.LIST_SHUFFLE: this.TranslateListShuffle(sb, args[0]); break;
                case Pastel.NativeFunction.LIST_SIZE: this.TranslateListSize(sb, args[0]); break;
                case Pastel.NativeFunction.LIST_TO_ARRAY: this.TranslateListToArray(sb, args[0]); break;
                case Pastel.NativeFunction.MATH_ARCCOS: this.TranslateMathArcCos(sb, args[0]); break;
                case Pastel.NativeFunction.MATH_ARCSIN: this.TranslateMathArcSin(sb, args[0]); break;
                case Pastel.NativeFunction.MATH_ARCTAN: this.TranslateMathArcTan(sb, args[0], args[1]); break;
                case Pastel.NativeFunction.MATH_COS: this.TranslateMathCos(sb, args[0]); break;
                case Pastel.NativeFunction.MATH_LOG: this.TranslateMathLog(sb, args[0]); break;
                case Pastel.NativeFunction.MATH_POW: this.TranslateMathPow(sb, args[0], args[1]); break;
                case Pastel.NativeFunction.MATH_SIN: this.TranslateMathSin(sb, args[0]); break;
                case Pastel.NativeFunction.MATH_TAN: this.TranslateMathTan(sb, args[0]); break;
                case Pastel.NativeFunction.MULTIPLY_LIST: this.TranslateMultiplyList(sb, args[0], args[1]); break;
                case Pastel.NativeFunction.PARSE_FLOAT: this.TranslateParseFloat(sb, args[0], args[1]); break;
                case Pastel.NativeFunction.PARSE_FLOAT_REDUNDANT: this.TranslateParseFloatREDUNDANT(sb, args[0]); break;
                case Pastel.NativeFunction.PARSE_INT: this.TranslateParseInt(sb, args[0]); break;
                case Pastel.NativeFunction.PRINT_STDERR: this.TranslatePrintStdErr(sb, args[0]); break;
                case Pastel.NativeFunction.PRINT_STDOUT: this.TranslatePrintStdOut(sb, args[0]); break;
                case Pastel.NativeFunction.RANDOM_FLOAT: this.TranslateRandomFloat(sb); break;
                case Pastel.NativeFunction.READ_BYTE_CODE_FILE: this.TranslateReadByteCodeFile(sb); break;
                case Pastel.NativeFunction.SET_PROGRAM_DATA: this.TranslateSetProgramData(sb, args[0]); break;
                case Pastel.NativeFunction.SORTED_COPY_OF_INT_ARRAY: this.TranslateSortedCopyOfIntArray(sb, args[0]); break;
                case Pastel.NativeFunction.SORTED_COPY_OF_STRING_ARRAY: this.TranslateSortedCopyOfStringArray(sb, args[0]); break;
                case Pastel.NativeFunction.STRING_APPEND: this.TranslateStringAppend(sb, args[0], args[1]); break;
                case Pastel.NativeFunction.STRING_BUFFER_16: this.TranslateStringBuffer16(sb); break;
                case Pastel.NativeFunction.STRING_CHAR_AT: this.TranslateStringCharAt(sb, args[0], args[1]); break;
                case Pastel.NativeFunction.STRING_CHAR_CODE_AT: this.TranslateStringCharCodeAt(sb, args[0], args[1]); break;
                case Pastel.NativeFunction.STRING_COMPARE_IS_REVERSE: this.TranslateStringCompareIsReverse(sb, args[0], args[1]); break;
                case Pastel.NativeFunction.STRING_CONCAT_ALL: this.TranslateStringConcatAll(sb, args); break;
                case Pastel.NativeFunction.STRING_CONTAINS: this.TranslateStringContains(sb, args[0], args[1]); break;
                case Pastel.NativeFunction.STRING_ENDS_WITH: this.TranslateStringEndsWith(sb, args[0], args[1]); break;
                case Pastel.NativeFunction.STRING_EQUALS: this.TranslateStringEquals(sb, args[0], args[1]); break;
                case Pastel.NativeFunction.STRING_FROM_CHAR_CODE: this.TranslateStringFromCharCode(sb, args[0]); break;
                case Pastel.NativeFunction.STRING_INDEX_OF: this.TranslateStringIndexOf(sb, args[0], args[1]); break;
                case Pastel.NativeFunction.STRING_LENGTH: this.TranslateStringLength(sb, args[0]); break;
                case Pastel.NativeFunction.STRING_REPLACE: this.TranslateStringReplace(sb, args[0], args[1], args[2]); break;
                case Pastel.NativeFunction.STRING_REVERSE: this.TranslateStringReverse(sb, args[0]); break;
                case Pastel.NativeFunction.STRING_SPLIT: this.TranslateStringSplit(sb, args[0], args[1]); break;
                case Pastel.NativeFunction.STRING_STARTS_WITH: this.TranslateStringStartsWith(sb, args[0], args[1]); break;
                case Pastel.NativeFunction.STRING_TO_LOWER: this.TranslateStringToLower(sb, args[0]); break;
                case Pastel.NativeFunction.STRING_TO_UPPER: this.TranslateStringToUpper(sb, args[0]); break;
                case Pastel.NativeFunction.STRING_TRIM: this.TranslateStringTrim(sb, args[0]); break;
                case Pastel.NativeFunction.STRING_TRIM_END: this.TranslateStringTrimEnd(sb, args[0]); break;
                case Pastel.NativeFunction.STRING_TRIM_START: this.TranslateStringTrimStart(sb, args[0]); break;
                case Pastel.NativeFunction.STRONG_REFERENCE_EQUALITY: this.TranslateStrongReferenceEquality(sb, args[0], args[1]); break;
                case Pastel.NativeFunction.THREAD_SLEEP: this.TranslateThreadSleep(sb, args[0]); break;
                case Pastel.NativeFunction.VM_GET_CURRENT_EXECUTION_CONTEXT_ID: this.TranslateVmGetCurrentExecutionContextId(sb); break;
                case Pastel.NativeFunction.VM_SUSPEND: this.TranslateVmSuspend(sb); break;
                
                default: throw new NotImplementedException(nativeFuncInvocation.Function.ToString());
            }
        }

        public abstract void TranslateArrayGet(StringBuilder sb, Expression array, Expression index);
        public abstract void TranslateArrayLength(StringBuilder sb, Expression array);
        public abstract void TranslateArraySet(StringBuilder sb, Expression array, Expression index, Expression value);
        public abstract void TranslateAssignment(StringBuilder sb, Assignment assignment);
        public abstract void TranslateBooleanConstant(StringBuilder sb, bool value);
        public abstract void TranslateBooleanNot(StringBuilder sb, UnaryOp unaryOp);
        public abstract void TranslateBreak(StringBuilder sb);
        public abstract void TranslateCast(StringBuilder sb, PType type, Expression expression);
        public abstract void TranslateCharConstant(StringBuilder sb, char value);
        public abstract void TranslateCharToString(StringBuilder sb, Expression charValue);
        public abstract void TranslateChr(StringBuilder sb, Expression charCode);
        public abstract void TranslateCommandLineArgs(StringBuilder sb);
        public abstract void TranslateConstructorInvocation(StringBuilder sb, ConstructorInvocation constructorInvocation);
        public abstract void TranslateConvertRawDictionaryValueCollectionToAReusableValueList(StringBuilder sb, Expression dictionary);
        public abstract void TranslateCurrentTimeSeconds(StringBuilder sb);
        public abstract void TranslateDictionaryContainsKey(StringBuilder sb, Expression dictionary, Expression key);
        public abstract void TranslateDictionaryGet(StringBuilder sb, Expression dictionary, Expression key);
        public abstract void TranslateDictionaryKeys(StringBuilder sb, Expression dictionary);
        public abstract void TranslateDictionaryKeysToValueList(StringBuilder sb, Expression dictionary);
        public abstract void TranslateDictionaryNew(StringBuilder sb, PType keyType, PType valueType);
        public abstract void TranslateDictionaryRemove(StringBuilder sb, Expression dictionary, Expression key);
        public abstract void TranslateDictionarySet(StringBuilder sb, Expression dictionary, Expression key, Expression value);
        public abstract void TranslateDictionarySize(StringBuilder sb, Expression dictionary);
        public abstract void TranslateDictionaryValues(StringBuilder sb, Expression dictionary);
        public abstract void TranslateDictionaryValuesToValueList(StringBuilder sb, Expression dictionary);
        public abstract void TranslateEmitComment(StringBuilder sb, string value);
        public abstract void TranslateExpressionAsExecutable(StringBuilder sb, Expression expression);
        public abstract void TranslateFloatConstant(StringBuilder sb, double value);
        public abstract void TranslateFloatDivision(StringBuilder sb, Expression floatNumerator, Expression floatDenominator);
        public abstract void TranslateFloatToInt(StringBuilder sb, Expression floatExpr);
        public abstract void TranslateFloatToString(StringBuilder sb, Expression floatExpr);
        public abstract void TranslateForceParens(StringBuilder sb, Expression expression);
        public abstract void TranslateFunctionInvocation(StringBuilder sb, FunctionInvocation funcInvocation);
        public abstract void TranslateFunctionReference(StringBuilder sb, FunctionReference funcRef);
        public abstract void TranslateGetProgramData(StringBuilder sb);
        public abstract void TranslateGetResourceManifest(StringBuilder sb);
        public abstract void TranslateIfStatement(StringBuilder sb, IfStatement ifStatement);
        public abstract void TranslateIntBuffer16(StringBuilder sb);
        public abstract void TranslateIntegerConstant(StringBuilder sb, int value);
        public abstract void TranslateIntegerDivision(StringBuilder sb, Expression integerNumerator, Expression integerDenominator);
        public abstract void TranslateIntToString(StringBuilder sb, Expression integer);
        public abstract void TranslateInvokeDynamicLibraryFunction(StringBuilder sb, Expression functionId, Expression argsArray);
        public abstract void TranslateIsValidInteger(StringBuilder sb, Expression stringValue);
        public abstract void TranslateListAdd(StringBuilder sb, Expression list, Expression item);
        public abstract void TranslateListAddAll(StringBuilder sb, Expression list, Expression items);
        public abstract void TranslateListClear(StringBuilder sb, Expression list);
        public abstract void TranslateListGet(StringBuilder sb, Expression list, Expression index);
        public abstract void TranslateListInsert(StringBuilder sb, Expression list, Expression index, Expression item);
        public abstract void TranslateListJoinChars(StringBuilder sb, Expression list);
        public abstract void TranslateListJoinStrings(StringBuilder sb, Expression list, Expression sep);
        public abstract void TranslateListNew(StringBuilder sb, PType type);
        public abstract void TranslateListPop(StringBuilder sb, Expression list);
        public abstract void TranslateListRemoveAt(StringBuilder sb, Expression list, Expression index);
        public abstract void TranslateListReverse(StringBuilder sb, Expression list);
        public abstract void TranslateListSet(StringBuilder sb, Expression list, Expression index, Expression value);
        public abstract void TranslateListShuffle(StringBuilder sb, Expression list);
        public abstract void TranslateListSize(StringBuilder sb, Expression list);
        public abstract void TranslateListToArray(StringBuilder sb, Expression list);
        public abstract void TranslateMathArcCos(StringBuilder sb, Expression ratio);
        public abstract void TranslateMathArcSin(StringBuilder sb, Expression ratio);
        public abstract void TranslateMathArcTan(StringBuilder sb, Expression yComponent, Expression xComponent);
        public abstract void TranslateMathCos(StringBuilder sb, Expression thetaRadians);
        public abstract void TranslateMathLog(StringBuilder sb, Expression value);
        public abstract void TranslateMathPow(StringBuilder sb, Expression expBase, Expression exponent);
        public abstract void TranslateMathSin(StringBuilder sb, Expression thetaRadians);
        public abstract void TranslateMathTan(StringBuilder sb, Expression thetaRadians);
        public abstract void TranslateMultiplyList(StringBuilder sb, Expression list, Expression n);
        public abstract void TranslateNegative(StringBuilder sb, UnaryOp unaryOp);
        public abstract void TranslateNullConstant(StringBuilder sb);
        public abstract void TranslateOpChain(StringBuilder sb, OpChain opChain);
        public abstract void TranslateParseFloat(StringBuilder sb, Expression stringValue, Expression floatOutList);
        public abstract void TranslateParseFloatREDUNDANT(StringBuilder sb, Expression stringValue);
        public abstract void TranslateParseInt(StringBuilder sb, Expression safeStringValue);
        public abstract void TranslatePrintStdErr(StringBuilder sb, Expression value);
        public abstract void TranslatePrintStdOut(StringBuilder sb, Expression value);
        public abstract void TranslateRandomFloat(StringBuilder sb);
        public abstract void TranslateReadByteCodeFile(StringBuilder sb);
        public abstract void TranslateReturnStatemnt(StringBuilder sb, ReturnStatement returnStatement);
        public abstract void TranslateSetProgramData(StringBuilder sb, Expression programData);
        public abstract void TranslateSortedCopyOfIntArray(StringBuilder sb, Expression intArray);
        public abstract void TranslateSortedCopyOfStringArray(StringBuilder sb, Expression stringArray);
        public abstract void TranslateStringAppend(StringBuilder sb, Expression str1, Expression str2);
        public abstract void TranslateStringBuffer16(StringBuilder sb);
        public abstract void TranslateStringCharAt(StringBuilder sb, Expression str, Expression index);
        public abstract void TranslateStringCharCodeAt(StringBuilder sb, Expression str, Expression index);
        public abstract void TranslateStringCompareIsReverse(StringBuilder sb, Expression str1, Expression str2);
        public abstract void TranslateStringConcatAll(StringBuilder sb, Expression[] strings);
        public abstract void TranslateStringConstant(StringBuilder sb, string value);
        public abstract void TranslateStringContains(StringBuilder sb, Expression haystack, Expression needle);
        public abstract void TranslateStringEndsWith(StringBuilder sb, Expression haystack, Expression needle);
        public abstract void TranslateStringEquals(StringBuilder sb, Expression left, Expression right);
        public abstract void TranslateStringFromCharCode(StringBuilder sb, Expression charCode);
        public abstract void TranslateStringIndexOf(StringBuilder sb, Expression haystack, Expression needle);
        public abstract void TranslateStringLength(StringBuilder sb, Expression str);
        public abstract void TranslateStringReplace(StringBuilder sb, Expression haystack, Expression needle, Expression newNeedle);
        public abstract void TranslateStringReverse(StringBuilder sb, Expression str);
        public abstract void TranslateStringSplit(StringBuilder sb, Expression haystack, Expression needle);
        public abstract void TranslateStringStartsWith(StringBuilder sb, Expression haystack, Expression needle);
        public abstract void TranslateStringToLower(StringBuilder sb, Expression str);
        public abstract void TranslateStringToUpper(StringBuilder sb, Expression str);
        public abstract void TranslateStringTrim(StringBuilder sb, Expression str);
        public abstract void TranslateStringTrimEnd(StringBuilder sb, Expression str);
        public abstract void TranslateStringTrimStart(StringBuilder sb, Expression str);
        public abstract void TranslateStrongReferenceEquality(StringBuilder sb, Expression left, Expression right);
        public abstract void TranslateThreadSleep(StringBuilder sb, Expression seconds);
        public abstract void TranslateStructFieldDereferenc(StringBuilder sb, Expression root, StructDefinition structDef, string fieldName, int fieldIndex);
        public abstract void TranslateSwitchStatement(StringBuilder sb, SwitchStatement switchStatement);
        public abstract void TranslateVariable(StringBuilder sb, Variable variable);
        public abstract void TranslateVariableDeclaration(StringBuilder sb, VariableDeclaration varDecl);
        public abstract void TranslateVmGetCurrentExecutionContextId(StringBuilder sb);
        public abstract void TranslateVmSuspend(StringBuilder sb);
        public abstract void TranslateWhileLoop(StringBuilder sb, WhileLoop whileLoop);
    }
}
