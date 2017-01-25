using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon.Translator.CSharp
{
    class CSharpOpenTkSystemFunctionTranslator : CSharpSystemFunctionTranslator
    {
        protected override void TranslateResourceGetManifest(List<string> output)
        {
            output.Add("ResourceReader.GetResourceManifest()");
        }
    }
}
