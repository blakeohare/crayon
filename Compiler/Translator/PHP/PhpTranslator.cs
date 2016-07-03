using System;
using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon.Translator.Php
{
    internal class PhpTranslator : CurlyBraceImplementation
    {
        public PhpTranslator() : base(true) { }

        protected override void TranslateAssignment(List<string> output, Assignment assignment)
        {
            output.Add(this.CurrentTabIndention);
            this.TranslateExpression(output, assignment.Target);
            output.Add(" " + assignment.AssignmentOpToken.Value + " ");
            this.TranslateExpression(output, assignment.Value);
            output.Add(";");
            output.Add(this.NL);
        }

        protected override void TranslateDotStepStruct(List<string> output, DotStepStruct dotStepStruct)
        {
            output.Add("$v_");
            output.Add(dotStepStruct.RootVar);
            output.Add("[");
            output.Add(dotStepStruct.StructDefinition.IndexByField[dotStepStruct.FieldName].ToString());
            output.Add("]");
        }

        protected override void TranslateFunctionDefinition(List<string> output, FunctionDefinition functionDef)
        {
            output.Add(this.CurrentTabIndention);
            output.Add("\nfunction v_");
            output.Add(functionDef.NameToken.Value);
            output.Add("(");
            for (int i = 0; i < functionDef.ArgNames.Length; ++i)
            {
                if (i > 0) output.Add(", ");
                output.Add("$v_" + functionDef.ArgNames[i].Value);
            }
            output.Add(") {\n");
            this.CurrentIndention++;
            foreach (Executable line in functionDef.Code)
            {
                this.Translate(output, line);
            }
            this.CurrentIndention--;
            output.Add(this.CurrentTabIndention);
            output.Add("}\n");
        }

        protected override void TranslateStructInstance(List<string> output, StructInstance structInstance)
        {
            output.Add("array(");
            for (int i = 0; i < structInstance.Args.Length; ++i)
            {
                if (i > 0) output.Add(", ");
                this.TranslateExpression(output, structInstance.Args[i]);
            }
            output.Add(")");
        }

        protected override void TranslateVariable(List<string> output, Variable expr)
        {
            output.Add("$");
            output.Add(this.GetVariableName(expr.Name));
        }
    }
}

