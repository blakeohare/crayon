using System;
using OpenTK.Graphics.OpenGL;
using Interpreter.Vm;

namespace Interpreter.Libraries.Game
{
	public static class GlUtil
	{
		public static void PrepareRenderPipeline() {
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			GL.ClearColor(0f, 0f, 0f, 1f);
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadIdentity();
		}

		public static int ForceLoadTexture(UniversalBitmap bitmap)
		{
			bitmap = NormalizeBitmap(bitmap);
			int width = bitmap.Width;
			int height = bitmap.Height;
			int textureId;

            UniversalBitmap.BitLockSession bitlock = bitmap.GetActiveBitLockSession();
            
			GL.GenTextures(1, out textureId);
			GL.BindTexture(TextureTarget.Texture2D, textureId);
			GL.TexImage2D(
				TextureTarget.Texture2D,
				0,
				PixelInternalFormat.Rgba,
				width, height, 0,
				PixelFormat.Bgra, PixelType.UnsignedByte,
				bitlock.GetPtr());

            bitlock.Free();

			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

			return textureId;
		}

		private static UniversalBitmap NormalizeBitmap(UniversalBitmap bitmap)
		{
			int oldWidth = bitmap.Width;
			int oldHeight = bitmap.Height;

			int newWidth = CrayonWrapper.v_nextPowerOf2(oldWidth);
			int newHeight = CrayonWrapper.v_nextPowerOf2(oldHeight);

			if (newWidth == oldWidth &&
				newHeight == oldHeight)
			{
				return bitmap;
			}

            UniversalBitmap newBmp = new UniversalBitmap(newWidth, newHeight);
            newBmp.GetActiveDrawingSession().Draw(bitmap, 0, 0, 0, 0, oldWidth, oldHeight).Flush();
			return newBmp;
		}
	}
}
