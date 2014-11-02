using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon
{
	public static class HackUtil
	{
		// Java does not recognize the alpha channel on a small subset of PNG encodings.
		// Re-encode them before copying them to a Java project.
		public static byte[] ReEncodePngImageForJava(string filepath)
		{
			System.Drawing.Image img = System.Drawing.Bitmap.FromFile(filepath);

			switch (img.PixelFormat)
			{
				case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
				case System.Drawing.Imaging.PixelFormat.Format32bppPArgb:
				case System.Drawing.Imaging.PixelFormat.Format16bppArgb1555:
				case System.Drawing.Imaging.PixelFormat.Format64bppArgb:
				case System.Drawing.Imaging.PixelFormat.Format64bppPArgb:
					System.Drawing.Bitmap newBmp = new System.Drawing.Bitmap(
						img.Width,
						img.Height,
						System.Drawing.Imaging.PixelFormat.Format32bppArgb);
					System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(newBmp);
					g.DrawImage(img, 0, 0, img.Width, img.Height);
					g.Flush();
					g.Dispose();
					string tempFile = System.IO.Path.GetTempFileName();
					newBmp.Save(tempFile);
					byte[] output = System.IO.File.ReadAllBytes(tempFile);
					System.IO.File.Delete(tempFile);
					return output;
				case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
				default:
					return null;
			}
		}
	}
}
