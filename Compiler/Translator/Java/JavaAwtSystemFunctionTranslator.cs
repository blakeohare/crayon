using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon.Translator.Java
{
    internal class JavaAwtSystemFunctionTranslator : JavaSystemFunctionTranslator
    {
        protected override void TranslatePrint(List<string> output, Expression expression, bool isErr)
        {
            if (isErr)
            {
                output.Add("System.err.println(");
            }
            else
            {
                output.Add("System.out.println(");
            }
            this.Translator.TranslateExpression(output, expression);
            output.Add(")");
        }

        protected override void TranslateResourceGetManifest(List<string> output)
        {
            output.Add("AwtTranslationHelper.getResourceManifest()");
        }

        protected override void TranslateIsWindowsProgram(List<string> output)
        {
            output.Add("AwtTranslationHelper.isWindows()");
        }

        protected override void TranslateGetRawByteCodeString(List<string> output)
        {
            output.Add("AwtTranslationHelper.getRawByteCodeString()");
        }

        protected override void TranslateAppDataRoot(List<string> output)
        {
            output.Add("AwtTranslationHelper.getAppDataRoot()");
        }

        protected override void TranslateResourceReadText(List<string> output, Expression path)
        {
            output.Add("AwtTranslationHelper.getTextResource(\"text/\" + ");
            this.Translator.TranslateExpression(output, path);
            output.Add(")");
        }
    }
}