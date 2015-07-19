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
			throw new NotImplementedException();
		}

		protected override void TranslateGetEventsRawList(List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateGlMaxTextureSize(List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateImageNativeBitmapHeight(List<string> output, ParseTree.Expression bitmap)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateImageNativeBitmapWidth(List<string> output, ParseTree.Expression bitmap)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateInitializeScreen(List<string> output, ParseTree.Expression gameWidth, ParseTree.Expression gameHeight, ParseTree.Expression screenWidth, ParseTree.Expression screenHeight)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateIoCreateDirectory(List<string> output, ParseTree.Expression path)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateIoCurrentDirectory(List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateIoDeleteDirectory(List<string> output, ParseTree.Expression path, ParseTree.Expression isRecursive)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateIoDeleteFile(List<string> output, ParseTree.Expression path, ParseTree.Expression isUserData)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateIoDoesPathExist(List<string> output, ParseTree.Expression canonicalizedPath, ParseTree.Expression directoriesOnly, ParseTree.Expression performCaseCheck, ParseTree.Expression isUserData)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateIoFileReadText(List<string> output, ParseTree.Expression path, ParseTree.Expression isUserData)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateIoFilesInDirectory(List<string> output, ParseTree.Expression verifiedCanonicalizedPath, ParseTree.Expression isUserData)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateIoFileWriteText(List<string> output, ParseTree.Expression path, ParseTree.Expression content, ParseTree.Expression isUserData)
		{
			throw new NotImplementedException();
		}

		protected override void TranslatePrint(List<string> output, ParseTree.Expression message)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateReadLocalImageResource(List<string> output, ParseTree.Expression filePath)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateReadLocalSoundResource(List<string> output, ParseTree.Expression filePath)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateReadLocalTileResource(List<string> output, ParseTree.Expression tileGenName)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateResourceReadText(List<string> output, ParseTree.Expression path)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateSetTitle(List<string> output, ParseTree.Expression title)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateSoundPlay(List<string> output, ParseTree.Expression soundInstance)
		{
			throw new NotImplementedException();
		}
	}
}
