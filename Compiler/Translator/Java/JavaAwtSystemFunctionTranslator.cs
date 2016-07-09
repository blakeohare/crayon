using System;
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

        protected override void TranslateHttpRequest(List<string> output, Expression httpRequest, Expression method, Expression url, Expression body, Expression userAgent, Expression contentType, Expression contentLength, Expression headerNameList, Expression headerValueList)
        {
            output.Add("AwtTranslationHelper.makeHttpRequest(");
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
            output.Add("AwtTranslationHelper.isWindows()");
        }

        protected override void TranslateImageCreateFlippedCopyOfNativeBitmap(List<string> output, Expression image, Expression flipX, Expression flipY)
        {
            output.Add("AwtTranslationHelper.flipImage(");
            this.Translator.TranslateExpression(output, image);
            output.Add(", ");
            this.Translator.TranslateExpression(output, flipX);
            output.Add(", ");
            this.Translator.TranslateExpression(output, flipY);
            output.Add(")");
        }

        protected override void TranslateGetRawByteCodeString(List<string> output)
        {
            output.Add("AwtTranslationHelper.getRawByteCodeString()");
        }

        protected override void TranslateAppDataRoot(List<string> output)
        {
            output.Add("AwtTranslationHelper.getAppDataRoot()");
        }

        protected override void TranslateImageImagetteFlushToNativeBitmap(List<string> output, Expression imagette)
        {
            output.Add("AwtTranslationHelper.flushImagetteToBitmap(");
            this.Translator.TranslateExpression(output, imagette);
            output.Add(")");
        }

        protected override void TranslateImageInitiateAsyncDownloadOfResource(List<string> output, Expression path)
        {
            // Client-side Java loads resources synchronously.
            throw new InvalidOperationException();
        }

        protected override void TranslateImageNativeBitmapHeight(List<string> output, Expression bitmap)
        {
            output.Add("((java.awt.image.BufferedImage) ");
            this.Translator.TranslateExpression(output, bitmap);
            output.Add(").getHeight()");
        }

        protected override void TranslateImageNativeBitmapWidth(List<string> output, Expression bitmap)
        {
            output.Add("((java.awt.image.BufferedImage) ");
            this.Translator.TranslateExpression(output, bitmap);
            output.Add(").getWidth()");
        }

        protected override void TranslateImageScaleNativeResource(List<string> output, Expression bitmap, Expression width, Expression height)
        {
            output.Add("AwtTranslationHelper.scaleImage((java.awt.image.BufferedImage) ");
            this.Translator.TranslateExpression(output, bitmap);
            output.Add(", ");
            this.Translator.TranslateExpression(output, width);
            output.Add(", ");
            this.Translator.TranslateExpression(output, height);
            output.Add(")");
        }

        protected override void TranslateIoCreateDirectory(List<string> output, Expression path)
        {
            output.Add("AwtTranslationHelper.createDirectory(");
            this.Translator.TranslateExpression(output, path);
            output.Add(")");
        }

        protected override void TranslateIoCurrentDirectory(List<string> output)
        {
            output.Add("System.getProperty(\"user.dir\")");
        }

        protected override void TranslateIoDeleteDirectory(List<string> output, Expression path, Expression isRecursive)
        {
            output.Add("AwtTranslationHelper.ioDeleteDirectory(");
            this.Translator.TranslateExpression(output, path);
            output.Add(", ");
            this.Translator.TranslateExpression(output, isRecursive);
            output.Add(")");
        }

        protected override void TranslateIoDeleteFile(List<string> output, Expression path, Expression isUserData)
        {
            output.Add("AwtTranslationHelper.ioDeleteFile(");
            this.Translator.TranslateExpression(output, path);
            output.Add(")");
        }

        protected override void TranslateIoDoesPathExist(List<string> output, Expression canonicalizedPath, Expression directoriesOnly, Expression performCaseCheck, Expression isUserData)
        {
            output.Add("AwtTranslationHelper.checkPathExistence(");
            this.Translator.TranslateExpression(output, canonicalizedPath);
            output.Add(", ");
            this.Translator.TranslateExpression(output, directoriesOnly);
            output.Add(", ");
            this.Translator.TranslateExpression(output, performCaseCheck);
            output.Add(")");
        }

        protected override void TranslateIoFileReadText(List<string> output, Expression path, Expression isUserData)
        {
            output.Add("AwtTranslationHelper.readFile(");
            this.Translator.TranslateExpression(output, path);
            output.Add(")");
        }

        protected override void TranslateIoFilesInDirectory(List<string> output, Expression verifiedCanonicalizedPath, Expression isUserData)
        {
            output.Add("AwtTranslationHelper.directoryListing(");
            this.Translator.TranslateExpression(output, verifiedCanonicalizedPath);
            output.Add(")");
        }

        protected override void TranslateIoFileWriteText(List<string> output, Expression path, Expression content, Expression isUserData)
        {
            output.Add("AwtTranslationHelper.writeFile(");
            this.Translator.TranslateExpression(output, path);
            output.Add(", ");
            this.Translator.TranslateExpression(output, content);
            output.Add(")");
        }

        protected override void TranslateReadLocalImageResource(List<string> output, Expression filePath)
        {
            output.Add("AwtTranslationHelper.loadImageFromLocalFile(");
            this.Translator.TranslateExpression(output, filePath);
            output.Add(")");
        }

        protected override void TranslateResourceReadText(List<string> output, Expression path)
        {
            output.Add("AwtTranslationHelper.getTextResource(\"text/\" + ");
            this.Translator.TranslateExpression(output, path);
            output.Add(")");
        }

        protected override void TranslateGlMaxTextureSize(List<string> output)
        {
            throw new InvalidOperationException();
        }
    }
}
