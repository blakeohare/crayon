using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon.Translator.Java
{
	internal class JavaAndroidSystemFunctionTranslator : JavaSystemFunctionTranslator
	{
		protected override void TranslateAppDataRoot(List<string> output)
		{
			output.Add("TODO_translateAppDataRoot()");
		}

		protected override void TranslateGetEventsRawList(List<string> output)
		{
			output.Add("TODO_translateGetEventsRawList()");
		}

		protected override void TranslateImageNativeBitmapHeight(List<string> output, ParseTree.Expression bitmap)
		{
			output.Add("TODO_translateImageNativeBitmapHeight()");
		}

		protected override void TranslateImageNativeBitmapWidth(List<string> output, ParseTree.Expression bitmap)
		{
			output.Add("TODO_translateImageNativeBitmapWidth()");
		}

		protected override void TranslateInitializeScreen(List<string> output, ParseTree.Expression gameWidth, ParseTree.Expression gameHeight, ParseTree.Expression screenWidth, ParseTree.Expression screenHeight)
		{
			output.Add("TODO_translateInitializeScreen()");
		}

		protected override void TranslateIoCreateDirectory(List<string> output, ParseTree.Expression path)
		{
			output.Add("TODO_translateIoCreateDirectory()");
		}

		protected override void TranslateIoCurrentDirectory(List<string> output)
		{
			output.Add("TODO_translateIoCurrentDirectory()");
		}

		protected override void TranslateIoDeleteDirectory(List<string> output, ParseTree.Expression path, ParseTree.Expression isRecursive)
		{
			output.Add("TODO_translateIoDeleteDirectory()");
		}

		protected override void TranslateIoDeleteFile(List<string> output, ParseTree.Expression path, ParseTree.Expression isUserData)
		{
			output.Add("TODO_translateIoDeleteFile()");
		}

		protected override void TranslateIoDoesPathExist(List<string> output, ParseTree.Expression canonicalizedPath, ParseTree.Expression directoriesOnly, ParseTree.Expression performCaseCheck, ParseTree.Expression isUserData)
		{
			output.Add("TODO_translateIoDoesPathExist()");
		}

		protected override void TranslateIoFileReadText(List<string> output, ParseTree.Expression path, ParseTree.Expression isUserData)
		{
			output.Add("TODO_translateIoFileReadText()");
		}

		protected override void TranslateIoFilesInDirectory(List<string> output, ParseTree.Expression verifiedCanonicalizedPath, ParseTree.Expression isUserData)
		{
			output.Add("TODO_translateIoFilesInDirectory()");
		}

		protected override void TranslateIoFileWriteText(List<string> output, ParseTree.Expression path, ParseTree.Expression content, ParseTree.Expression isUserData)
		{
			output.Add("TODO_translateIoFileWriteText()");
		}

		protected override void TranslatePrint(List<string> output, ParseTree.Expression message)
		{
			output.Add("TODO_translatePrint()");
		}

		protected override void TranslateReadLocalImageResource(List<string> output, ParseTree.Expression filePath)
		{
			output.Add("TODO_translateReadLocalImageResource()");
		}

		protected override void TranslateReadLocalSoundResource(List<string> output, ParseTree.Expression filePath)
		{
			output.Add("TODO_translateReadLocalSoundResource()");
		}

		protected override void TranslateReadLocalTileResource(List<string> output, ParseTree.Expression tileGenName)
		{
			output.Add("TODO_translateReadLocalTileResource()");
		}

		protected override void TranslateResourceReadText(List<string> output, ParseTree.Expression path)
		{
			output.Add("TODO_translateResourceReadText()");
		}

		protected override void TranslateSetTitle(List<string> output, ParseTree.Expression title)
		{
			output.Add("TODO_translateSetTitle()");
		}

		protected override void TranslateSoundPlay(List<string> output, ParseTree.Expression soundInstance)
		{
			output.Add("TODO_translateSoundPlay()");
		}
	}
}
