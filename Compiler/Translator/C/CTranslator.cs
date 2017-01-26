using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crayon.ParseTree;

namespace Crayon.Translator.C
{
    internal class CTranslator : AbstractTranslator
    {
        protected override string TabString { get { return "\t"; } }

        private COpenGlPlatform CPlatform { get { return (COpenGlPlatform)this.Platform; } }

        protected override void TranslateAssignment(List<string> output, Assignment assignment)
        {
            output.Add(this.CurrentTabIndention);
            Variable variable = assignment.TargetAsVariable;
            Annotation typeAnnotation;
            if (variable != null && variable.Annotations != null && variable.Annotations.TryGetValue("type", out typeAnnotation))
            {
                string variableDeclaration = this.CPlatform.GetTypeStringFromAnnotation(new AnnotatedType(typeAnnotation));
                output.Add(variableDeclaration);
                output.Add(" ");
            }
            this.TranslateExpression(output, assignment.Target);
            output.Add(" ");
            output.Add(assignment.AssignmentOp);
            output.Add(" ");
            this.TranslateExpression(output, assignment.Value);
            output.Add(";");
            output.Add(this.NL);
        }

        protected override void TranslateExpressionAsExecutable(List<string> output, ExpressionAsExecutable exprAsExec)
        {
            output.Add(this.CurrentTabIndention);
            TranslateExpression(output, exprAsExec.Expression);
            output.Add(";" + this.NL);
        }

        protected override void TranslateBooleanCombination(List<string> output, BooleanCombination booleanCombination)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateBooleanConstant(List<string> output, BooleanConstant booleanConstant)
        {
            output.Add(booleanConstant.Value ? "TRUE" : "FALSE");
        }

        protected override void TranslateBooleanNot(List<string> output, BooleanNot booleanNot)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateBreakStatement(List<string> output, BreakStatement breakStatement)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateDotStep(List<string> output, DotStep dotStep)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateDotStepStruct(List<string> output, DotStepStruct dotStepStruct)
        {
            output.Add(dotStepStruct.RootVar);
            output.Add("->");
            output.Add(dotStepStruct.FieldName);
        }

        protected override void TranslateFloatConstant(List<string> output, FloatConstant floatConstant)
        {
            output.Add(Util.FloatToString(floatConstant.Value));
        }

        protected override void TranslateForLoop(List<string> output, ForLoop forLoop)
        {
            this.Translate(output, forLoop.Init);
            output.Add(this.CurrentTabIndention);
            output.Add("while (");
            this.TranslateExpression(output, forLoop.Condition);
            output.Add(")");
            output.Add(this.NL);
            output.Add(this.CurrentTabIndention);
            output.Add("{");
            output.Add(this.NL);
            this.CurrentIndention++;
            this.Translate(output, forLoop.Code);
            this.Translate(output, forLoop.Step);
            this.CurrentIndention--;
            output.Add(this.CurrentTabIndention);
            output.Add("}" + this.NL);
        }

        protected override void TranslateFunctionCall(List<string> output, FunctionCall functionCall)
        {
            this.TranslateExpression(output, functionCall.Root);
            output.Add("(");
            Expression[] args = functionCall.Args;
            for (int i = 0; i < args.Length; ++i)
            {
                if (i > 0) output.Add(", ");
                this.TranslateExpression(output, args[i]);
            }
            output.Add(")");
        }

        protected override void TranslateFunctionDefinition(List<string> output, FunctionDefinition functionDef)
        {
            output.Add(this.CPlatform.GetFunctionSignature(functionDef));
            output.Add("\n");
            output.Add("{\n");
            this.CurrentIndention++;
            this.Translate(output, functionDef.Code);
            this.CurrentIndention--;
            output.Add("}\n\n");
        }

        protected override void TranslateIfStatement(List<string> output, IfStatement ifStatement)
        {
            output.Add(this.CurrentTabIndention);
            output.Add("if (");
            this.TranslateExpression(output, ifStatement.Condition);
            output.Add(")");
            output.Add(this.NL);
            output.Add(this.CurrentTabIndention);
            output.Add("{");
            output.Add(this.NL);
            this.CurrentIndention++;
            this.Translate(output, ifStatement.TrueCode);
            this.CurrentIndention--;

            output.Add(this.CurrentTabIndention);
            output.Add("}");
            if (ifStatement.FalseCode.Length > 0)
            {
                output.Add(this.NL);
                output.Add(this.CurrentTabIndention);
                output.Add("else");
                output.Add(this.NL);
                output.Add(this.CurrentTabIndention);
                output.Add("{");
                output.Add(this.NL);

                this.CurrentIndention++;
                this.Translate(output, ifStatement.FalseCode);
                this.CurrentIndention--;
                output.Add(this.CurrentTabIndention);
                output.Add("}");
            }
            output.Add(this.NL);
        }

        protected override void TranslateIntegerConstant(List<string> output, IntegerConstant intConstant)
        {
            output.Add(intConstant.Value.ToString());
        }

        protected override void TranslateNegativeSign(List<string> output, NegativeSign negativeSign)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateNullConstant(List<string> output, NullConstant nullConstant)
        {
            output.Add("NULL");
        }

        protected override void TranslateReturnStatement(List<string> output, ReturnStatement returnStatement)
        {
            output.Add(this.CurrentTabIndention);
            if (returnStatement.Expression == null)
            {
                output.Add("return;");
            }
            else
            {
                output.Add("return ");
                this.TranslateExpression(output, returnStatement.Expression);
                output.Add(";");
            }
            output.Add(this.NL);
        }

        protected override void TranslateStringConstant(List<string> output, StringConstant stringConstant)
        {
            output.Add("String_new(");
            output.Add(Util.ConvertStringValueToCode(stringConstant.Value));
            output.Add(")");
        }

        protected override void TranslateStructInstance(List<string> output, StructInstance structInstance)
        {
            Expression[] args = structInstance.Args;
            string cType = this.CPlatform.GetTypeStringFromAnnotation(structInstance.NameToken, structInstance.NameToken.Value);
            cType = cType.TrimEnd('*');
            string constructorName = cType + "_new";
            if (cType == "Value")
            {
                IntegerConstant ic = args[0] as IntegerConstant;
                if (ic != null)
                {
                    switch (ic.Value)
                    {
                        case (int)Types.BOOLEAN:
                        case (int)Types.INTEGER:
                            constructorName = "Value_newInt";
                            break;
                        case (int)Types.FLOAT:
                            constructorName = "Value_newFloat";
                            break;
                    }
                }
            }
            output.Add(constructorName);
            output.Add("(");
            for (int i = 0; i < args.Length; ++i)
            {
                if (i > 0) output.Add(", ");
                this.TranslateExpression(output, args[i]);
            }
            output.Add(")");
        }

        protected override void TranslateSwitchStatementContinuousSafe(List<string> output, SwitchStatementContinuousSafe switchStatement)
        {
            foreach (SwitchStatement.Chunk chunks in switchStatement.OriginalSwitchStatement.Chunks)
            {
                if (chunks.Cases.Length > 1) throw new Exception("Not a continuous switch statement.");
                if (chunks.Cases[0] == null) throw new Exception("Not a safe switch statement.");
            }

            this.TranslateSwitchStatement(output, switchStatement.OriginalSwitchStatement);
        }

        protected override void TranslateSwitchStatementUnsafeBlotchy(List<string> output, SwitchStatementUnsafeBlotchy switchStatement)
        {
            this.TranslateSwitchStatement(output, switchStatement.OriginalSwitchStatement);
        }

        protected override void TranslateSwitchStatement(List<string> output, SwitchStatement switchStatement)
        {
            output.Add(this.CurrentTabIndention);
            output.Add("switch (");
            this.TranslateExpression(output, switchStatement.Condition);
            output.Add(") {" + this.NL);
            this.CurrentIndention++;

            foreach (SwitchStatement.Chunk chunk in switchStatement.Chunks)
            {
                foreach (Expression caseExpr in chunk.Cases)
                {
                    output.Add(this.CurrentTabIndention);
                    if (caseExpr == null)
                    {
                        output.Add("default:" + this.NL);
                    }
                    else
                    {
                        output.Add("case ");
                        TranslateExpression(output, caseExpr);
                        output.Add(":" + this.NL);
                    }

                    this.CurrentIndention++;

                    Translate(output, chunk.Code);

                    this.CurrentIndention--;
                }
            }

            this.CurrentIndention--;
            output.Add(this.CurrentTabIndention);
            output.Add("}" + this.NL);
        }

        protected override void TranslateVariable(List<string> output, Variable expr)
        {
            output.Add("v_");
            output.Add(expr.Name);
        }

        protected override void TranslateWhileLoop(List<string> output, WhileLoop whileLoop)
        {
            output.Add(this.CurrentTabIndention);
            output.Add("while (");
            this.TranslateExpression(output, whileLoop.Condition);
            output.Add(")");
            output.Add(this.NL);
            output.Add(this.CurrentTabIndention);
            output.Add("{");
            output.Add(this.NL);
            this.CurrentIndention++;
            this.Translate(output, whileLoop.Code);
            this.CurrentIndention--;
            output.Add(this.CurrentTabIndention);
            output.Add("}" + this.NL);
        }
    }
}
