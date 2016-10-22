using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon.Translator.CSharp
{
    class CSharpXamarinAndroidSystemFunctionTranslator : CSharpSystemFunctionTranslator
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

        protected override void TranslateIsWindowsProgram(List<string> output)
        {
            output.Add("TranslationHelper.AlwaysFalse()");
        }
        
        protected override void TranslateResourceReadText(List<string> output, Expression path)
        {
            output.Add("ResourceReader.ReadTextResource(");
            this.Translator.TranslateExpression(output, path);
            output.Add(")");
        }
    }
}
