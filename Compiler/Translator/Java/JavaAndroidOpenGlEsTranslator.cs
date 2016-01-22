using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crayon.ParseTree;
using Crayon.Translator;

namespace Crayon.Translator.Java
{
	internal class JavaAndroidOpenGlEsTranslator : AbstractOpenGlTranslator
	{
		public override bool IsNewStyle { get { return true; } }

		public override void TranslateGlBeginPolygon(List<string> output, Expression gl)
		{
			throw new InvalidOperationException();
		}

		public override void TranslateGlBeginQuads(List<string> output, Expression gl)
		{
			throw new InvalidOperationException();
		}

		public override void TranslateGlBindTexture(List<string> output, Expression gl, Expression textureId)
		{
			this.Translator.TranslateExpression(output, gl);
			output.Add(".glBindTexture(javax.microedition.khronos.opengles.GL10.GL_TEXTURE_2D, ");
			this.Translator.TranslateExpression(output, textureId);
			output.Add(")");
		}

		public override void TranslateGlColor4(List<string> output, Expression gl, Expression r, Expression g, Expression b, Expression a)
		{
			this.Translator.TranslateExpression(output, gl);
			output.Add(".glColor4f(");
			this.Translator.TranslateExpression(output, r);
			output.Add(" / 255f, ");
			this.Translator.TranslateExpression(output, g);
			output.Add(" / 255f, ");
			this.Translator.TranslateExpression(output, b);
			output.Add(" / 255f, ");
			this.Translator.TranslateExpression(output, a);
			output.Add(" / 255f)");
		}

		public override void TranslateGlDisableTexture2D(List<string> output, Expression gl)
		{
			throw new InvalidOperationException();
		}

		public override void TranslateGlDisableTexCoordArray(List<string> output, Expression gl)
		{
			this.Translator.TranslateExpression(output, gl);
			output.Add(".glDisableClientState(javax.microedition.khronos.opengles.GL10.GL_TEXTURE_COORD_ARRAY)");
		}

		public override void TranslateGlDisableVertexArray(List<string> output, Expression gl)
		{
			this.Translator.TranslateExpression(output, gl);
			output.Add(".glDisableClientState(javax.microedition.khronos.opengles.GL10.GL_VERTEX_ARRAY)");
		}

		public override void TranslateGlDrawArrays(List<string> output, Expression gl, Expression vertexCount)
		{
			this.Translator.TranslateExpression(output, gl);
			output.Add(".glDrawArrays(javax.microedition.khronos.opengles.GL10.GL_TRIANGLE_STRIP, 0, ");
			this.Translator.TranslateExpression(output, vertexCount);
			output.Add(")");
		}

		public override void TranslateGlDrawEllipseVertices(List<string> output, Expression gl)
		{
			throw new InvalidOperationException();
		}

		public override void TranslateGlEnableTexture2D(List<string> output, Expression gl)
		{
			throw new InvalidOperationException();
		}

		public override void TranslateGlEnableTextureCoordArray(List<string> output, Expression gl)
		{
			this.Translator.TranslateExpression(output, gl);
			output.Add(".glEnableClientState(javax.microedition.khronos.opengles.GL10.GL_TEXTURE_COORD_ARRAY)");
		}

		public override void TranslateGlEnableVertexArray(List<string> output, Expression gl)
		{
			this.Translator.TranslateExpression(output, gl);
			output.Add(".glEnableClientState(javax.microedition.khronos.opengles.GL10.GL_VERTEX_ARRAY)");
		}

		public override void TranslateGlEnd(List<string> output, Expression gl)
		{
			throw new InvalidOperationException();
		}

		public override void TranslateGlFrontFaceCw(List<string> output, Expression gl)
		{
			this.Translator.TranslateExpression(output, gl);
			output.Add(".glFrontFace(javax.microedition.khronos.opengles.GL10.GL_CW)");
		}

		public override void TranslateGlGetQuadTextureVbo(List<string> output, Expression gl)
		{
			output.Add("GlUtil.getQuadTextureVbo(");
			this.Translator.TranslateExpression(output, gl);
			output.Add(")");
		}

		public override void TranslateGlGetQuadVbo(List<string> output, Expression gl)
		{
			output.Add("GlUtil.getQuadVbo(");
			this.Translator.TranslateExpression(output, gl);
			output.Add(")");
		}

		public override void TranslateGlLoadIdentity(List<string> output, Expression gl)
		{
			this.Translator.TranslateExpression(output, gl);
			output.Add(".glLoadIdentity()");
		}

		public override void TranslateGlLoadTexture(List<string> output, Expression gl, Expression platformBitmapResource)
		{
			output.Add("GlUtil.forceLoadTexture(");
			this.Translator.TranslateExpression(output, gl);
			output.Add(", (android.graphics.Bitmap) ");
			this.Translator.TranslateExpression(output, platformBitmapResource);
			output.Add(")");
		}

		public override void TranslateGlMaxTextureSize(List<string> output)
		{
			output.Add("GlUtil.getMaxTextureSize()");
		}

		public override void TranslateGlScale(List<string> output, Expression gl, Expression xratio, Expression yratio)
		{
			this.Translator.TranslateExpression(output, gl);
			output.Add(".glScalef(");
			this.Translator.TranslateExpression(output, xratio);
			output.Add(", ");
			this.Translator.TranslateExpression(output, yratio);
			output.Add(", 0f)");
		}

		public override void TranslateGlTexCoord2(List<string> output, Expression gl, Expression x, Expression y)
		{
			throw new InvalidOperationException();
		}

		public override void TranslateGlTexCoordPointer(List<string> output, Expression gl, Expression textureBuffer)
		{
			this.Translator.TranslateExpression(output, gl);
			output.Add(".glTexCoordPointer(2, javax.microedition.khronos.opengles.GL10.GL_FLOAT, 0, ");
			this.Translator.TranslateExpression(output, textureBuffer);
			output.Add(")");
		}

		public override void TranslateGlTranslate(List<string> output, Expression gl, Expression dx, Expression dy)
		{
			this.Translator.TranslateExpression(output, gl);
			output.Add(".glTranslatef(");
			this.Translator.TranslateExpression(output, dx);
			output.Add(", ");
			this.Translator.TranslateExpression(output, dy);
			output.Add(", 0f)");
		}

		public override void TranslateGlVertex2(List<string> output, Expression gl, Expression x, Expression y)
		{
			throw new InvalidOperationException();
		}

		public override void TranslateGlPrepareDrawPipeline(List<string> output, Expression gl)
		{
			output.Add("GlUtil.prepareDrawPipeline(");
			this.Translator.TranslateExpression(output, gl);
			output.Add(")");
		}

		public override void TranslateGlVertexPointer(List<string> output, Expression gl, Expression vertexBuffer)
		{
			this.Translator.TranslateExpression(output, gl);
			output.Add(".glVertexPointer(3, javax.microedition.khronos.opengles.GL10.GL_FLOAT, 0, ");
			this.Translator.TranslateExpression(output, vertexBuffer);
			output.Add(")");
		}
	}
}
