using System;
using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon.Translator.Python
{
    internal class PythonTranslator : AbstractTranslator
    {
        public PythonTranslator()
            : base()
        { }

        public void TranslateListDefinition(List<string> output, ListDefinition list)
        {
            output.Add("[");
            for (int i = 0; i < list.Items.Length; ++i)
            {
                if (i > 0)
                {
                    output.Add(", ");
                }

                this.TranslateExpression(output, list.Items[i]);
            }
            output.Add("]");
        }

        protected override void TranslateDotStepStruct(List<string> output, DotStepStruct dotStepStruct)
        {
            output.Add(dotStepStruct.RootVar);
            output.Add("[");
            output.Add(dotStepStruct.StructDefinition.IndexByField[dotStepStruct.FieldName].ToString());
            output.Add("]");
        }

        protected override void TranslateSwitchStatementContinuousSafe(List<string> output, SwitchStatementContinuousSafe switchStatement)
        {
            SwitchStatementContinuousSafe.SearchTree tree = switchStatement.GenerateSearchTree();
            int switchId = this.GetNextInt();
            string varName = "switch_key_" + switchId;
            output.Add(this.CurrentTabIndention);
            output.Add(varName);
            output.Add(" = ");
            this.TranslateExpression(output, switchStatement.Condition);
            output.Add("\r\n");
            this.GenerateSearchTree(output, tree, varName);
        }

        private void GenerateSearchTree(List<string> output, SwitchStatementContinuousSafe.SearchTree tree, string varName)
        {
            if (tree.Code == null)
            {
                output.Add(this.CurrentTabIndention);
                output.Add("if " + varName + " < " + tree.LessThanThis + ":\r\n");
                this.CurrentIndention++;
                this.GenerateSearchTree(output, tree.Left, varName);
                this.CurrentIndention--;
                output.Add(this.CurrentTabIndention);
                output.Add("else:\r\n");
                this.CurrentIndention++;
                this.GenerateSearchTree(output, tree.Right, varName);
                this.CurrentIndention--;
            }
            else
            {
                this.Translate(output, tree.Code);
            }
        }

        protected override void TranslateSwitchStatementUnsafeBlotchy(List<string> output, SwitchStatementUnsafeBlotchy switchStatement)
        {
            string lookupName = switchStatement.LookupTableName;
            string varName = "switch_key_" + this.GetNextInt();

            if (switchStatement.UsesStrings)
            {
                output.Add(this.CurrentTabIndention);
                output.Add(varName);
                output.Add(" = ");
                output.Add("v_" + lookupName);
                output.Add(".get(");
                this.TranslateExpression(output, switchStatement.Condition);
                output.Add(", " + switchStatement.DefaultCaseId);
                output.Add(")\r\n");
            }
            else
            {
                output.Add(this.CurrentTabIndention);
                output.Add(varName);
                output.Add(" = ");
                output.Add("v_" + lookupName);
                output.Add("[");
                this.TranslateExpression(output, switchStatement.Condition);
                output.Add("]\r\n");
            }

            this.GenerateSearchTree(output, switchStatement.GenerateSearchTree(), varName);
        }

        protected override void TranslateFloatConstant(List<string> output, FloatConstant floatConstant)
        {
            output.Add(Util.FloatToString(floatConstant.Value));
        }

        protected override void TranslateBooleanNot(List<string> output, BooleanNot booleanNot)
        {
            output.Add("not (");
            this.TranslateExpression(output, booleanNot.Root);
            output.Add(")");
        }

        protected override void TranslateBooleanCombination(List<string> output, BooleanCombination booleanCombination)
        {
            output.Add("(");
            this.TranslateExpression(output, booleanCombination.Expressions[0]);
            output.Add(")");

            for (int i = 0; i < booleanCombination.Ops.Length; ++i)
            {
                if (booleanCombination.Ops[i].Value == "&&")
                {
                    output.Add(" and ");
                }
                else
                {
                    output.Add(" or ");
                }

                output.Add("(");
                this.TranslateExpression(output, booleanCombination.Expressions[i + 1]);
                output.Add(")");
            }
        }

        protected override void TranslateNegativeSign(List<string> output, NegativeSign negativeSign)
        {
            output.Add("-(");
            this.TranslateExpression(output, negativeSign.Root);
            output.Add(")");
        }

        protected override void TranslateStringConstant(List<string> output, StringConstant stringConstant)
        {
            output.Add("\"");
            string value = stringConstant.Value;
            foreach (char c in value)
            {
                string nc = "" + c;
                switch (c)
                {
                    case '\\': nc = "\\\\"; break;
                    case '"': nc = "\\\""; break;
                    case '\0': nc = "\\0"; break;
                    case '\t': nc = "\\t"; break;
                    case '\n': nc = "\\n"; break;
                    case '\r': nc = "\\r"; break;
                    default: break;
                }
                output.Add(nc);
            }
            output.Add("\"");
        }

        protected override void TranslateReturnStatement(List<string> output, ReturnStatement returnStatement)
        {
            output.Add(this.CurrentTabIndention);
            output.Add("return");
            if (returnStatement.Expression != null)
            {
                output.Add(" ");
                TranslateExpression(output, returnStatement.Expression);
            }
            output.Add("\r\n");
        }

        protected override void TranslateBreakStatement(List<string> output, BreakStatement breakStatement)
        {
            output.Add(this.CurrentTabIndention);
            output.Add("break\r\n");
        }

        protected override void TranslateDotStep(List<string> output, DotStep dotStep)
        {
            output.Add("(");
            TranslateExpression(output, dotStep.Root);
            output.Add(")");
            output.Add(".f_");
            output.Add(dotStep.StepToken.Value);
        }

        protected override void TranslateSwitchStatement(List<string> output, SwitchStatement switchStatement)
        {
            int switchId = this.GetNextInt();

            if (switchStatement.UsesStrings)
            {
                throw new NotImplementedException("Create a string dictionary lookup");
            }
            else if (switchStatement.IsSafe && switchStatement.IsContinuous)
            {
                this.TranslateSwitchStatementAsBinaryDecisions(output, switchStatement, switchId, false);
            }
            else
            {
                this.TranslateSwitchStatementAsElifChain(output, switchStatement, switchId);
            }
        }

        private void TranslateSwitchStatementAsBinaryDecisions(List<string> output, SwitchStatement switchStatement, int switchId, bool doSafeMapping)
        {

        }

        private void TranslateSwitchStatementAsElifChain(List<string> output, SwitchStatement switchStatement, int switchId)
        {
            output.Add(this.CurrentTabIndention);
            output.Add("switch_loopy_" + switchId + " = 1\r\n");
            output.Add(this.CurrentTabIndention);
            output.Add("while switch_loopy_" + switchId + " == 1:\r\n");
            this.CurrentIndention++;
            output.Add(this.CurrentTabIndention);
            output.Add("switch_loopy_" + switchId + " = 0\r\n");

            Expression firstCase = switchStatement.Chunks[0].Cases[0];
            if (firstCase == null)
            {
                // WAT
                throw new Exception("This should have been optimized out.");
            }
            else if (firstCase is IntegerConstant)
            {
                // TODO: do this for real
                // There are several optimziations that can be done.
                // if it's a small switch, just use if/elif/else
                // if it's all positive and condensed and ordered, use a binary search with if/else
                // if they're sparse, are they super sparse?
                // if they're chunked not in order, normalize them with a static int-to-int array
                // if there are any weird breaks that aren't at the end, you need to put the whole thing in a one-time loop. e.g. "for ignored in (1,):"
                // now I feel sad. Here's a big-ass if/elif/else chain.
                string varName = "switch_expr_" + switchId;

                output.Add(this.CurrentTabIndention);
                output.Add(varName);
                output.Add(" = ");
                TranslateExpression(output, switchStatement.Condition);
                output.Add("\r\n");

                /*
                 * This is probably best
                 * _switch_statement_lookup_27 = {
                 *   case_expr1: 1,
                 *   case_expr2a: 2,
                 *   case_expr2b: 2,
                 *   case_expr3: 3
                 * }
                 * ...
                 * _ignored_switch_loopy_thing_27 = 1
                 * while _ignored_switch_loopy_thing_27 == =1:
                 *   _ignored_switch_loopy_thing = 0
                 *   _switch_expr_27 = _switch_statement_lookup_27.get(arbitrary_expression, 4) # 4 is the default case
                 *   if _switch_expr_27 < 3:
                 *     if _switch_expr_27 < 2:
                 *       code for case 1
                 *     else:
                 *       code for case 2
                 *   else:
                 *     if _switch_expr_27 < 4:
                 *       code for case 3
                 *     else:
                 *       code for case 4 (default)
                 *
                 * This way you just leave the breaks as they are.
                 * Don't try to use an array instead of a dictionary, though. Because that will cause the switch to crash if a crazy value is passed in.
                 */

                // This isn't even right. If a default is first, this will blow up.
                for (int i = 0; i < switchStatement.Chunks.Length; ++i)
                {
                    SwitchStatement.Chunk chunk = switchStatement.Chunks[i];
                    bool conditionRequired = true;
                    output.Add(this.CurrentTabIndention);
                    if (i == 0)
                    {
                        output.Add("if ");
                    }
                    else if (chunk.Cases[chunk.Cases.Length - 1] == null)
                    {
                        output.Add("else:\r\n");
                        conditionRequired = false;
                    }
                    else
                    {
                        output.Add("elif ");
                    }

                    if (conditionRequired)
                    {
                        bool parens = chunk.Cases.Length > 1;

                        if (parens)
                        {
                            output.Add("(");
                        }

                        for (int j = 0; j < chunk.Cases.Length; ++j)
                        {
                            if (j > 0)
                            {
                                output.Add(") or (");
                            }

                            output.Add(varName);
                            output.Add(" == ");
                            TranslateExpression(output, chunk.Cases[j]);
                        }

                        if (parens)
                        {
                            output.Add(")");
                        }

                        output.Add(":\r\n");
                        this.CurrentIndention++;

                        Translate(output, chunk.Code);

                        if (chunk.Code.Length == 0)
                        {
                            output.Add(this.CurrentTabIndention);
                            output.Add("pass\r\n");
                        }

                        this.CurrentIndention--;
                    }
                    else
                    {
                        this.CurrentIndention++;

                        Translate(output, chunk.Code);

                        if (chunk.Code.Length == 0)
                        {
                            output.Add(this.CurrentTabIndention);
                            output.Add("pass\r\n");
                        }

                        this.CurrentIndention--;
                    }
                }

            }
            else if (firstCase is StringConstant)
            {
                throw new NotImplementedException("switch on string not implemented yet.");
            }
            else
            {
                throw new ParserException(firstCase.FirstToken, "Invalid value for a switch statement case.");
            }

            this.CurrentIndention--;
        }

        protected override void TranslateBooleanConstant(List<string> output, BooleanConstant booleanConstant)
        {
            output.Add(booleanConstant.Value ? "True" : "False");
        }

        protected override void TranslateWhileLoop(List<string> output, WhileLoop whileLoop)
        {
            output.Add(this.CurrentTabIndention);
            output.Add("while ");
            this.TranslateExpression(output, whileLoop.Condition);
            output.Add(":\r\n");
            this.CurrentIndention++;
            this.Translate(output, whileLoop.Code);
            this.CurrentIndention--;
        }

        protected override void TranslateExpressionAsExecutable(List<string> output, ExpressionAsExecutable exprAsExec)
        {
            output.Add(this.CurrentTabIndention);
            TranslateExpression(output, exprAsExec.Expression);
            output.Add("\r\n");
        }

        protected override void TranslateFunctionCall(List<string> output, FunctionCall functionCall)
        {
            TranslateExpression(output, functionCall.Root);
            output.Add("(");
            for (int i = 0; i < functionCall.Args.Length; ++i)
            {
                if (i > 0) output.Add(", ");
                TranslateExpression(output, functionCall.Args[i]);
            }
            output.Add(")");
        }

        protected override void TranslateNullConstant(List<string> output, NullConstant nullConstant)
        {
            output.Add("None");
        }

        protected override void TranslateIntegerConstant(List<string> output, IntegerConstant intConstant)
        {
            int value = intConstant.Value;
            if (value >= 0)
            {
                output.Add("" + value);
            }
            else
            {
                output.Add("(" + value + ")");
            }
        }

        protected override void TranslateStructInstance(List<string> output, StructInstance structInstance)
        {
            output.Add("[");
            for (int i = 0; i < structInstance.Args.Length; ++i)
            {
                if (i > 0) output.Add(", ");
                this.TranslateExpression(output, structInstance.Args[i]);
            }
            output.Add("]");
        }

        protected override void TranslateAssignment(List<string> output, Assignment assignment)
        {
            output.Add(this.CurrentTabIndention);
            this.TranslateExpression(output, assignment.Target);
            output.Add(" ");
            output.Add(this.GetAssignmentOp(assignment));
            output.Add(" ");
            this.TranslateExpression(output, assignment.Value);
            output.Add("\r\n");
        }

        protected override void TranslateIfStatement(List<string> output, ParseTree.IfStatement exec)
        {
            output.Add(this.CurrentTabIndention);
            output.Add("if ");
            TranslateExpression(output, exec.Condition);
            output.Add(":\r\n");
            this.CurrentIndention++;
            if (exec.TrueCode.Length == 0)
            {
                output.Add(this.CurrentTabIndention);
                output.Add("pass\r\n");
            }
            else
            {
                Translate(output, exec.TrueCode);
            }
            this.CurrentIndention--;
            if (exec.FalseCode.Length > 0)
            {
                output.Add(this.CurrentTabIndention);
                output.Add("else:\r\n");
                this.CurrentIndention++;
                Translate(output, exec.FalseCode);
                this.CurrentIndention--;
            }
        }

        protected override void TranslateForLoop(List<string> output, ParseTree.ForLoop exec)
        {
            Translate(output, exec.Init);
            output.Add(this.CurrentTabIndention);
            output.Add("while ");
            TranslateExpression(output, exec.Condition);
            output.Add(":\r\n");
            this.CurrentIndention++;
            Translate(output, exec.Code);
            Translate(output, exec.Step);
            this.CurrentIndention--;
        }

        protected override void TranslateVariable(List<string> output, ParseTree.Variable expr)
        {
            output.Add(this.GetVariableName(expr.Name));
        }

        protected override string TabString { get { return "\t"; } }

        protected override void TranslateFunctionDefinition(List<string> output, ParseTree.FunctionDefinition functionDef)
        {
            output.Add(this.CurrentTabIndention);
            output.Add("def v_");
            output.Add(functionDef.NameToken.Value);
            output.Add("(");
            for (int i = 0; i < functionDef.ArgNames.Length; ++i)
            {
                if (i > 0) output.Add(", ");
                output.Add("v_" + functionDef.ArgNames[i].Value);
            }
            output.Add("):\r\n");
            this.CurrentIndention++;
            Translate(output, functionDef.Code);
            this.CurrentIndention--;
            output.Add("\r\n");
        }
    }
}
