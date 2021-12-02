using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Wax.Util.Disk;

namespace Wax.Util.Images
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
            this.bitmap = SafeLoad(filepath, null);
            this.Width = this.bitmap.Width;
            this.Height = this.bitmap.Height;
        }

        public Bitmap(byte[] bytes)
        {
            this.bitmap = SafeLoad(null, bytes);
            this.Width = this.bitmap.Width;
            this.Height = this.bitmap.Height;
        }

        private static Image<Rgba32> SafeLoad(string path, byte[] orBytesIfYouPrefer)
        {
            if (path != null) path = path.Replace('/', System.IO.Path.DirectorySeparatorChar);
            try
            {
                if (path != null)
                {
                    return Image.Load<Rgba32>(path);
                }
                else
                {
                    return Image.Load(orBytesIfYouPrefer);
                }
            }
            catch (System.Exception) { }

            if (path != null)
            {
                throw new System.InvalidOperationException("The following image resource is invalid: '" + path + "'");
            }

            throw new System.InvalidOperationException("An invalid image was loaded from bytes.");
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

        private static readonly System.Random random = new System.Random();
        private static string GenerateGibberish(int length)
        {
            string chars = "0123456789abcdefghijklmnopqrstuvwxyz";
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < length; i++)
            {
                sb.Append(chars[random.Next(chars.Length)]);
            }
            return sb.ToString();
        }

        public byte[] SaveBytes(ImageFormat format)
        {
            // TODO: for Windows you can save a stream. Need to look into Cairo, but this may be the only way for OSX.
            string file = FileUtil.JoinPath(
                FileUtil.GetTempDirectory(),
                "crayon-" + GenerateGibberish(32) + FormatToExtension(format));
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
                this.BlitStretched(bmp, x, y, bmp.Width, bmp.Height);
            }

            public void BlitStretched(Bitmap bmp, int x, int y, int stretchWidth, int stretchHeight)
            {
                int sourceWidth = bmp.Width;
                int sourceHeight = bmp.Height;
                int originalTargetLeft = x;
                int originalTargetTop = y;
                int targetLeft = System.Math.Max(0, originalTargetLeft);
                int targetTop = System.Math.Max(0, originalTargetTop);
                int originalTargetRight = originalTargetLeft + stretchWidth;
                int originalTargetBottom = originalTargetTop + stretchHeight;
                int targetRight = System.Math.Min(this.target.Width, originalTargetRight);
                int targetBottom = System.Math.Min(this.target.Height, originalTargetBottom);

                if (stretchWidth <= 0 || stretchHeight <= 0) return;

                int targetX, targetY, sourceX, sourceY;

                for (targetY = targetTop; targetY < targetBottom; ++targetY)
                {
                    sourceY = (targetY - originalTargetTop) * sourceHeight / stretchHeight;

                    for (targetX = targetLeft; targetX < targetRight; ++targetX)
                    {
                        sourceX = (targetX - originalTargetLeft) * sourceWidth / stretchWidth;
                        this.target[targetX, targetY] = bmp.bitmap[sourceX, sourceY];
                    }
                }
            }
        }
    }
}
