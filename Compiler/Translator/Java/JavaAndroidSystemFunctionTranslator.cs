using System;
using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon.Translator.Java
{
    internal class JavaAndroidSystemFunctionTranslator : JavaSystemFunctionTranslator
    {
        protected override void TranslatePrint(List<string> output, Expression expression, bool isErr)
        {
            output.Add("android.util.Log.d(\"\", ");
            this.Translator.TranslateExpression(output, expression);
            output.Add(")");
        }

        protected override void TranslateResourceGetManifest(List<string> output)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateIsWindowsProgram(List<string> output)
        {
            output.Add("AndroidTranslationHelper.isWindows()");
        }

        protected override void TranslateGetRawByteCodeString(List<string> output)
        {
            output.Add("AndroidTranslationHelper.getRawByteCodeString()");
        }

        protected override void TranslateAppDataRoot(List<string> output)
        {
            output.Add("AndroidTranslationHelper.getAppDataRoot()");
        }

        protected override void TranslateResourceReadText(List<string> output, Expression path)
        {
            output.Add("AndroidTranslationHelper.resourceReadText(");
            this.Translator.TranslateExpression(output, path);
            output.Add(")");
        }
    }
}
