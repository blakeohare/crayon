using System;
using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon.Translator.Java
{
    class JavaTranslator : CurlyBraceImplementation
    {
        public JavaTranslator() : base(true) { }

        public JavaPlatform JavaPlatform { get { return (JavaPlatform)this.Platform; } }

        protected override void TranslateAssignment(List<string> output, ParseTree.Assignment assignment)
        {
            output.Add(this.CurrentTabIndention);
            Expression target = assignment.Target;

            if (target is Variable && ((Variable)target).IsStatic)
            {
                output.Add("public static ");
            }

            Annotation typeAnnotation = target.GetAnnotation("type");

            if (typeAnnotation != null)
            {
                string type = this.JavaPlatform.GetTypeStringFromAnnotation(
                    typeAnnotation.FirstToken,
                    typeAnnotation.GetSingleArgAsString(null),
                    false, false);
                output.Add(type);
                output.Add(" ");
            }

            this.TranslateExpression(output, target);
            output.Add(" ");
            output.Add(this.GetAssignmentOp(assignment));
            output.Add(" ");
            this.TranslateExpression(output, assignment.Value);
            output.Add(";");
            output.Add(this.NL);
        }

        protected override void TranslateFunctionDefinition(List<string> output, ParseTree.FunctionDefinition functionDef)
        {
            Annotation returnType = functionDef.GetAnnotation("type");
            string type = returnType == null ? "Object" : this.JavaPlatform.GetTypeStringFromString(returnType.GetSingleArgAsString(null), false, false);

            output.Add(this.CurrentTabIndention);
            output.Add("public static ");
            output.Add(type);
            output.Add(" v_");
            output.Add(functionDef.NameToken.Value);
            output.Add("(");
            for (int i = 0; i < functionDef.ArgNames.Length; ++i)
            {
                if (i > 0)
                {
                    output.Add(", ");
                }
                Annotation annotation = functionDef.ArgAnnotations[i];
                string argType = annotation == null ? "Object" : annotation.GetSingleArgAsString(null);
                output.Add(this.JavaPlatform.GetTypeStringFromString(argType, false, false));
                output.Add(" v_");
                output.Add(functionDef.ArgNames[i].Value);
            }
            output.Add(") {");
            output.Add(this.NL);

            this.CurrentIndention++;
            Executable[] code = functionDef.Code;
            if (functionDef.GetAnnotation("omitReturn") != null)
            {
                Executable[] newCode = new Executable[code.Length - 1];
                Array.Copy(code, newCode, newCode.Length);
                code = newCode;
            }
            this.Translate(output, code);
            this.CurrentIndention--;

            output.Add(this.CurrentTabIndention);
            output.Add("}");
            output.Add(this.NL);
        }

        protected override void TranslateDotStepStruct(List<string> output, DotStepStruct dotStepStruct)
        {
            output.Add(dotStepStruct.RootVar);
            output.Add(".");
            output.Add(dotStepStruct.FieldName);
        }

        protected override void TranslateStructInstance(List<string> output, ParseTree.StructInstance structInstance)
        {
            output.Add("new ");
            output.Add(structInstance.NameToken.Value);
            output.Add("(");
            for (int i = 0; i < structInstance.Args.Length; ++i)
            {
                if (i > 0) output.Add(", ");
                this.TranslateExpression(output, structInstance.Args[i]);
            }
            output.Add(")");
        }
    }
}
