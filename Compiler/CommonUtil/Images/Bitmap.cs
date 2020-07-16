using CommonUtil.Disk;
using CommonUtil.Random;
using System.Collections.Generic;

namespace CommonUtil.Images
{
    public enum ImageFormat
    {
        PNG,
        JPEG,
    }

    /**
     * Wraps a System.Drawing.Bitmap in Windows or a MonoMac.CoreGraphics Bitmap on a Mac.
     */
    public class Bitmap
    {
        private System.Drawing.Bitmap bitmap;
        public int Width { get; set; }
        public int Height { get; set; }

        public Bitmap(string filepath)
        {
            this.bitmap = new System.Drawing.Bitmap(filepath.Replace('/', '\\'));
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

        public Bitmap(byte[] bytes)
        {
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(bytes))
            {
                this.bitmap = new System.Drawing.Bitmap(ms);
            }
            this.bitmap.SetResolution(96, 96);
            this.Width = this.bitmap.Width;
            this.Height = this.bitmap.Height;
        }

        public Bitmap(int width, int height)
        {
            this.Width = width;
            this.Height = height;
            this.bitmap = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            this.bitmap.SetResolution(96, 96);
        }

        public Bitmap CloneToNewSize(int width, int height)
        {
            Bitmap newBitmap = new Bitmap(width, height);
            Graphics g = newBitmap.MakeGraphics();
            if (width == this.Width && height == this.Height)
            {
                g.Blit(this, 0, 0);
            }
            else
            {
                g.Blit(this, 0, 0, width, height);
            }
            return newBitmap;
        }

        public void Save(string path)
        {
            if (path.ToLowerInvariant().EndsWith(".ico"))
            {
                IconGenerator ico = new IconGenerator();
                ico.AddImage(this);
                FileUtil.WriteFileBytes(path, ico.GenerateIconFile());
            }
            else
            {
                this.bitmap.Save(path);
            }
        }

        private static string FormatToExtension(ImageFormat format)
        {
            switch (format)
            {
                case ImageFormat.PNG: return ".png";
                case ImageFormat.JPEG: return ".jpg";
                default: throw new System.Exception();
            }
        }

        public byte[] SaveBytes(ImageFormat format)
        {
            string seed = IdGenerator.GetRandomSeed();
            string file = FileUtil.JoinPath(
                FileUtil.GetTempDirectory(),
                "crayon-" + IdGenerator.Generate32HexDigits(seed, "image") + FormatToExtension(format));
            this.Save(file);
            byte[] bytes = FileUtil.ReadFileBytes(file);
            FileUtil.DeleteFile(file);
            return bytes;
        }

        public Graphics MakeGraphics()
        {
            return new Graphics(this);
        }

        public class Graphics
        {
            private System.Drawing.Graphics systemGraphics;

            public Graphics(Bitmap owner)
            {
                this.systemGraphics = System.Drawing.Graphics.FromImage(owner.bitmap);
            }

            public void Blit(Bitmap bmp, int x, int y)
            {
                this.systemGraphics.DrawImageUnscaled(bmp.bitmap, x, y);
            }

            public void Blit(Bitmap bmp, int x, int y, int stretchWidth, int stretchHeight)
            {
                this.systemGraphics.DrawImage(bmp.bitmap, x, y, stretchWidth, stretchHeight);
            }
        }
    }
}
