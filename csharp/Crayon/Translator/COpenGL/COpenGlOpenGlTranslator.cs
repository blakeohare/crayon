using System;
using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon.Translator.COpenGL
{
	internal class COpenGlOpenGlTranslator : AbstractOpenGlTranslator
	{
		public override bool IsNewStyle { get { return true; } }

		public override void TranslateGlBeginPolygon(List<string> output)
		{
			throw new NotImplementedException();
		}

		public override void TranslateGlBeginQuads(List<string> output)
		{
			throw new NotImplementedException();
		}

		public override void TranslateGlBindTexture(List<string> output, Expression textureId)
		{
			throw new NotImplementedException();
		}

		public override void TranslateGlColor4(List<string> output, Expression r, Expression g, Expression b, Expression a)
		{
			throw new NotImplementedException();
		}

		public override void TranslateGlDisableTexture2D(List<string> output)
		{
			throw new NotImplementedException();
		}

		public override void TranslateGlDisableVertexArray(List<string> output)
		{
			throw new NotImplementedException();
		}

		public override void TranslateGlDrawEllipseVertices(List<string> output)
		{
			throw new NotImplementedException();
		}

		public override void TranslateGlEnableTexture2D(List<string> output)
		{
			throw new NotImplementedException();
		}

		public override void TranslateGlEnableVertexArray(List<string> output)
		{
			throw new NotImplementedException();
		}

		public override void TranslateGlEnd(List<string> output)
		{
			throw new NotImplementedException();
		}

		public override void TranslateGlLoadIdentity(List<string> output)
		{
			throw new NotImplementedException();
		}

		public override void TranslateGlLoadTexture(List<string> output, Expression platformBitmapResource)
		{
			throw new NotImplementedException();
		}

		public override void TranslateGlMaxTextureSize(List<string> output)
		{
			throw new NotImplementedException();
		}

		public override void TranslateGlScale(List<string> output, Expression xratio, Expression yratio)
		{
			throw new NotImplementedException();
		}

		public override void TranslateGlTexCoord2(List<string> output, Expression x, Expression y)
		{
			throw new NotImplementedException();
		}

		public override void TranslateGlTranslate(List<string> output, Expression dx, Expression dy)
		{
			throw new NotImplementedException();
		}

		public override void TranslateGlVertex2(List<string> output, Expression x, Expression y)
		{
			throw new NotImplementedException();
		}

		public override void TranslateGlPrepareDrawPipeline(List<string> output, Expression glReference)
		{
			throw new NotImplementedException();
		}
	}
}