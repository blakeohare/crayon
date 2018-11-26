using Pastel.Nodes;

namespace Pastel.Transpilers
{
    internal abstract class CurlyBraceTranslator : AbstractTranslator
    {
        private bool isEgyptian;

        public CurlyBraceTranslator(string tabChar, string newline, bool isEgyptian)
            : base(tabChar, newline)
        {
            this.isEgyptian = isEgyptian;
        }

        public override void TranslateAssignment(TranspilerContext sb, Assignment assignment)
        {
            sb.Append(sb.CurrentTab);
            this.TranslateExpression(sb, assignment.Target);
            sb.Append(' ');
            sb.Append(assignment.OpToken.Value);
            sb.Append(' ');
            this.TranslateExpression(sb, assignment.Value);
            sb.Append(';');
            sb.Append(this.NewLine);
        }

        public override void TranslateBooleanConstant(TranspilerContext sb, bool value)
        {
            sb.Append(value ? "true" : "false");
        }

        public override void TranslateBooleanNot(TranspilerContext sb, UnaryOp unaryOp)
        {
            sb.Append('!');
            this.TranslateExpression(sb, unaryOp.Expression);
        }

        public override void TranslateBreak(TranspilerContext sb)
        {
            sb.Append(sb.CurrentTab);
            sb.Append("break;");
            sb.Append(this.NewLine);
        }

        public override void TranslateEmitComment(TranspilerContext sb, string value)
        {
            sb.Append("// ");
            sb.Append(value.Replace("\n", "\\n"));
        }

        public override void TranslateExpressionAsExecutable(TranspilerContext sb, Expression expression)
        {
            sb.Append(sb.CurrentTab);
            this.TranslateExpression(sb, expression);
            sb.Append(';');
            sb.Append(this.NewLine);
        }

        public override void TranslateFloatConstant(TranspilerContext sb, double value)
        {
            sb.Append(PastelUtil.FloatToString(value));
        }

        public override void TranslateFunctionInvocationWithPrefix(TranspilerContext sb, string prefix, FunctionReference funcRef, Expression[] args)
        {
            sb.Append(prefix);
            this.TranslateFunctionReference(sb, funcRef);
            sb.Append('(');
            this.TranslateCommaDelimitedExpressions(sb, args);
            sb.Append(')');
        }

        public override void TranslateFunctionInvocation(TranspilerContext sb, FunctionReference funcRef, Expression[] args)
        {
            this.TranslateFunctionReference(sb, funcRef);
            sb.Append('(');
            this.TranslateCommaDelimitedExpressions(sb, args);
            sb.Append(')');
        }

        public override void TranslateFunctionReference(TranspilerContext sb, FunctionReference funcRef)
        {
            sb.Append(funcRef.Function.NameToken.Value);
        }

        public override void TranslateIfStatement(TranspilerContext sb, IfStatement ifStatement)
        {
            sb.Append(sb.CurrentTab);
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
                sb.Append(sb.CurrentTab);
                sb.Append("{");
                sb.Append(this.NewLine);
            }

            sb.TabDepth++;
            this.TranslateExecutables(sb, ifStatement.IfCode);
            sb.TabDepth--;
            sb.Append(sb.CurrentTab);
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

                    sb.Append(sb.CurrentTab);
                    sb.Append("else");
                    sb.Append(this.NewLine);

                    sb.Append(sb.CurrentTab);
                    sb.Append("{");
                    sb.Append(this.NewLine);
                }

                sb.TabDepth++;
                this.TranslateExecutables(sb, ifStatement.ElseCode);
                sb.TabDepth--;

                sb.Append(sb.CurrentTab);
                sb.Append("}");
            }

            sb.Append(this.NewLine);
        }

        public override void TranslateInlineIncrement(TranspilerContext sb, Expression innerExpression, bool isPrefix, bool isAddition)
        {
            if (isPrefix) sb.Append(isAddition ? "++" : "--");
            this.TranslateExpression(sb, innerExpression);
            if (!isPrefix) sb.Append(isAddition ? "++" : "--");
        }

        public override void TranslateIntegerConstant(TranspilerContext sb, int value)
        {
            sb.Append(value.ToString());
        }

        public override void TranslateNegative(TranspilerContext sb, UnaryOp unaryOp)
        {
            sb.Append('-');
            this.TranslateExpression(sb, unaryOp.Expression);
        }

        public override void TranslateOpChain(TranspilerContext sb, OpChain opChain)
        {
            // Need to do something about these parenthesis.
            sb.Append('(');
            for (int i = 0; i < opChain.Expressions.Length; ++i)
            {
                if (i > 0)
                {
                    sb.Append(' ');
                    sb.Append(opChain.Ops[i - 1].Value);
                    sb.Append(' ');
                }
                this.TranslateExpression(sb, opChain.Expressions[i]);
            }
            sb.Append(')');
        }

        public override void TranslateReturnStatemnt(TranspilerContext sb, ReturnStatement returnStatement)
        {
            sb.Append(sb.CurrentTab);
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

        public override void TranslateStringConstant(TranspilerContext sb, string value)
        {
            sb.Append(PastelUtil.ConvertStringValueToCode(value));
        }

        public override void TranslateSwitchStatement(TranspilerContext sb, SwitchStatement switchStatement)
        {
            sb.Append(sb.CurrentTab);
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
                sb.Append(sb.CurrentTab);
                sb.Append('{');
            }
            sb.Append(this.NewLine);

            sb.TabDepth++;

            foreach (SwitchStatement.SwitchChunk chunk in switchStatement.Chunks)
            {
                for (int i = 0; i < chunk.Cases.Length; ++i)
                {
                    sb.Append(sb.CurrentTab);
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
                    sb.Append(this.NewLine);
                    sb.TabDepth++;
                    this.TranslateExecutables(sb, chunk.Code);
                    sb.TabDepth--;
                }
            }

            sb.TabDepth--;
            sb.Append(sb.CurrentTab);
            sb.Append('}');
            sb.Append(this.NewLine);
        }

        public override void TranslateVariable(TranspilerContext sb, Variable variable)
        {
            sb.Append(variable.Name);
        }

        public override void TranslateWhileLoop(TranspilerContext sb, WhileLoop whileLoop)
        {
            sb.Append(sb.CurrentTab);
            sb.Append("while (");
            this.TranslateExpression(sb, whileLoop.Condition);
            sb.Append(')');
            if (this.isEgyptian)
            {
                sb.Append(" {");
                sb.Append(this.NewLine);
            }
            else
            {
                sb.Append(this.NewLine);
                sb.Append(sb.CurrentTab);
                sb.Append("{");
                sb.Append(this.NewLine);
            }
            sb.TabDepth++;
            this.TranslateExecutables(sb, whileLoop.Code);
            sb.TabDepth--;
            sb.Append(sb.CurrentTab);
            sb.Append("}");
            sb.Append(this.NewLine);
        }
    }
}
