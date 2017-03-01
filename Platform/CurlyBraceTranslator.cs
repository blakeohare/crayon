using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pastel.Nodes;

namespace Platform
{
    public abstract class CurlyBraceTranslator : AbstractTranslator
    {
        private bool isEgyptian;

        public CurlyBraceTranslator(Platform.AbstractPlatform platform, string tabChar, string newline, bool isEgyptian)
            : base(platform, tabChar, newline)
        {
            this.isEgyptian = isEgyptian;
        }

        public override void TranslateAssignment(StringBuilder sb, Assignment assignment)
        {
            sb.Append(this.CurrentTab);
            this.TranslateExpression(sb, assignment.Target);
            sb.Append(" = ");
            this.TranslateExpression(sb, assignment.Value);
            sb.Append(';');
        }

        public override void TranslateBooleanConstant(StringBuilder sb, bool value)
        {
            sb.Append(value ? "true" : "false");
        }

        public override void TranslateBooleanNot(StringBuilder sb, UnaryOp unaryOp)
        {
            sb.Append('!');
            this.TranslateExpression(sb, unaryOp.Expression);
        }

        public override void TranslateBreak(StringBuilder sb)
        {
            sb.Append(this.CurrentTab);
            sb.Append("break;");
            sb.Append(this.NewLine);
        }

        public override void TranslateEmitComment(StringBuilder sb, string value)
        {
            sb.Append("// ");
            sb.Append(value.Replace("\n", "\\n"));
        }

        public override void TranslateExpressionAsExecutable(StringBuilder sb, Expression expression)
        {
            sb.Append(this.CurrentTab);
            this.TranslateExpression(sb, expression);
            sb.Append(';');
            sb.Append(this.NewLine);
        }

        public override void TranslateFloatConstant(StringBuilder sb, double value)
        {
            sb.Append(Common.Util.FloatToString(value));
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
            for (int i = 0; i < args.Length; ++i)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }
                this.TranslateExpression(sb, args[i]);
            }
            sb.Append(')');
        }

        public override void TranslateFunctionReference(StringBuilder sb, FunctionReference funcRef)
        {
            sb.Append("v_");
            sb.Append(funcRef.Function.NameToken.Value);
        }

        public override void TranslateIfStatement(StringBuilder sb, IfStatement ifStatement)
        {
            sb.Append(this.CurrentTab);
            sb.Append("if (");
            this.TranslateExpression(sb, ifStatement.Condition);
            if (this.isEgyptian)
            {
                sb.Append(") {");
                sb.Append(this.NewLine);
            }
            else
            {
                sb.Append(")");
                sb.Append(this.NewLine);
                sb.Append(this.CurrentTab);
                sb.Append("{");
                sb.Append(this.NewLine);
            }

            this.TabDepth++;
            this.TranslateExecutables(sb, ifStatement.IfCode);
            this.TabDepth--;
            sb.Append(this.CurrentTab);
            sb.Append("}");

            if (ifStatement.ElseCode.Length > 0)
            {
                if (this.isEgyptian)
                {
                    sb.Append(" else {");
                    sb.Append(this.NewLine);
                }
                else
                {
                    sb.Append(this.NewLine);

                    sb.Append(this.CurrentTab);
                    sb.Append("else");
                    sb.Append(this.NewLine);

                    sb.Append(this.CurrentTab);
                    sb.Append("{");
                    sb.Append(this.NewLine);
                }

                this.TabDepth++;
                this.TranslateExecutables(sb, ifStatement.ElseCode);
                this.TabDepth--;

                sb.Append(this.CurrentTab);
                sb.Append("}");
            }

            sb.Append(this.NewLine);
        }

        public override void TranslateIntegerConstant(StringBuilder sb, int value)
        {
            sb.Append(value.ToString());
        }

        public override void TranslateNegative(StringBuilder sb, UnaryOp unaryOp)
        {
            sb.Append('-');
            this.TranslateExpression(sb, unaryOp.Expression);
        }

        public override void TranslateOpChain(StringBuilder sb, OpChain opChain)
        {
            for (int i = 0; i < opChain.Expressions.Length; ++i)
            {
                if (i > 0)
                {
                    sb.Append(' ');
                    sb.Append(opChain.Ops[i - 1]);
                    sb.Append(' ');
                }
                this.TranslateExpression(sb, opChain.Expressions[i]);
            }
        }

        public override void TranslateReturnStatemnt(StringBuilder sb, ReturnStatement returnStatement)
        {
            sb.Append(this.CurrentTab);
            sb.Append("return ");
            if (returnStatement.Expression == null)
            {
                sb.Append("null");
            }
            else
            {
                this.TranslateExpression(sb, returnStatement.Expression);
            }
            sb.Append(';');
            sb.Append(this.NewLine);
        }

        public override void TranslateStringConstant(StringBuilder sb, string value)
        {
            sb.Append(Common.Util.ConvertStringValueToCode(value));
        }

        public override void TranslateSwitchStatement(StringBuilder sb, SwitchStatement switchStatement)
        {
            sb.Append(this.CurrentTab);
            sb.Append("switch (");
            this.TranslateExpression(sb, switchStatement.Condition);
            sb.Append(")");
            if (this.isEgyptian)
            {
                sb.Append(" {");
            }
            else
            {
                sb.Append(this.NewLine);
                sb.Append(this.CurrentTab);
                sb.Append('{');
                sb.Append(this.CurrentTab);
            }
            this.TabDepth++;

            foreach (SwitchStatement.SwitchChunk chunk in switchStatement.Chunks)
            {
                for (int i = 0; i < chunk.Cases.Length; ++i)
                {
                    sb.Append(this.CurrentTab);
                    Expression c = chunk.Cases[i];
                    if (c == null)
                    {
                        sb.Append("default:");
                    }
                    else
                    {
                        sb.Append("case ");
                        this.TranslateExpression(sb, c);
                        sb.Append(':');
                    }
                    this.TabDepth++;
                    this.TranslateExecutables(sb, chunk.Code);
                    this.TabDepth--;
                }
            }

            this.TabDepth--;
            sb.Append(this.CurrentTab);
            sb.Append('}');
            sb.Append(this.NewLine);
        }

        public override void TranslateVariable(StringBuilder sb, Variable variable)
        {
            sb.Append("v_");
            sb.Append(variable.Name);
        }

        public override void TranslateWhileLoop(StringBuilder sb, WhileLoop whileLoop)
        {
            sb.Append(this.CurrentTab);
            sb.Append("while (");
            this.TranslateExpression(sb, whileLoop.Condition);
            sb.Append("(");
            if (this.isEgyptian)
            {
                sb.Append(" {");
                sb.Append(this.NewLine);
            }
            else
            {
                sb.Append(this.NewLine);
                sb.Append(this.CurrentTab);
                sb.Append("{");
                sb.Append(this.NewLine);
            }
            this.TabDepth++;
            this.TranslateExecutables(sb, whileLoop.Code);
            this.TabDepth--;
            sb.Append(this.CurrentTab);
            sb.Append("}");
            sb.Append(this.NewLine);
        }
    }
}
