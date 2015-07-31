using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon.Translator
{
	internal abstract class AbstractOpenGlTranslator
	{
		public AbstractPlatform Platform { get; set; }
		public AbstractTranslator Translator { get; set; }

		public abstract bool IsNewStyle { get; }

		public abstract void TranslateGlBeginPolygon(List<string> output);
		public abstract void TranslateGlBeginQuads(List<string> output);
		public abstract void TranslateGlBindTexture(List<string> output, Expression textureId);
		public abstract void TranslateGlColor4(List<string> output, Expression r, Expression g, Expression b, Expression a);
		public abstract void TranslateGlDisableTexture2D(List<string> output);
		public abstract void TranslateGlDisableVertexArray(List<string> output);
		public abstract void TranslateGlDrawEllipseVertices(List<string> output);
		public abstract void TranslateGlEnableTexture2D(List<string> output);
		public abstract void TranslateGlEnableVertexArray(List<string> output);
		public abstract void TranslateGlEnd(List<string> output);
		public abstract void TranslateGlLoadIdentity(List<string> output);
		public abstract void TranslateGlLoadTexture(List<string> output, Expression platformBitmapResource);
		public abstract void TranslateGlMaxTextureSize(List<string> output);
		public abstract void TranslateGlPrepareDrawPipeline(List<string> output, Expression glReference);
		public abstract void TranslateGlScale(List<string> output, Expression xratio, Expression yratio);
		public abstract void TranslateGlTexCoord2(List<string> output, Expression x, Expression y);
		public abstract void TranslateGlTranslate(List<string> output, Expression dx, Expression dy);
		public abstract void TranslateGlVertex2(List<string> output, Expression x, Expression y);
	}
}
