using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon.Translator.Java
{
    internal class JavaAwtSystemFunctionTranslator : JavaSystemFunctionTranslator
    {
        protected override void TranslateResourceGetManifest(List<string> output)
        {
            output.Add("AwtTranslationHelper.getResourceManifest()");
        }

        protected override void TranslateIsWindowsProgram(List<string> output)
        {
            output.Add("AwtTranslationHelper.isWindows()");
        }
    }
}
