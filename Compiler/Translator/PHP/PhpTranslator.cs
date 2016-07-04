using System.Collections.Generic;
using System.Linq;
using Crayon.ParseTree;

namespace Crayon.Translator.Php
{
    internal class PhpTranslator : CurlyBraceImplementation
    {
        public PhpTranslator() : base(true) { }

        public override string GetAssignmentOp(Assignment assignment)
        {
            string output = assignment.AssignmentOp;
            if (output == ":=") return "= &";
            return output + " ";
        }

        public bool IsTypeReference(string type)
        {
            // I should be embarrassed, but oddly proud of this one-liner.
            // I'll eat my pride when this breaks royally a year down the road.
            return type[0] < 'a' || type[0] > 'z';
        }

        protected override void TranslateAssignment(List<string> output, Assignment assignment)
        {
            output.Add(this.CurrentTabIndention);
            this.TranslateExpression(output, assignment.Target);
            output.Add(" ");
            output.Add(this.GetAssignmentOp(assignment));
            this.TranslateExpression(output, assignment.Value);
            output.Add(";");
            output.Add(this.NL);
        }

        protected override void TranslateDotStepStruct(List<string> output, DotStepStruct dotStepStruct)
        {
            output.Add("$");
            output.Add(dotStepStruct.RootVar);
            output.Add("[");
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
            if (this.IsTypeReference(type))
            {
                output.Add("&");
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
                string argType = annotation == null ? "Object" : annotation.GetSingleArgAsString(null);
                if (this.IsTypeReference(argType))
                {
                    output.Add("&");
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
            output.Add(this.CurrentTabIndention);
            output.Add("global $nullhack;\n");

            foreach (Executable line in functionDef.Code)
            {
                this.Translate(output, line);
            }
            this.CurrentIndention--;
            output.Add(this.CurrentTabIndention);
            output.Add("}\n");
        }

        public void ArgSafeTranslateExpression(List<string> output, Expression expression)
        {
            if (expression is NullConstant)
            {
                output.Add("$nullhack");
            }
            else
            {
                List<string> buffer = new List<string>();
                this.TranslateExpression(buffer, expression);
                string value = string.Join("", buffer);
                if (value.StartsWith("array(") && value.EndsWith(")"))
                {
                    output.Add("array_hack(");
                    output.Add(value);
                    output.Add(")");
                }
                else
                {
                    output.Add(value);
                }
            }
        }

        protected override void TranslateFunctionCall(List<string> output, FunctionCall functionCall)
        {
            Variable func = (Variable)functionCall.Root;
            output.Add(this.GetVariableName(func.Name));
            output.Add("(");
            for (int i = 0; i < functionCall.Args.Length; ++i)
            {
                if (i > 0) output.Add(", ");
                this.ArgSafeTranslateExpression(output, functionCall.Args[i]);
            }
            output.Add(")");
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

