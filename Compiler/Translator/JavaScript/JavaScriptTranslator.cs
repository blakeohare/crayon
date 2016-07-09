using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon.Translator.JavaScript
{
    internal class JavaScriptTranslator : CurlyBraceImplementation
    {
        public JavaScriptTranslator()
            : base(true)
        { }

        protected override void TranslateDotStepStruct(List<string> output, DotStepStruct dotStepStruct)
        {
            output.Add(dotStepStruct.RootVar);
            output.Add("[");
            output.Add(dotStepStruct.StructDefinition.IndexByField[dotStepStruct.FieldName].ToString());
            output.Add("]");
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
            output.Add(";" + this.NL);
        }

        protected override void TranslateFunctionDefinition(List<string> output, FunctionDefinition functionDef)
        {
            output.Add(this.NL);
            output.Add(this.CurrentTabIndention);
            output.Add("function ");
            output.Add("v_" + functionDef.NameToken.Value);
            output.Add("(");
            for (int i = 0; i < functionDef.ArgNames.Length; ++i)
            {
                if (i > 0) output.Add(", ");
                string argName = functionDef.ArgNames[i].Value;
                output.Add("v_" + argName);
            }
            output.Add(") {" + this.NL);
            this.CurrentIndention++;

            foreach (string varName in functionDef.GetVariableDeclarationList())
            {
                output.Add(this.CurrentTabIndention);
                output.Add("var v_" + varName + " = null;" + this.NL);
            }

            Translate(output, functionDef.Code);
            this.CurrentIndention--;
            output.Add(this.CurrentTabIndention);
            output.Add("}" + this.NL);
        }
    }
}
