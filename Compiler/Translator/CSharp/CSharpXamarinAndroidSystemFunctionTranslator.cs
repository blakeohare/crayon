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

        protected override void TranslateHttpRequest(List<string> output, Expression httpRequest, Expression method, Expression url, Expression body, Expression userAgent, Expression contentType, Expression contentLength, Expression headerNameList, Expression headerValueList)
        {
            output.Add("CsxaTranslationHelper.MakeHttpRequestWithHandler(");
            this.Translator.TranslateExpression(output, httpRequest);
            output.Add(", ");
            this.Translator.TranslateExpression(output, method);
            output.Add(", ");
            this.Translator.TranslateExpression(output, url);
            output.Add(", ");
            this.Translator.TranslateExpression(output, body);
            output.Add(", ");
            this.Translator.TranslateExpression(output, userAgent);
            output.Add(", ");
            this.Translator.TranslateExpression(output, contentType);
            output.Add(", ");
            this.Translator.TranslateExpression(output, contentLength);
            output.Add(", ");
            this.Translator.TranslateExpression(output, headerNameList);
            output.Add(", ");
            this.Translator.TranslateExpression(output, headerValueList);
            output.Add(")");
        }

        protected override void TranslateAppDataRoot(List<string> output)
        {
            output.Add("\".\"");
        }

        protected override void TranslateIoCreateDirectory(List<string> output, Expression path)
        {
            output.Add("CsxaTranslationHelper.IoCreateDirectory(");
            this.Translator.TranslateExpression(output, path);
            output.Add(")");
        }

        protected override void TranslateIoCurrentDirectory(List<string> output)
        {
            output.Add("CsxaTranslationHelper.IoCurrentDirectory()");
        }

        protected override void TranslateIoDeleteDirectory(List<string> output, Expression path, Expression isRecursive)
        {
            output.Add("CsxaTranslationHelper.IoDeleteDirectory(");
            this.Translator.TranslateExpression(output, path);
            output.Add(", ");
            this.Translator.TranslateExpression(output, isRecursive);
            output.Add(")");
        }

        protected override void TranslateIoDeleteFile(List<string> output, Expression path, Expression isUserData)
        {
            output.Add("CsxaTranslationHelper.IoDeleteFile(");
            this.Translator.TranslateExpression(output, path);
            output.Add(", ");
            this.Translator.TranslateExpression(output, isUserData);
            output.Add(")");
        }

        protected override void TranslateIoDoesPathExist(List<string> output, Expression canonicalizedPath, Expression directoriesOnly, Expression performCaseCheck, Expression isUserData)
        {
            output.Add("CsxaTranslationHelper.IoPathExists(");
            this.Translator.TranslateExpression(output, canonicalizedPath);
            output.Add(", ");
            this.Translator.TranslateExpression(output, directoriesOnly);
            output.Add(", ");
            this.Translator.TranslateExpression(output, performCaseCheck);
            output.Add(", ");
            this.Translator.TranslateExpression(output, isUserData);
            output.Add(")");
        }

        protected override void TranslateIoFileReadText(List<string> output, Expression path, Expression isUserData)
        {
            output.Add("CsxaTranslationHelper.IoFileReadText(");
            this.Translator.TranslateExpression(output, path);
            output.Add(", ");
            this.Translator.TranslateExpression(output, isUserData);
            output.Add(")");
        }

        protected override void TranslateIoFilesInDirectory(List<string> output, Expression verifiedCanonicalizedPath, Expression isUserData)
        {
            output.Add("CsxaTranslationHelper.IoFilesInDirectory(");
            this.Translator.TranslateExpression(output, verifiedCanonicalizedPath);
            output.Add(", ");
            this.Translator.TranslateExpression(output, isUserData);
            output.Add(")");
        }

        protected override void TranslateIoFileWriteText(List<string> output, Expression path, Expression content, Expression isUserData)
        {
            output.Add("CsxaTranslationHelper.IoFileWriteText(");
            this.Translator.TranslateExpression(output, path);
            output.Add(", ");
            this.Translator.TranslateExpression(output, content);
            output.Add(", ");
            this.Translator.TranslateExpression(output, isUserData);
            output.Add(")");
        }
    }
}
