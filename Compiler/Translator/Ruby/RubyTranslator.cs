using System;
using System.Collections.Generic;
using Crayon.ParseTree;
using Common;

namespace Crayon.Translator.Ruby
{
    internal class RubyTranslator : AbstractTranslator
    {
        protected override void TranslateAssignment(List<string> output, Assignment assignment)
        {
            output.Add(this.CurrentTabIndention);
            this.TranslateExpression(output, assignment.Target);
            output.Add(" ");
            output.Add(assignment.AssignmentOp);
            output.Add(" ");
            this.TranslateExpression(output, assignment.Value);
            output.Add(this.NL);
        }

        protected override void TranslateBinaryOpSyntax(List<string> output, string tokenValue)
        {
            base.TranslateBinaryOpSyntax(output, tokenValue);
        }

        protected override void TranslateForLoop(List<string> output, ForLoop forLoop)
        {
            this.Translate(output, forLoop.Init);
            output.Add(this.CurrentTabIndention);
            output.Add("while ");
            this.TranslateExpression(output, forLoop.Condition);
            output.Add(this.NL);

            this.CurrentIndention++;
            this.Translate(output, forLoop.Code);
            this.Translate(output, forLoop.Step);
            this.CurrentIndention--;

            output.Add(this.CurrentTabIndention);
            output.Add("end");
            output.Add(this.NL);
        }

        protected override void TranslateDotStep(List<string> output, DotStep dotStep)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateVariable(List<string> output, Variable expr)
        {
            // TODO: No. Use a common list somewhere else.
            switch (expr.Name)
            {
                case "INTEGERS_POSITIVE_CACHE":
                case "INTEGERS_NEGATIVE_CACHE":
                case "VALUE_INT_ZERO":
                case "VALUE_INT_ONE":
                case "VALUE_INT_NEG_ONE":
                case "VALUE_NULL":
                case "VALUE_TRUE":
                case "VALUE_FALSE":
                case "VALUE_EMPTY_STRING":
                case "VALUE_FLOAT_ZERO":
                case "VALUE_FLOAT_ONE":
                case "VALUE_FLOAT_NEGATIVE_ONE":
                case "COMMON_STRINGS":
                    output.Add("$");
                    break;
            }
            output.Add("v_");
            output.Add(expr.Name);
        }

        protected override void TranslateWhileLoop(List<string> output, WhileLoop whileLoop)
        {
            output.Add(this.CurrentTabIndention);
            output.Add("while ");
            this.TranslateExpression(output, whileLoop.Condition);
            output.Add(this.NL);
            this.CurrentIndention++;
            this.Translate(output, whileLoop.Code);
            this.CurrentIndention--;
            output.Add(this.CurrentTabIndention);
            output.Add("end");
            output.Add(this.NL);
        }

        protected override void TranslateBooleanNot(List<string> output, BooleanNot booleanNot)
        {
            output.Add("!");
            this.TranslateExpression(output, booleanNot.Root);
        }

        protected override void TranslateIfStatement(List<string> output, IfStatement ifStatement)
        {
            output.Add(this.CurrentTabIndention);
            output.Add("if ");
            this.TranslateExpression(output, ifStatement.Condition);
            output.Add(this.NL);
            this.CurrentIndention++;
            this.Translate(output, ifStatement.TrueCode);
            this.CurrentIndention--;

            if (ifStatement.FalseCode.Length > 0)
            {
                output.Add(this.CurrentTabIndention);
                output.Add("else");
                output.Add(this.NL);
                this.CurrentIndention++;
                this.Translate(output, ifStatement.FalseCode);
                this.CurrentIndention--;
            }

            output.Add(this.CurrentTabIndention);
            output.Add("end");
            output.Add(this.NL);
        }

        protected override void TranslateNullConstant(List<string> output, NullConstant nullConstant)
        {
            output.Add("nil");
        }

        protected override void TranslateNegativeSign(List<string> output, NegativeSign negativeSign)
        {
            output.Add("-(");
            this.TranslateExpression(output, negativeSign.Root);
            output.Add(")");
        }

        protected override void TranslateFunctionCall(List<string> output, FunctionCall functionCall)
        {
            this.TranslateExpression(output, functionCall.Root);
            Expression[] args = functionCall.Args;
            if (args.Length > 0)
            {
                output.Add("(");
                for (int i = 0; i < args.Length; ++i)
                {
                    if (i > 0) output.Add(", ");
                    this.TranslateExpression(output, args[i]);
                }
                output.Add(")");
            }
        }

        protected override void TranslateFloatConstant(List<string> output, FloatConstant floatConstant)
        {
            output.Add(Util.FloatToString(floatConstant.Value));
        }

        protected override void TranslateDotStepStruct(List<string> output, DotStepStruct dotStepStruct)
        {
            output.Add(dotStepStruct.RootVar);
            output.Add("[");
            output.Add(dotStepStruct.StructDefinition.IndexByField[dotStepStruct.FieldName].ToString());
            output.Add("]");
        }

        protected override void TranslateIntegerConstant(List<string> output, IntegerConstant intConstant)
        {
            output.Add(intConstant.Value.ToString());
        }

        protected override void TranslateStringConstant(List<string> output, StringConstant stringConstant)
        {
            output.Add(Util.ConvertStringValueToCode(stringConstant.Value));
        }

        protected override void TranslateBreakStatement(List<string> output, BreakStatement breakStatement)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateStructInstance(List<string> output, StructInstance structInstance)
        {
            output.Add("[");
            Expression[] args = structInstance.Args;
            for (int i = 0; i < args.Length; ++i)
            {
                if (i > 0) output.Add(", ");
                this.TranslateExpression(output, args[i]);
            }
            output.Add("]");
        }

        protected override void TranslateSwitchStatement(List<string> output, SwitchStatement switchStatement)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateReturnStatement(List<string> output, ReturnStatement returnStatement)
        {
            output.Add(this.CurrentTabIndention);
            output.Add("return ");
            if (returnStatement.Expression == null)
            {
                output.Add("nil");
            }
            else
            {
                this.TranslateExpression(output, returnStatement.Expression);
            }
            output.Add(this.NL);
        }

        protected override void TranslateBooleanConstant(List<string> output, BooleanConstant booleanConstant)
        {
            output.Add(booleanConstant.Value ? "true" : "false");
        }

        protected override void TranslateFunctionDefinition(List<string> output, FunctionDefinition functionDef)
        {
            output.Add(this.NL);
            output.Add(this.CurrentTabIndention);
            output.Add("def v_");
            output.Add(functionDef.NameToken.Value);
            output.Add("(");
            Token[] args = functionDef.ArgNames;
            for (int i = 0; i < args.Length; ++i)
            {
                if (i > 0) output.Add(", ");
                output.Add("v_");
                output.Add(args[i].Value);
            }
            output.Add(")\n");
            this.CurrentIndention++;

            this.Translate(output, functionDef.Code);

            this.CurrentIndention--;
            output.Add(this.CurrentTabIndention);
            output.Add("end");
            output.Add(this.NL);
        }

        protected override void TranslateBooleanCombination(List<string> output, BooleanCombination booleanCombination)
        {
            for (int i = 0; i < booleanCombination.Expressions.Length; ++i)
            {
                if (i > 0)
                {
                    output.Add(" ");
                    output.Add(booleanCombination.Ops[i - 1].Value);
                }

                this.TranslateExpression(output, booleanCombination.Expressions[i]);
            }
        }

        protected override void TranslateExpressionAsExecutable(List<string> output, ExpressionAsExecutable exprAsExec)
        {
            output.Add(this.CurrentTabIndention);
            this.TranslateExpression(output, exprAsExec.Expression);
            output.Add(this.NL);
        }

        protected override void TranslateSwitchStatementUnsafeBlotchy(List<string> output, SwitchStatementUnsafeBlotchy switchStatement)
        {
            output.Add(this.CurrentTabIndention);
            output.Add("case ");
            this.TranslateExpression(output, switchStatement.Condition);
            output.Add(this.NL);
            //this.CurrentIndention++;
            SwitchStatement.Chunk[] chunks = switchStatement.OriginalSwitchStatement.Chunks; ;
            for (int i = 0; i < chunks.Length; ++i)
            {
                SwitchStatement.Chunk chunk = chunks[i];

                string whenLabel;
                if (chunk.Cases[0] == null)
                {
                    whenLabel = "else";
                }
                else
                {
                    List<string> literals = new List<string>();
                    foreach (Expression caseLiteral in chunk.Cases)
                    {
                        if (caseLiteral is StringConstant)
                        {
                            literals.Add(Util.ConvertStringValueToCode(((StringConstant)caseLiteral).Value));
                        }
                        else
                        {
                            literals.Add(((IntegerConstant)caseLiteral).Value.ToString());
                        }
                    }
                    whenLabel = "when " + string.Join(", ", literals);
                }

                output.Add(this.CurrentTabIndention);
                output.Add(whenLabel);
                output.Add(this.NL);
                this.CurrentIndention++;
                this.Translate(output, switchStatement.CodeMapping[i]);
                this.CurrentIndention--;
            }
            //this.CurrentIndention--;
            output.Add(this.CurrentTabIndention);
            output.Add("end");
            output.Add(this.NL);
        }

        protected override void TranslateSwitchStatementContinuousSafe(List<string> output, SwitchStatementContinuousSafe switchStatement)
        {
            output.Add(this.CurrentTabIndention);
            output.Add("case ");
            this.TranslateExpression(output, switchStatement.Condition);
            output.Add(this.NL);
            this.CurrentIndention++;
            Dictionary<int, Executable[]> codeByCase = switchStatement.CaseCode;
            foreach (int key in codeByCase.Keys)
            {
                output.Add(this.CurrentTabIndention);
                output.Add("when " + key);
                output.Add(this.NL);
                this.CurrentIndention++;
                this.Translate(output, codeByCase[key]);
                this.CurrentIndention--;
            }
            this.CurrentIndention--;
            output.Add(this.CurrentTabIndention);
            output.Add("end");
            output.Add(this.NL);
        }

        protected override string TabString { get { return "    "; } }
        public override string NL { get { return "\n"; } }

        protected override void TranslateStructDefinition(List<string> output, StructDefinition structDef)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateEnumDefinition(List<string> output, EnumDefinition enumDef)
        {
            throw new NotImplementedException();
        }
    }
}
