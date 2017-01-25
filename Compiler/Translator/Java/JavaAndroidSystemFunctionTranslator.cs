using System;
using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon.Translator.Java
{
    internal class JavaAndroidSystemFunctionTranslator : JavaSystemFunctionTranslator
    {
        protected override void TranslateResourceGetManifest(List<string> output)
        {
            throw new NotImplementedException();
        }

        protected override void TranslateIsWindowsProgram(List<string> output)
        {
            output.Add("AndroidTranslationHelper.isWindows()");
        }
    }
}
