using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon.Translator.CSharp
{
    internal class CSharpXamarinIosSystemFunctionTranslator : CSharpSystemFunctionTranslator
    {
        protected override void TranslatePrint(List<string> output, Expression expression, bool isErr)
        {
            output.Add("System.Console.WriteLine(");
            this.Translator.TranslateExpression(output, expression);
            output.Add(")");
        }

        protected override void TranslateResourceGetManifest(List<string> output)
        {
            output.Add("ResourceReader.GetResourceManifest()");
        }

        protected override void TranslateAppDataRoot(List<string> output)
        {
            output.Add("CsxiTranslationHelper.AppDataRoot");
        }
    }
}
