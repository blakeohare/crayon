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
        public void CheesyCleanup()
        {
#if OSX
            // There seems to be a bug in Cairo where the reference counting isn't decremented correctly.
            // However, making manual multiple redundant calls to cairo_surface_destroy will (safely?) decrement
            // the reference count.
            // TODO: track references to Bitmap and verify all have had CheesyCleanup called on it when
            // the program exits. Be sure to set the bitmap field to null so it doesn't act too much like a memory leak.
            System.IntPtr handle = this.bitmap.Handle;
            int refCount = (int)this.bitmap.ReferenceCount;
            this.bitmap.Dispose();
            refCount -= 1;
            if (refCount <= 0)
            {
                System.Type nativeMethods = typeof(Cairo.Surface).Assembly.GetType("Cairo.NativeMethods");
                System.Reflection.MethodInfo surfaceDestroyFp = nativeMethods.GetMethod("cairo_surface_destroy", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                for (int i = 0; i < refCount; ++i)
                {
                    surfaceDestroyFp.Invoke(null, new object[] { handle });
                }
            }
#endif
        }

#if WINDOWS
        private System.Drawing.Bitmap bitmap;
#elif OSX
        // TODO: determine what the reasoning behind this readonly was.
        private readonly Cairo.ImageSurface bitmap;
#endif

        public int Width { get; set; }
        public int Height { get; set; }

        public Bitmap(string filepath)
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

        public Bitmap(byte[] bytes)
        {
#if WINDOWS
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(bytes))
            {
                this.bitmap = new System.Drawing.Bitmap(ms);
            }
            this.bitmap.SetResolution(96, 96);
            this.Width = this.bitmap.Width;
            this.Height = this.bitmap.Height;
#elif OSX
            string tempDir = FileUtil.GetTempDirectory();
            string tempFile = FileUtil.JoinPath(tempDir, "crayon_image_temp.png");
            FileUtil.WriteFileBytes(tempFile, bytes);
            this.bitmap = new Cairo.ImageSurface(tempFile);
            FileUtil.DeleteFile(tempFile);
            this.Width = this.bitmap.Width;
            this.Height = this.bitmap.Height;
#endif
        }

        public Bitmap(int width, int height)
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
            g.Cleanup();
            return newBitmap;
        }

        public void Save(string path)
        {
            if (path.ToLower().EndsWith(".ico"))
            {
                IconGenerator ico = new IconGenerator();
                ico.AddImage(this);
                FileUtil.WriteFileBytes(path, ico.GenerateIconFile());
            }
            else
            {
#if WINDOWS
                this.bitmap.Save(path);
#elif OSX
                this.bitmap.WriteToPng(path);
#endif
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
#if WINDOWS
            private System.Drawing.Graphics systemGraphics;
#elif OSX
            private readonly Cairo.Context context;
            private readonly Bitmap sysBmp;
#endif

            public Graphics(Bitmap owner)
            {
#if WINDOWS
                this.systemGraphics = System.Drawing.Graphics.FromImage(owner.bitmap);
#elif OSX
                this.sysBmp = owner;
                this.context = new Cairo.Context(owner.bitmap);
#endif
                undisposed.Add(this);
            }

            public void Blit(Bitmap bmp, int x, int y)
            {
#if WINDOWS
                this.systemGraphics.DrawImageUnscaled(bmp.bitmap, x, y);
#elif OSX

                this.context.SetSource(bmp.bitmap, x, y);
                this.context.Paint();
#endif
            }

            public void Blit(Bitmap bmp, int x, int y, int stretchWidth, int stretchHeight)
            {
#if WINDOWS
                this.systemGraphics.DrawImage(bmp.bitmap, x, y, stretchWidth, stretchHeight);
#elif OSX
                this.context.Scale(1.0 * this.sysBmp.Width / bmp.bitmap.Width, 1.0 * this.sysBmp.Height / bmp.bitmap.Height);
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
