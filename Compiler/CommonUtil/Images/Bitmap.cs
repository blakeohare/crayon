using CommonUtil.Disk;
using CommonUtil.Random;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Drawing.Processing.Processors.Drawing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors;

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
        private Image<Rgba32> bitmap;

        public int Width { get; set; }
        public int Height { get; set; }

        public Bitmap(string filepath)
        {
            this.bitmap = Image.Load<Rgba32>(filepath.Replace('/', System.IO.Path.DirectorySeparatorChar));
            this.Width = this.bitmap.Width;
            this.Height = this.bitmap.Height;
        }

        public Bitmap(byte[] bytes)
        {
            this.bitmap = Image.Load(bytes);
            this.Width = this.bitmap.Width;
            this.Height = this.bitmap.Height;
        }

        public Bitmap(int width, int height)
        {
            this.Width = width;
            this.Height = height;
            this.bitmap = new Image<Rgba32>(width, height);
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
                g.BlitStretched(this, 0, 0, width, height);
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
            // TODO: for Windows you can save a stream. Need to look into Cairo, but this may be the only way for OSX.
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
            private Image<Rgba32> target;

            public Graphics(Bitmap owner)
            {
                this.target = owner.bitmap;
            }

            public void Blit(Bitmap bmp, int x, int y)
            {
                int sourceLeft = 0;
                int sourceTop = 0;
                int sourceRight = bmp.Width - 1;
                int sourceBottom = bmp.Height - 1;
                int targetLeft = x;
                int targetTop = y;
                int targetRight = x + bmp.Width - 1;
                int targetBottom = y + bmp.Height - 1;
                if (targetLeft >= this.target.Width ||
                    targetTop >= this.target.Height) return;
                if (targetRight < 0 ||
                    targetBottom < 0) return;
                while (targetLeft < 0)
                {
                    targetLeft++;
                    sourceLeft++;
                }
                while (targetTop < 0)
                {
                    targetTop++;
                    sourceTop++;
                }
                while (targetRight >= this.target.Width)
                {
                    targetRight--;
                    sourceRight--;
                }
                while (targetBottom >= this.target.Height)
                {
                    targetBottom--;
                    sourceBottom--;
                }

                int copyWidth = sourceRight - sourceLeft + 1;
                int copyHeight = sourceBottom - sourceTop + 1;
                if (copyWidth <= 0 || copyHeight <= 0) return;
                int px, py;

                for (py = 0; py < copyHeight; ++py)
                {
                    for (px = 0; px < copyWidth; ++px)
                    {
                        Rgba32 o = bmp.bitmap[sourceLeft + px, sourceTop + py];
                        this.target[targetLeft + px, targetTop + py] = o;
                    }
                }
            }

            public void BlitStretched(Bitmap bmp, int x, int y, int stretchWidth, int stretchHeight)
            {
                //int sourceLeft = 0;
                //int sourceTop = 0;
                int sourceRight = bmp.Width;
                int sourceBottom = bmp.Height;
                int targetLeft = x;
                int targetTop = y;
                int targetRight = x + stretchWidth;
                int targetBottom = y + stretchHeight;
                if (stretchWidth <= 0 || stretchHeight <= 0) return;

                int targetWidth = targetRight - targetLeft;
                int targetHeight = targetBottom - targetTop;
                int targetX, targetY, sourceX, sourceY;
                for (y = 0; y < stretchHeight; ++y)
                {
                    targetY = y + targetLeft;
                    sourceY = y * bmp.Height / stretchHeight;
                    if (targetY < 0 || targetY >= this.target.Height) continue;

                    for (x = 0; x < stretchWidth; ++x)
                    {
                        targetX = x + targetTop;
                        sourceX = x * bmp.Width / stretchWidth;
                        if (targetX < 0 || targetX >= this.target.Width) continue;
                        this.target[targetX, targetY] = bmp.bitmap[sourceX, sourceY];
                    }
                }
            }
        }
    }
}
