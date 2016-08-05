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

        protected override void TranslateHttpRequest(List<string> output, Expression httpRequest, Expression method, Expression url, Expression body, Expression userAgent, Expression contentType, Expression contentLength, Expression headerNameList, Expression headerValueList)
        {
            output.Add("AndroidTranslationHelper.makeHttpRequest(");
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

        protected override void TranslateIoCreateDirectory(List<string> output, Expression path)
        {
            output.Add("AndroidTranslationHelper.ioCreateDirectory(");
            this.Translator.TranslateExpression(output, path);
            output.Add(")");
        }

        protected override void TranslateIoCurrentDirectory(List<string> output)
        {
            output.Add("AndroidTranslationHelper.ioCurrentDirectory()");
        }

        protected override void TranslateIoDeleteDirectory(List<string> output, Expression path, Expression isRecursive)
        {
            output.Add("AndroidTranslationHelper.ioDeleteDirectory(");
            this.Translator.TranslateExpression(output, path);
            output.Add(", ");
            this.Translator.TranslateExpression(output, isRecursive);
            output.Add(")");
        }

        protected override void TranslateIoDeleteFile(List<string> output, Expression path, Expression isUserData)
        {
            output.Add("AndroidTranslationHelper.ioDeleteFile(");
            this.Translator.TranslateExpression(output, path);
            output.Add(", ");
            this.Translator.TranslateExpression(output, isUserData);
            output.Add(")");
        }

        protected override void TranslateIoDoesPathExist(List<string> output, Expression canonicalizedPath, Expression directoriesOnly, Expression performCaseCheck, Expression isUserData)
        {
            output.Add("AndroidTranslationHelper.ioDoesPathExist(");
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
            output.Add("AndroidTranslationHelper.ioFileReadText(");
            this.Translator.TranslateExpression(output, path);
            output.Add(", ");
            this.Translator.TranslateExpression(output, isUserData);
            output.Add(")");
        }

        protected override void TranslateIoFilesInDirectory(List<string> output, Expression verifiedCanonicalizedPath, Expression isUserData)
        {
            output.Add("AndroidTranslationHelper.ioFilesInDirectory(");
            this.Translator.TranslateExpression(output, verifiedCanonicalizedPath);
            output.Add(", ");
            this.Translator.TranslateExpression(output, isUserData);
            output.Add(")");
        }

        protected override void TranslateIoFileWriteText(List<string> output, Expression path, Expression content, Expression isUserData)
        {
            output.Add("AndroidTranslationHelper.ioFileWriteText(");
            this.Translator.TranslateExpression(output, path);
            output.Add(", ");
            this.Translator.TranslateExpression(output, content);
            output.Add(", ");
            this.Translator.TranslateExpression(output, isUserData);
            output.Add(")");
        }

        protected override void TranslateResourceReadText(List<string> output, Expression path)
        {
            output.Add("AndroidTranslationHelper.resourceReadText(");
            this.Translator.TranslateExpression(output, path);
            output.Add(")");
        }
    }
}
