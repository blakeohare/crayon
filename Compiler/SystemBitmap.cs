using System.Collections.Generic;

namespace Crayon
{
    /**
     * Wraps a System.Drawing.Bitmap in Windows or a MonoMac.CoreGraphics Bitmap on a Mac.
     */
    public class SystemBitmap
    {
#if WINDOWS
        private System.Drawing.Bitmap bitmap;
#elif OSX
        private readonly Cairo.ImageSurface bitmap;
#endif

        public int Width { get; set; }
        public int Height { get; set; }

        public SystemBitmap(string filepath)
        {
#if WINDOWS
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
#elif OSX
            this.bitmap = new Cairo.ImageSurface(filepath);
            this.Width = this.bitmap.Width;
            this.Height = this.bitmap.Height;
#endif
        }

        public SystemBitmap(int width, int height)
        {
            this.Width = width;
            this.Height = height;
#if WINDOWS
            this.bitmap = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            this.bitmap.SetResolution(96, 96);
#elif OSX
            this.bitmap = new Cairo.ImageSurface(Cairo.Format.ARGB32, width, height);
#endif
        }

        public void Save(string path)
        {
#if WINDOWS
            this.bitmap.Save(path);
#elif OSX
            this.bitmap.WriteToPng(path);
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
            private readonly Cairo.Context context;
#endif

            public Graphics(SystemBitmap owner)
            {
#if WINDOWS
                this.systemGraphics = System.Drawing.Graphics.FromImage(owner.bitmap);
#elif OSX
                this.context = new Cairo.Context(owner.bitmap);
#endif
                undisposed.Add(this);
            }

            public void Blit(SystemBitmap bmp, int x, int y)
            {
#if WINDOWS
                this.systemGraphics.DrawImageUnscaled(bmp.bitmap, x, y);
#elif OSX

                this.context.SetSource(bmp.bitmap, x, y);
                this.context.Paint();
#endif
            }

            private static List<Graphics> undisposed = new List<Graphics>();

            public void Cleanup()
            {
#if WINDOWS

#elif OSX
                this.context.Dispose();
#endif
                undisposed.Remove(this);
            }

            public static void EnsureCleanedUp()
            {
                if (undisposed.Count > 0)
                {
                    // Did not call Cleanup() on a graphics instance.
                    throw new System.InvalidOperationException();
                }
            }
        }
    }
}
