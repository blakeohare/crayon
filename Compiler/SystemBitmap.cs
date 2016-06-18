using System;

namespace Crayon
{
	/**
	 * Wraps a System.Drawing.Bitmap in Windows or a MonoMac.CoreGraphics Bitmap on a Mac.
	 */
	public class SystemBitmap
	{
		private System.Drawing.Bitmap bitmap;

		public int Width { get; set; }
		public int Height { get; set; }

		public SystemBitmap(string filepath)
		{
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
		}

		public SystemBitmap(int width, int height)
		{
			this.bitmap = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			this.bitmap.SetResolution(96, 96);
			this.Width = width;
			this.Height = height;
		}

		public void Save(string path)
		{
			this.bitmap.Save(path);
		}

		public Graphics MakeGraphics()
		{
			return new Graphics(this);
		}

		public class Graphics
		{
			private System.Drawing.Graphics systemGraphics;

			public Graphics(SystemBitmap owner)
			{
				this.systemGraphics = System.Drawing.Graphics.FromImage(owner.bitmap);
			}

			public void Blit(SystemBitmap bmp, int x, int y)
			{
				this.systemGraphics.DrawImageUnscaled(bmp.bitmap, x, y);
			}
		}
	}
}
