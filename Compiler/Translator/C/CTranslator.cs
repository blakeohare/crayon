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
            Variable variable = assignment.TargetAsVariable;
            Annotation typeAnnotation;
            if (variable != null && variable.Annotations.TryGetValue("type", out typeAnnotation))
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
            throw new NotImplementedException();
        }

        protected override void TranslateFloatConstant(List<string> output, FloatConstant floatConstant)
        {
            output.Add(Util.FloatToString(floatConstant.Value));
        }

        protected override void TranslateForLoop(List<string> output, ForLoop forLoop)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateFunctionCall(List<string> output, FunctionCall functionCall)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateFunctionDefinition(List<string> output, FunctionDefinition functionDef)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateIfStatement(List<string> output, IfStatement ifStatement)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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

        protected override void TranslateSwitchStatement(List<string> output, SwitchStatement switchStatement)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateSwitchStatementContinuousSafe(List<string> output, SwitchStatementContinuousSafe switchStatement)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateSwitchStatementUnsafeBlotchy(List<string> output, SwitchStatementUnsafeBlotchy switchStatement)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateVariable(List<string> output, Variable expr)
        {
            output.Add("v_");
            output.Add(expr.Name);
        }

        protected override void TranslateWhileLoop(List<string> output, WhileLoop whileLoop)
        {
            throw new NotImplementedException();
        }
    }
}
