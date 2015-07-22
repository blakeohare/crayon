using System;
using System.Collections.Generic;

namespace Crayon.Translator.COpenGL
{
	internal class COpenGlOpenGlTranslator : AbstractOpenGlTranslator
	{
		public override void TranslateGlBeginPolygon(List<string> output)
		{
			throw new NotImplementedException();
		}

		public override void TranslateGlBeginQuads(List<string> output)
		{
			throw new NotImplementedException();
		}

		public override void TranslateGlBindTexture(List<string> output, ParseTree.Expression textureId)
		{
			throw new NotImplementedException();
		}

		public override void TranslateGlColor4(List<string> output, ParseTree.Expression r, ParseTree.Expression g, ParseTree.Expression b, ParseTree.Expression a)
		{
			throw new NotImplementedException();
		}

		public override void TranslateGlDisableTexture2D(List<string> output)
		{
			throw new NotImplementedException();
		}

		public override void TranslateGlEnableTexture2D(List<string> output)
		{
			throw new NotImplementedException();
		}

		public override void TranslateGlEnd(List<string> output)
		{
			throw new NotImplementedException();
		}

		public override void TranslateGlLoadTexture(List<string> output, ParseTree.Expression platformBitmapResource)
		{
			throw new NotImplementedException();
		}

		public override void TranslateGlMaxTextureSize(List<string> output)
		{
			throw new NotImplementedException();
		}

		public override void TranslateGlTexCoord2(List<string> output, ParseTree.Expression x, ParseTree.Expression y)
		{
			throw new NotImplementedException();
		}

		public override void TranslateGlVertex2(List<string> output, ParseTree.Expression x, ParseTree.Expression y)
		{
			throw new NotImplementedException();
		}
	}
}