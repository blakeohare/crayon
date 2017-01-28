using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crayon.ParseTree;

namespace Crayon.Translator.Pastel
{
    class PastelTranslator : CurlyBraceImplementation
    {
        public PastelTranslator() : base(true) { }

        protected override void TranslateAssignment(List<string> output, Assignment assignment)
        {
            output.Add(this.CurrentTabIndention);
            Annotation type = assignment.Target.GetAnnotation("type");
            if (type != null)
            {
                output.Add(((StringConstant)type.Args[0]).Value);
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

        protected override void TranslateDotStepStruct(List<string> output, DotStepStruct dotStepStruct)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateFunctionDefinition(List<string> output, FunctionDefinition functionDef)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateStructInstance(List<string> output, StructInstance structInstance)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateStructDefinition(List<string> output, StructDefinition structDef)
        {
            output.Add("struct ");
            output.Add(structDef.Name.Value);
            output.Add("{\n");
            for (int i = 0; i < structDef.Fields.Length; ++i)
            {
                output.Add("\t");
                output.Add(structDef.TypeStrings[i]);
                output.Add(" ");
                output.Add(structDef.Fields[i].Value);
                output.Add(";");
            }
            output.Add("}\n\n");
        }

        protected override void TranslateEnumDefinition(List<string> output, EnumDefinition enumDef)
        {
            output.Add("enum ");
            output.Add(enumDef.Name);
            output.Add(" {\n");
            for (int i = 0; i < enumDef.Items.Length; ++i)
            {
                output.Add("\t");
                output.Add(enumDef.Items[i].Value);
                output.Add(" = ");
                Expression value = enumDef.Values[i];
                if (value == null)
                {
                    output.Add("" + (i + 1));
                }
                else
                {
                    this.TranslateExpression(output, value);
                }
                output.Add(",\n");
            }
            output.Add("}\n\n");
        }

        protected override void TranslateConstStatement(List<string> output, ConstStatement constStatement)
        {
            output.Add("const ");
            output.Add(constStatement.Name);
            output.Add(" = ");
            this.TranslateExpression(output, constStatement.Expression);
            output.Add(";");
        }

        protected override void TranslateTextReplaceConstant(List<string> output, TextReplaceConstant textReplaceConstnat)
        {
            output.Add("%%%");
            output.Add(textReplaceConstnat.Name);
            output.Add("%%%");
        }
    }
}
