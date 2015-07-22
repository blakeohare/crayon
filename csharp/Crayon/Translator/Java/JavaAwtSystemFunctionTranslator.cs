using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon.Translator.Java
{
	internal class JavaAwtSystemFunctionTranslator : JavaSystemFunctionTranslator
	{
		protected override void TranslateAppDataRoot(List<string> output)
		{
			output.Add("TranslationHelper.getAppDataRoot()");
		}

		protected override void TranslateGetEventsRawList(List<string> output)
		{
			output.Add("GameWindow.INSTANCE.pumpEventQueue()");
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

		protected override void TranslateInitializeScreen(List<string> output, Expression gameWidth, Expression gameHeight, Expression screenWidth, Expression screenHeight)
		{
			output.Add("GameWindow.initializeScreen(");
			this.Translator.TranslateExpression(output, gameWidth);
			output.Add(", ");
			this.Translator.TranslateExpression(output, gameHeight);
			if (screenWidth is NullConstant)
			{
				output.Add(")");
			}
			else
			{
				output.Add(", ");
				this.Translator.TranslateExpression(output, screenWidth);
				output.Add(", ");
				this.Translator.TranslateExpression(output, screenHeight);
				output.Add(")");
			}
		}

		protected override void TranslateIoCreateDirectory(List<string> output, Expression path)
		{
			output.Add("TranslationHelper.createDirectory(");
			this.Translator.TranslateExpression(output, path);
			output.Add(")");
		}

		protected override void TranslateIoCurrentDirectory(List<string> output)
		{
			output.Add("System.getProperty(\"user.dir\")");
		}

		protected override void TranslateIoDeleteDirectory(List<string> output, Expression path, Expression isRecursive)
		{
			output.Add("TranslationHelper.ioDeleteDirectory(");
			this.Translator.TranslateExpression(output, path);
			output.Add(", ");
			this.Translator.TranslateExpression(output, isRecursive);
			output.Add(")");
		}

		protected override void TranslateIoDeleteFile(List<string> output, Expression path, Expression isUserData)
		{
			output.Add("TranslationHelper.ioDeleteFile(");
			this.Translator.TranslateExpression(output, path);
			output.Add(")");
		}

		protected override void TranslateIoDoesPathExist(List<string> output, Expression canonicalizedPath, Expression directoriesOnly, Expression performCaseCheck, Expression isUserData)
		{
			output.Add("TranslationHelper.checkPathExistence(");
			this.Translator.TranslateExpression(output, canonicalizedPath);
			output.Add(", ");
			this.Translator.TranslateExpression(output, directoriesOnly);
			output.Add(", ");
			this.Translator.TranslateExpression(output, performCaseCheck);
			output.Add(")");
		}

		protected override void TranslateIoFileReadText(List<string> output, Expression path, Expression isUserData)
		{
			output.Add("TranslationHelper.readFile(");
			this.Translator.TranslateExpression(output, path);
			output.Add(")");
		}

		protected override void TranslateIoFilesInDirectory(List<string> output, Expression verifiedCanonicalizedPath, Expression isUserData)
		{
			output.Add("TranslationHelper.directoryListing(");
			this.Translator.TranslateExpression(output, verifiedCanonicalizedPath);
			output.Add(")");
		}

		protected override void TranslateIoFileWriteText(List<string> output, Expression path, Expression content, Expression isUserData)
		{
			output.Add("TranslationHelper.writeFile(");
			this.Translator.TranslateExpression(output, path);
			output.Add(", ");
			this.Translator.TranslateExpression(output, content);
			output.Add(")");
		}

		protected override void TranslatePrint(List<string> output, Expression message)
		{
			output.Add("System.out.println(");
			this.Translator.TranslateExpression(output, message);
			output.Add(")");
		}

		protected override void TranslateReadLocalImageResource(List<string> output, Expression filePath)
		{
			output.Add("TranslationHelper.loadImageFromLocalFile(");
			this.Translator.TranslateExpression(output, filePath);
			output.Add(")");
		}

		protected override void TranslateReadLocalSoundResource(List<string> output, Expression filePath)
		{
			output.Add("TranslationHelper.readLocalSoundResource(");
			this.Translator.TranslateExpression(output, filePath);
			output.Add(")");
		}

		protected override void TranslateReadLocalTileResource(List<string> output, Expression tileGenName)
		{
			output.Add("TranslationHelper.readLocalTileResource(");
			this.Translator.TranslateExpression(output, tileGenName);
			output.Add(")");
		}

		protected override void TranslateResourceReadText(List<string> output, Expression path)
		{
			output.Add("TranslationHelper.getTextResource(");
			this.Translator.TranslateExpression(output, path);
			output.Add(")");
		}

		protected override void TranslateSetTitle(List<string> output, Expression title)
		{
			output.Add("GameWindow.INSTANCE.setTitle(");
			this.Translator.TranslateExpression(output, title);
			output.Add(")");
		}

		protected override void TranslateSoundPlay(List<string> output, Expression soundInstance)
		{
			output.Add("TranslationHelper.playSoundImpl(");
			this.Translator.TranslateExpression(output, soundInstance);
			output.Add(")");
		}

	}
}
