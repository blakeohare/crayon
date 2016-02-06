using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon.Translator
{
	internal abstract class AbstractOpenGlTranslator
	{
		public AbstractPlatform Platform { get; set; }
		public AbstractTranslator Translator { get; set; }

		public abstract bool IsNewStyle { get; }

		public abstract void TranslateGlBeginPolygon(List<string> output, Expression gl);
		public abstract void TranslateGlBeginQuads(List<string> output, Expression gl);
		public abstract void TranslateGlBindTexture(List<string> output, Expression gl, Expression textureId);
		public abstract void TranslateGlColor4(List<string> output, Expression gl, Expression r, Expression g, Expression b, Expression a);
		public abstract void TranslateGlDisableTexture2D(List<string> output, Expression gl);
		public abstract void TranslateGlDisableTexCoordArray(List<string> output, Expression gl);
		public abstract void TranslateGlDisableVertexArray(List<string> output, Expression gl);
		public abstract void TranslateGlDrawArrays(List<string> output, Expression gl, Expression vertexCount);
		public abstract void TranslateGlDrawEllipseVertices(List<string> output, Expression gl);
		public abstract void TranslateGlEnableTexture2D(List<string> output, Expression gl);
		public abstract void TranslateGlEnableTextureCoordArray(List<string> output, Expression gl);
		public abstract void TranslateGlEnableVertexArray(List<string> output, Expression gl);
		public abstract void TranslateGlEnd(List<string> output, Expression gl);
		public abstract void TranslateGlFrontFaceCw(List<string> output, Expression gl);
		public abstract void TranslateGlGetQuadTextureVbo(List<string> output, Expression gl);
		public abstract void TranslateGlGetQuadVbo(List<string> output, Expression gl);
		public abstract void TranslateGlLoadIdentity(List<string> output, Expression gl);
		public abstract void TranslateGlLoadTexture(List<string> output, Expression gl, Expression platformBitmapResource);
		public abstract void TranslateGlMaxTextureSize(List<string> output);
		public abstract void TranslateGlPrepareDrawPipeline(List<string> output, Expression gl);
		public abstract void TranslateGlScale(List<string> output, Expression gl, Expression xratio, Expression yratio);
		public abstract void TranslateGlTexCoord2(List<string> output, Expression gl, Expression x, Expression y);
		public abstract void TranslateGlTexCoordPointer(List<string> output, Expression gl, Expression textureBuffer);
		public abstract void TranslateGlTranslate(List<string> output, Expression gl, Expression dx, Expression dy);
		public abstract void TranslateGlVertex2(List<string> output, Expression gl, Expression x, Expression y);
		public abstract void TranslateGlVertexPointer(List<string> output, Expression gl, Expression vertexBuffer);
	}
}
