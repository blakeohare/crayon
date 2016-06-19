using System;

namespace Crayon
{
	/**
	 * Wraps a System.Drawing.Bitmap in Windows or a MonoMac.CoreGraphics Bitmap on a Mac.
	 */
	public class SystemBitmap
	{
		#if WINDOWS
		private System.Drawing.Bitmap bitmap;
		#elif MAC

		#endif
		public int Width { get; set; }
		public int Height { get; set; }

		public SystemBitmap(string filepath)
		{
#if WINDOWS
			this.bitmap = new System.Drawing.Bitmap(filepath);
			this.bitmap.SetResolution(96, 96);
			this.Width = this.bitmap.Width;
			this.Height = this.bitmap.Height;

			// Java does not recognize the alpha channel on a small subset of PNG encodings.
			// Re-encode them before handing them off to the universe.
			System.Drawing.Bitmap newBmp = new System.Drawing.Bitmap(this.Width, this.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(newBmp);
			g.DrawImage(this.bitmap, 0, 0, this.Width, this.Height);
			g.Flush();
			g.Dispose();

			this.bitmap = newBmp;
#elif OSX
			throw new System.NotImplementedException();
#endif
		}

		public SystemBitmap(int width, int height)
		{
#if WINDOWS
			this.bitmap = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			this.bitmap.SetResolution(96, 96);
			this.Width = width;
			this.Height = height;
#elif OSX
			throw new System.NotImplementedException();
#endif
		}

		public void Save(string path)
		{
#if WINDOWS
			this.bitmap.Save(path);
#elif OSX
			throw new System.NotImplementedException();
#endif
		}

		public Graphics MakeGraphics()
		{
			return new Graphics(this);
		}

		public class Graphics
		{
#if WINDOWS
			private System.Drawing.Graphics systemGraphics;
#elif OSX

#endif

			public Graphics(SystemBitmap owner)
			{
#if WINDOWS
				this.systemGraphics = System.Drawing.Graphics.FromImage(owner.bitmap);
#elif OSX
				throw new System.NotImplementedException();
#endif
			}

			public void Blit(SystemBitmap bmp, int x, int y)
			{
#if WINDOWS
				this.systemGraphics.DrawImageUnscaled(bmp.bitmap, x, y);
#elif OSX
				throw new System.NotImplementedException();
#endif
			}
		}
	}
}
