using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon.Translator.CSharp
{
    class CSharpXamarinAndroidSystemFunctionTranslator : CSharpSystemFunctionTranslator
    {
        protected override void TranslateResourceGetManifest(List<string> output)
        {
            output.Add("ResourceReader.GetResourceManifest()");
        }

        protected override void TranslateIsWindowsProgram(List<string> output)
        {
            output.Add("TranslationHelper.AlwaysFalse()");
        }
    }
}
