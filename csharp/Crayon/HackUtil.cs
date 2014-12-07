using System.Drawing;
using System.Drawing.Imaging;

namespace Crayon
{
	public static class HackUtil
	{
		// Java does not recognize the alpha channel on a small subset of PNG encodings.
		// Re-encode them before copying them to a Java project.
		public static System.Drawing.Bitmap ReEncodePngImageForJava(string filepath)
		{
			Image img = Bitmap.FromFile(filepath);
			switch (img.PixelFormat)
			{
				case PixelFormat.Format32bppArgb:
				case PixelFormat.Format32bppPArgb:
				case PixelFormat.Format16bppArgb1555:
				case PixelFormat.Format64bppArgb:
				case PixelFormat.Format64bppPArgb:
					Bitmap newBmp = new Bitmap(img.Width, img.Height, PixelFormat.Format32bppArgb);
					Graphics g = Graphics.FromImage(newBmp);
					g.DrawImage(img, 0, 0, img.Width, img.Height);
					g.Flush();
					g.Dispose();
					return newBmp;
				case PixelFormat.Format24bppRgb:
				default:
					return null;
			}
		}
	}
}
