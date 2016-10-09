using System.Collections.Generic;
using System.Linq;
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
            output.Add(" ");
            output.Add(this.GetAssignmentOp(assignment));
            output.Add(" ");
            this.TranslateExpression(output, assignment.Value);
            output.Add(";");
            output.Add(this.NL);
        }

        protected override void TranslateBinaryOpSyntax(List<string> output, string tokenValue)
        {
            switch (tokenValue)
            {
                case "==": tokenValue = "==="; break;
                case "!=": tokenValue = "!=="; break;
                default: break;
            }
            output.Add(tokenValue);
        }

        protected override void TranslateDotStepStruct(List<string> output, DotStepStruct dotStepStruct)
        {
            output.Add("$");
            output.Add(dotStepStruct.RootVar);
            output.Add("->r[");
            output.Add(dotStepStruct.StructDefinition.IndexByField[dotStepStruct.FieldName].ToString());
            output.Add("]");
        }

        protected override void TranslateFunctionDefinition(List<string> output, FunctionDefinition functionDef)
        {
            output.Add(this.CurrentTabIndention);
            output.Add("\nfunction ");
            Annotation returnTypeAnnotation = functionDef.GetAnnotation("type");
            if (returnTypeAnnotation == null)
            {
                throw new ParserException(functionDef.FirstToken, "Need return type.");
            }
            string type = returnTypeAnnotation.GetSingleArgAsString(null);
            if (type == null)
            {
                throw new ParserException(functionDef.FirstToken, "Need return type.");
            }
            output.Add("v_");
            output.Add(functionDef.NameToken.Value);
            output.Add("(");
            for (int i = 0; i < functionDef.ArgNames.Length; ++i)
            {
                if (i > 0) output.Add(", ");
                Annotation annotation = functionDef.ArgAnnotations[i];
                if (annotation == null)
                {
                    throw new ParserException(functionDef.FirstToken, "Arg needs a type.");
                }
                output.Add("$v_" + functionDef.ArgNames[i].Value);
            }
            output.Add(") {\n");
            this.CurrentIndention++;

            HashSet<Variable> variablesUsed = new HashSet<Variable>();
            foreach (Executable line in functionDef.Code)
            {
                line.GetAllVariablesReferenced(variablesUsed);
            }

            foreach (string variable in variablesUsed
                .Select<Variable, string>(v => v.Name)
                .Where<string>(s => s.ToUpper() == s)
                .Distinct<string>()
                .OrderBy<string, string>(s => s))
            {
                output.Add(this.CurrentTabIndention);
                output.Add("global $v_");
                output.Add(variable);
                output.Add(";\n");
            }

            foreach (Executable line in functionDef.Code)
            {
                this.Translate(output, line);
            }
            this.CurrentIndention--;
            output.Add(this.CurrentTabIndention);
            output.Add("}\n");
        }

        protected override void TranslateFunctionCall(List<string> output, FunctionCall functionCall)
        {
            Variable func = (Variable)functionCall.Root;
            output.Add(this.GetVariableName(func.Name));
            output.Add("(");
            for (int i = 0; i < functionCall.Args.Length; ++i)
            {
                if (i > 0) output.Add(", ");
                this.TranslateExpression(output, functionCall.Args[i]);
            }
            output.Add(")");
        }

        protected override void TranslateStructInstance(List<string> output, StructInstance structInstance)
        {
            output.Add("new Rf(array(");
            for (int i = 0; i < structInstance.Args.Length; ++i)
            {
                if (i > 0) output.Add(", ");
                this.TranslateExpression(output, structInstance.Args[i]);
            }
            output.Add("))");
        }

        protected override void TranslateVariable(List<string> output, Variable expr)
        {
            output.Add("$");
            output.Add(this.GetVariableName(expr.Name));
        }
    }
}
