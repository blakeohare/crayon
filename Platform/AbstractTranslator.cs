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
                case "IfStatement": this.TranslateIfStatement(sb, (IfStatement)executable); break;
                case "ReturnStatement": this.TranslateReturnStatemnt(sb, (ReturnStatement)executable); break;
                case "VariableDeclaration": this.TranslateVariableDeclaration(sb, (VariableDeclaration)executable); break;
                default:
                    throw new NotImplementedException(typeName);
            }
        }

        public void TranslateExpression(StringBuilder sb, Expression expression)
        {
            string typeName = expression.GetType().Name;
            switch (typeName)
            {
                case "FunctionInvocation": this.TranslateFunctionInvocation(sb, (FunctionInvocation)expression); break;
                case "FunctionReference": this.TranslateFunctionReference(sb, (FunctionReference)expression); break;
                case "NativeFunctionInvocation": this.TranslateNativeFunctionInvocation(sb, (NativeFunctionInvocation)expression); break;
                case "UnaryOp":
                    UnaryOp uo = (UnaryOp)expression;
                    if (uo.OpToken.Value == "-") this.TranslateNegative(sb, uo);
                    else this.TranslateBooleanNot(sb, uo);
                    break;
                case "Variable": this.TranslateVariable(sb, (Variable)expression); break;
                default: throw new NotImplementedException(typeName);
            }
        }

        public void TranslateNativeFunctionInvocation(StringBuilder sb, NativeFunctionInvocation nativeFuncInvocation)
        {
            Expression[] args = nativeFuncInvocation.Args;
            switch (nativeFuncInvocation.Function)
            {
                case Pastel.NativeFunction.DICTIONARY_CONTAINS_KEY: this.TranslateDictionaryContainsKey(sb, args[0], args[1]); break;
                default: throw new NotImplementedException(nativeFuncInvocation.Function.ToString());
            }
        }

        public abstract void TranslateAssignment(StringBuilder sb, Assignment assignment);
        public abstract void TranslateBooleanNot(StringBuilder sb, UnaryOp unaryOp);
        public abstract void TranslateDictionaryContainsKey(StringBuilder sb, Expression dictionary, Expression key);
        public abstract void TranslateFunctionInvocation(StringBuilder sb, FunctionInvocation funcInvocation);
        public abstract void TranslateFunctionReference(StringBuilder sb, FunctionReference funcRef);
        public abstract void TranslateIfStatement(StringBuilder sb, IfStatement ifStatement);
        public abstract void TranslateNegative(StringBuilder sb, UnaryOp unaryOp);
        public abstract void TranslateReturnStatemnt(StringBuilder sb, ReturnStatement returnStatement);
        public abstract void TranslateVariable(StringBuilder sb, Variable variable);
        public abstract void TranslateVariableDeclaration(StringBuilder sb, VariableDeclaration varDecl);
    }
}
