using System;
using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon.Translator.CSharp
{
    class CSharpWinFormsSystemFunctionTranslator : CSharpSystemFunctionTranslator
    {
        protected override void TranslateAppDataRoot(List<string> output)
        {
            output.Add("WinFormsTranslationHelper.TODO_appDataRoot()");
        }

        protected override void TranslateHttpRequest(List<string> output, Expression httpRequest, Expression method, Expression url, Expression body, Expression userAgent, Expression contentType, Expression contentLength, Expression headerNameList, Expression headerValueList)
        {
            output.Add("WinFormsTranslationHelper.TODO_httpRequest()");
        }

        protected override void TranslateIoCreateDirectory(List<string> output, Expression path)
        {
            output.Add("WinFormsTranslationHelper.TODO_createDirectory()");
        }

        protected override void TranslateIoCurrentDirectory(List<string> output)
        {
            output.Add("WinFormsTranslationHelper.TODO_currentDirectory()");
        }

        protected override void TranslateIoDeleteDirectory(List<string> output, Expression path, Expression isRecursive)
        {
            output.Add("WinFormsTranslationHelper.TODO_deleteDirectory()");
        }

        protected override void TranslateIoDeleteFile(List<string> output, Expression path, Expression isUserData)
        {
            output.Add("WinFormsTranslationHelper.TODO_deleteFile()");
        }

        protected override void TranslateIoDoesPathExist(List<string> output, Expression canonicalizedPath, Expression directoriesOnly, Expression performCaseCheck, Expression isUserData)
        {
            output.Add("WinFormsTranslationHelper.TODO_ioPathExists()");
        }

        protected override void TranslateIoFileReadText(List<string> output, Expression path, Expression isUserData)
        {
            output.Add("WinFormsTranslationHelper.TODO_readText()");
        }

        protected override void TranslateIoFilesInDirectory(List<string> output, Expression verifiedCanonicalizedPath, Expression isUserData)
        {
            output.Add("WinFormsTranslationHelper.TODO_filesInDirectory()");
        }

        protected override void TranslateIoFileWriteText(List<string> output, Expression path, Expression content, Expression isUserData)
        {
            output.Add("WinFormsTranslationHelper.TODO_fileWriteText()");
        }

        protected override void TranslateResourceGetManifest(List<string> output)
        {
            output.Add("ResourceReader.GetResourceManifest()");
        }
    }
}
