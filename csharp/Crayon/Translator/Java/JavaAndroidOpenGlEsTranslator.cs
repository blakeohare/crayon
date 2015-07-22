using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crayon.Translator;

namespace Crayon.Translator.Java
{
	internal class JavaAndroidOpenGlEsTranslator : AbstractOpenGlTranslator
	{
		public override void TranslateGlBeginPolygon(List<string> output)
		{
			output.Add("TODO_GlBegin(Mode.Polygon)");
		}

		public override void TranslateGlBeginQuads(List<string> output)
		{
			output.Add("TODO_GlBegin(Mode.Quads)");
		}

		public override void TranslateGlBindTexture(List<string> output, ParseTree.Expression textureId)
		{
			output.Add("TODO_GlBindTexture(");
			this.Translator.TranslateExpression(output, textureId);
			output.Add(")");
		}

		public override void TranslateGlColor4(List<string> output, ParseTree.Expression r, ParseTree.Expression g, ParseTree.Expression b, ParseTree.Expression a)
		{
			output.Add("TODO_GlColor4(");
			this.Translator.TranslateExpression(output, r);
			output.Add(", ");
			this.Translator.TranslateExpression(output, g);
			output.Add(", ");
			this.Translator.TranslateExpression(output, b);
			output.Add(", ");
			this.Translator.TranslateExpression(output, a);
			output.Add(")");
		}

		public override void TranslateGlDisableTexture2D(List<string> output)
		{
			output.Add("TODO_GlDisable(Texture2D)");
		}

		public override void TranslateGlEnableTexture2D(List<string> output)
		{
			output.Add("TODO_GlEnable(Texture2D)");
		}

		public override void TranslateGlEnd(List<string> output)
		{
			output.Add("TODO_GlEnd()");
		}

		public override void TranslateGlLoadTexture(List<string> output, ParseTree.Expression platformBitmapResource)
		{
			output.Add("TODO_GlLoadTexture(");
			this.Translator.TranslateExpression(output, platformBitmapResource);
			output.Add(")");
		}

		public override void TranslateGlMaxTextureSize(List<string> output)
		{
			output.Add("TODO_GlMaxTextureSize()");
		}

		public override void TranslateGlTexCoord2(List<string> output, ParseTree.Expression x, ParseTree.Expression y)
		{
			output.Add("TODO_GlTexCoord2(");
			this.Translator.TranslateExpression(output, x);
			output.Add(", ");
			this.Translator.TranslateExpression(output, y);
			output.Add(")");
		}

		public override void TranslateGlVertex2(List<string> output, ParseTree.Expression x, ParseTree.Expression y)
		{
			output.Add("TODO_GlVertex2(");
			this.Translator.TranslateExpression(output, x);
			output.Add(", ");
			this.Translator.TranslateExpression(output, y);
			output.Add(")");
		}
	}
}
