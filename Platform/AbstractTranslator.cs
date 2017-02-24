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
                case "ExpressionAsExecutable": this.TranslateExpressionAsExecutable(sb, ((ExpressionAsExecutable)executable).Expression); break;
                case "IfStatement": this.TranslateIfStatement(sb, (IfStatement)executable); break;
                case "ReturnStatement": this.TranslateReturnStatemnt(sb, (ReturnStatement)executable); break;
                case "VariableDeclaration": this.TranslateVariableDeclaration(sb, (VariableDeclaration)executable); break;
                case "WhileLoop": this.TranslateWhileLoop(sb, (WhileLoop)executable); break;
                default:
                    throw new NotImplementedException(typeName);
            }
        }

        public void TranslateExpression(StringBuilder sb, Expression expression)
        {
            string typeName = expression.GetType().Name;
            switch (typeName)
            {
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
                case Pastel.NativeFunction.DICTIONARY_CONTAINS_KEY: this.TranslateDictionaryContainsKey(sb, args[0], args[1]); break;
                case Pastel.NativeFunction.DICTIONARY_GET: this.TranslateDictionaryGet(sb, args[0], args[1]); break;
                case Pastel.NativeFunction.DICTIONARY_SET: this.TranslateDictionarySet(sb, args[0], args[1], args[2]); break;
                case Pastel.NativeFunction.DICTIONARY_SIZE: this.TranslateDictionarySize(sb, args[0]); break;
                case Pastel.NativeFunction.STRING_LENGTH: this.TranslateStringLength(sb, args[0]); break;
                default: throw new NotImplementedException(nativeFuncInvocation.Function.ToString());
            }
        }

        public abstract void TranslateArrayGet(StringBuilder sb, Expression array, Expression index);
        public abstract void TranslateArrayLength(StringBuilder sb, Expression array);
        public abstract void TranslateArraySet(StringBuilder sb, Expression array, Expression index, Expression value);
        public abstract void TranslateAssignment(StringBuilder sb, Assignment assignment);
        public abstract void TranslateBooleanConstant(StringBuilder sb, bool value);
        public abstract void TranslateBooleanNot(StringBuilder sb, UnaryOp unaryOp);
        public abstract void TranslateConstructorInvocation(StringBuilder sb, ConstructorInvocation constructorInvocation);
        public abstract void TranslateDictionaryContainsKey(StringBuilder sb, Expression dictionary, Expression key);
        public abstract void TranslateDictionaryGet(StringBuilder sb, Expression dictionary, Expression key);
        public abstract void TranslateDictionarySet(StringBuilder sb, Expression dictionary, Expression key, Expression value);
        public abstract void TranslateDictionarySize(StringBuilder sb, Expression dictionary);
        public abstract void TranslateExpressionAsExecutable(StringBuilder sb, Expression expression);
        public abstract void TranslateFloatConstant(StringBuilder sb, double value);
        public abstract void TranslateFunctionInvocation(StringBuilder sb, FunctionInvocation funcInvocation);
        public abstract void TranslateFunctionReference(StringBuilder sb, FunctionReference funcRef);
        public abstract void TranslateIfStatement(StringBuilder sb, IfStatement ifStatement);
        public abstract void TranslateIntegerConstant(StringBuilder sb, int value);
        public abstract void TranslateNegative(StringBuilder sb, UnaryOp unaryOp);
        public abstract void TranslateNullConstant(StringBuilder sb);
        public abstract void TranslateOpChain(StringBuilder sb, OpChain opChain);
        public abstract void TranslateReturnStatemnt(StringBuilder sb, ReturnStatement returnStatement);
        public abstract void TranslateStringConstant(StringBuilder sb, string value);
        public abstract void TranslateStringLength(StringBuilder sb, Expression str);
        public abstract void TranslateStructFieldDereferenc(StringBuilder sb, Expression root, StructDefinition structDef, string fieldName, int fieldIndex);
        public abstract void TranslateVariable(StringBuilder sb, Variable variable);
        public abstract void TranslateVariableDeclaration(StringBuilder sb, VariableDeclaration varDecl);
        public abstract void TranslateWhileLoop(StringBuilder sb, WhileLoop whileLoop);
    }
}
