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
            if (assignment.HACK_IsVmGlobal)
            {
                output.Add("global ");
            }

            Annotation type = assignment.Target.GetAnnotation("type");
            if (type != null)
            {
                string name = ((Variable)assignment.Target).Name;
                switch (name)
                {
                    // This is now a Core translated reference
                    case "intOutParam":
                    case "floatOutParam":
                    case "stringOutParam":
                        return;
                    default:
                        break;
                }

                output.Add(((StringConstant)type.Args[0]).Value);
                output.Add(" ");
            }
            this.TranslateExpression(output, assignment.Target, false);
            output.Add(" ");
            output.Add(assignment.AssignmentOp);
            output.Add(" ");
            this.TranslateExpression(output, assignment.Value, false);
            output.Add(";");
            output.Add(this.NL);
        }

        protected override void TranslateDotStep(List<string> output, DotStep dotStep)
        {
            if (dotStep.Root is Variable)
            {
                this.TranslateExpression(output, dotStep.Root);
                output.Add(".");
                output.Add(dotStep.StepToken.Value);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        protected override void TranslateDotStepStruct(List<string> output, DotStepStruct dotStepStruct)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateForLoop(List<string> output, ForLoop forLoop)
        {
            if (forLoop.Init.Length != 1 || forLoop.Step.Length != 1)
            {
                throw new NotImplementedException();
            }
            List<string> outputHack = new List<string>();
            output.Add(this.CurrentTabIndention);
            output.Add("for (");
            this.Translate(outputHack, forLoop.Init[0]);
            output.Add(string.Join("", outputHack).Trim().TrimEnd(';'));
            outputHack.Clear();
            output.Add("; ");
            this.TranslateExpression(output, forLoop.Condition, false);
            output.Add("; ");
            this.Translate(outputHack, forLoop.Step[0]);
            output.Add(string.Join("", outputHack).Trim().TrimEnd(';'));
            outputHack.Clear();
            output.Add(") {" + this.NL);
            this.CurrentIndention++;
            this.Translate(output, forLoop.Code);
            this.CurrentIndention--;
            output.Add(this.CurrentTabIndention);
            output.Add("}" + this.NL);
        }

        protected override void TranslateFunctionDefinition(List<string> output, FunctionDefinition functionDef)
        {
            output.Add(((StringConstant)functionDef.GetAnnotation("type").Args[0]).Value);
            output.Add(" ");
            output.Add(functionDef.NameToken.Value);
            output.Add("(");
            for (int i = 0; i < functionDef.ArgNames.Length; ++i)
            {
                if (i > 0) output.Add(", ");
                output.Add(((StringConstant)functionDef.ArgAnnotations[i].Args[0]).Value);
                output.Add(" ");
                output.Add(functionDef.ArgNames[i].Value);
            }
            output.Add(") {");
            output.Add(this.NL);
            this.CurrentIndention++;
            this.Translate(output, functionDef.Code);
            this.CurrentIndention--;
            output.Add("}");
            output.Add(this.NL);
            output.Add(this.NL);
        }

        protected override void TranslateImportInlineStatement(List<string> output, ImportInlineStatement importInline)
        {
            output.Add(this.CurrentTabIndention);
            output.Add("@import");
            if (importInline.LibraryName != null)
            {
                output.Add("_lib");
            }
            output.Add("(\"");
            output.Add(importInline.Path);
            output.Add("\");\n");
        }

        protected override void TranslateInstantiate(List<string> output, Instantiate instantiate)
        {
            output.Add("new ");
            output.Add(instantiate.Name);
            output.Add("(");
            for (int i = 0; i < instantiate.Args.Length; ++i)
            {
                if (i > 0) output.Add(", ");
                this.TranslateExpression(output, instantiate.Args[i]);
            }
            output.Add(")");
        }

        protected override void TranslateStructInstance(List<string> output, StructInstance structInstance)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateStructDefinition(List<string> output, StructDefinition structDef)
        {
            output.Add("struct ");
            output.Add(structDef.Name.Value);
            output.Add(" {\n");
            for (int i = 0; i < structDef.Fields.Length; ++i)
            {
                output.Add("\t");
                output.Add(structDef.TypeStrings[i]);
                output.Add(" ");
                output.Add(structDef.Fields[i].Value);
                output.Add(";\n");
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
            output.Add("const int ");
            output.Add(constStatement.Name);
            output.Add(" = ");
            this.TranslateExpression(output, constStatement.Expression);
            output.Add(";\n\n");
        }

        protected override void TranslateTextReplaceConstant(List<string> output, TextReplaceConstant textReplaceConstnat)
        {
            string name = textReplaceConstnat.Name;
            bool isInteger;
            if (name.StartsWith("TYPE_ID_"))
            {
                isInteger = true;
            }
            else if (name.StartsWith("IS_") || name.Contains("SUPPORTS_") || name.Contains("_SUPPORTED") || name.Contains("_USES_"))
            {
                isInteger = false;
            }
            else
            {
                switch (name)
                {
                    case "INT_IS_FLOOR":
                    case "IMAGE_RESOURCES_YIELD_REQUIRED_BY_PLATFORM":
                        isInteger = false;
                        break;
                    default:
                        throw new Exception("what type is '" + name + "'?");
                }
            }
            output.Add("@");
            output.Add(isInteger ? "ext_integer" : "ext_boolean");
            output.Add("(\"");
            output.Add(name);
            output.Add("\")");
        }

        protected override void TranslateVariable(List<string> output, Variable expr)
        {
            string name = expr.Name;
            switch (name)
            {
                case "intOutParam":
                    name = "Core.IntBuffer16";
                    break;

                case "floatOutParam":
                    name = "Core.FloatBuffer16";
                    break;

                case "stringOutParam":
                    name = "Core.StringBuffer16";
                    break;

                default: break;
            }

            output.Add(name);
        }

        protected override void TranslateReturnStatement(List<string> output, ReturnStatement returnStatement)
        {
            FunctionCall fc = returnStatement.Expression as FunctionCall;
            if (fc != null)
            {
                Variable fp = fc.Root as Variable;
                if (fp != null && fp.Name == "suspendInterpreter")
                {
                    output.Add(this.CurrentTabIndention);
                    output.Add("Core.VmSuspend()");
                    output.Add(this.NL);
                    return;
                }
            }

            base.TranslateReturnStatement(output, returnStatement);
        }
    }
}
