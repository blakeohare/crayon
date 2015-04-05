using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon.Translator.CSharp
{
	class CSharpWindowsPhoneSystemFunctionTranslator : CSharpSystemFunctionTranslator
	{
		protected override void TranslateBlitImage(List<string> output, ParseTree.Expression image, ParseTree.Expression x, ParseTree.Expression y)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateBlitImagePartial(List<string> output, ParseTree.Expression image, ParseTree.Expression targetX, ParseTree.Expression targetY, ParseTree.Expression targetWidth, ParseTree.Expression targetHeight, ParseTree.Expression sourceX, ParseTree.Expression sourceY, ParseTree.Expression sourceWidth, ParseTree.Expression sourceHeight)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDrawEllipse(List<string> output, ParseTree.Expression left, ParseTree.Expression top, ParseTree.Expression width, ParseTree.Expression height, ParseTree.Expression red, ParseTree.Expression green, ParseTree.Expression blue, ParseTree.Expression alpha)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDrawLine(List<string> output, ParseTree.Expression ax, ParseTree.Expression ay, ParseTree.Expression bx, ParseTree.Expression by, ParseTree.Expression lineWidth, ParseTree.Expression red, ParseTree.Expression green, ParseTree.Expression blue, ParseTree.Expression alpha)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDrawRectangle(List<string> output, ParseTree.Expression left, ParseTree.Expression top, ParseTree.Expression width, ParseTree.Expression height, ParseTree.Expression red, ParseTree.Expression green, ParseTree.Expression blue, ParseTree.Expression alpha)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateFillScreen(List<string> output, ParseTree.Expression red, ParseTree.Expression green, ParseTree.Expression blue)
		{
			throw new NotImplementedException();
		}
	}
}
