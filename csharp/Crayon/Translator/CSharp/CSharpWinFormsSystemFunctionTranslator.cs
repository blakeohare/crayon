using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crayon.ParseTree;

namespace Crayon.Translator.CSharp
{
	class CSharpWinFormsSystemFunctionTranslator : CSharpSystemFunctionTranslator
	{
		protected override void TranslateBlitImage(List<string> output, Expression image, Expression x, Expression y)
		{
			output.Add("Renderer.INSTANCE.BlitImage((Image)");
			this.Translator.TranslateExpression(output, image);
			output.Add(", ");
			this.Translator.TranslateExpression(output, x);
			output.Add(", ");
			this.Translator.TranslateExpression(output, y);
			output.Add(")");
		}

		protected override void TranslateBlitImagePartial(List<string> output, Expression image, Expression targetX, Expression targetY, Expression targetWidth, Expression targetHeight, Expression sourceX, Expression sourceY, Expression sourceWidth, Expression sourceHeight)
		{
			output.Add("Renderer.INSTANCE.BlitImagePartial((Image)");
			this.Translator.TranslateExpression(output, image);
			output.Add(", ");
			this.Translator.TranslateExpression(output, targetX);
			output.Add(", ");
			this.Translator.TranslateExpression(output, targetY);
			output.Add(", ");
			this.Translator.TranslateExpression(output, targetWidth);
			output.Add(", ");
			this.Translator.TranslateExpression(output, targetHeight);
			output.Add(", ");
			this.Translator.TranslateExpression(output, sourceX);
			output.Add(", ");
			this.Translator.TranslateExpression(output, sourceY);
			output.Add(", ");
			this.Translator.TranslateExpression(output, sourceWidth);
			output.Add(", ");
			this.Translator.TranslateExpression(output, sourceHeight);
			output.Add(")");
		}

		protected override void TranslateDrawEllipse(List<string> output, Expression left, Expression top, Expression width, Expression height, Expression red, Expression green, Expression blue, Expression alpha)
		{
			output.Add("Renderer.INSTANCE.DrawEllipse(");
			this.Translator.TranslateExpression(output, left);
			output.Add(", ");
			this.Translator.TranslateExpression(output, top);
			output.Add(", ");
			this.Translator.TranslateExpression(output, width);
			output.Add(", ");
			this.Translator.TranslateExpression(output, height);
			output.Add(", ");
			this.Translator.TranslateExpression(output, red);
			output.Add(", ");
			this.Translator.TranslateExpression(output, green);
			output.Add(", ");
			this.Translator.TranslateExpression(output, blue);
			output.Add(", ");
			this.Translator.TranslateExpression(output, alpha);
			output.Add(")");
		}

		protected override void TranslateDrawLine(List<string> output, Expression ax, Expression ay, Expression bx, Expression by, Expression lineWidth, Expression red, Expression green, Expression blue, Expression alpha)
		{
			output.Add("Renderer.INSTANCE.DrawLine(");
			this.Translator.TranslateExpression(output, ax);
			output.Add(", ");
			this.Translator.TranslateExpression(output, ay);
			output.Add(", ");
			this.Translator.TranslateExpression(output, bx);
			output.Add(", ");
			this.Translator.TranslateExpression(output, by);
			output.Add(", ");
			this.Translator.TranslateExpression(output, lineWidth);
			output.Add(", ");
			this.Translator.TranslateExpression(output, red);
			output.Add(", ");
			this.Translator.TranslateExpression(output, green);
			output.Add(", ");
			this.Translator.TranslateExpression(output, blue);
			output.Add(", ");
			this.Translator.TranslateExpression(output, alpha);
			output.Add(")");
		}

		protected override void TranslateDrawRectangle(List<string> output, Expression left, Expression top, Expression width, Expression height, Expression red, Expression green, Expression blue, Expression alpha)
		{
			output.Add("Renderer.INSTANCE.DrawRectangle(");
			this.Translator.TranslateExpression(output, left);
			output.Add(", ");
			this.Translator.TranslateExpression(output, top);
			output.Add(", ");
			this.Translator.TranslateExpression(output, width);
			output.Add(", ");
			this.Translator.TranslateExpression(output, height);
			output.Add(", ");
			this.Translator.TranslateExpression(output, red);
			output.Add(", ");
			this.Translator.TranslateExpression(output, green);
			output.Add(", ");
			this.Translator.TranslateExpression(output, blue);
			output.Add(", ");
			this.Translator.TranslateExpression(output, alpha);
			output.Add(")");
		}

		protected override void TranslateFillScreen(List<string> output, Expression red, Expression green, Expression blue)
		{
			output.Add("Renderer.INSTANCE.Fill(");
			this.Translator.TranslateExpression(output, red);
			output.Add(", ");
			this.Translator.TranslateExpression(output, green);
			output.Add(", ");
			this.Translator.TranslateExpression(output, blue);
			output.Add(")");
		}
	}
}
