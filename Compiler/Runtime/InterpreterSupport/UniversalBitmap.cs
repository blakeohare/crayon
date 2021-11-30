using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Interpreter
{
    public enum ImageFormat
    {
        PNG,
        JPEG,
    }

    public class UniversalBitmap
    {
        private Image<Rgba32> bitmap;
        private DrawingSession activeDrawingSession = null;
        private PixelBuffer pixelBuffer = null;
        public bool IsValid { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public UniversalBitmap(string filepath)
        {
            SafeLoad(filepath, null);
        }

        public UniversalBitmap(byte[] bytes)
        {
            SafeLoad(null, bytes);
        }

        private void SafeLoad(string path, byte[] orBytesIfYouPrefer)
        {
            if (path != null) path = path.Replace('/', System.IO.Path.DirectorySeparatorChar);
            try
            {
                if (path != null)
                {
                    this.bitmap = Image.Load<Rgba32>(path);
                }
                else
                {
                    this.bitmap = Image.Load(orBytesIfYouPrefer);
                }
                this.IsValid = true;
                this.Width = this.bitmap.Width;
                this.Height = this.bitmap.Height;
            }
            catch (System.Exception)
            {
                this.IsValid = false;
            }
        }

        public UniversalBitmap(int width, int height)
        {
            this.IsValid = true;
            this.Width = width;
            this.Height = height;
            this.bitmap = new Image<Rgba32>(width, height);
        }

        public byte[] GetBytesAsPng()
        {
            return this.SaveBytes(ImageFormat.PNG);
        }

        public byte[] GetBytesAsJpeg()
        {
            return this.SaveBytes(ImageFormat.JPEG);
        }

        public UniversalBitmap CloneToNewSize(int width, int height)
        {
            UniversalBitmap newBitmap = new UniversalBitmap(width, height);
            DrawingSession g = newBitmap.CreateNewDrawingSession();
            if (width == this.Width && height == this.Height)
            {
                g.Blit(this, 0, 0);
            }
            else
            {
                g.BlitStretched(this, 0, 0, width, height);
            }
            g.Flush();
            newBitmap.activeDrawingSession = null;
            newBitmap.pixelBuffer = null;
            return newBitmap;
        }

        public void Save(string path)
        {
            this.bitmap.Save(path);
        }

        private byte[] SaveBytes(ImageFormat format)
        {
            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            switch (format)
            {
                case ImageFormat.PNG:
                    this.bitmap.SaveAsPng(stream);
                    break;
                case ImageFormat.JPEG:
                    this.bitmap.SaveAsJpeg(stream);
                    break;
                default:
                    throw new System.NotImplementedException();
            }
            return stream.ToArray();
        }

        public DrawingSession CreateNewDrawingSession()
        {
            if (this.activeDrawingSession != null) return this.activeDrawingSession;
            this.activeDrawingSession = new DrawingSession(this);
            return this.activeDrawingSession;
        }

        public void GetPixel(int x, int y, int[] colorOut)
        {
            Rgba32 color = this.bitmap[x, y];
            colorOut[0] = color.R;
            colorOut[1] = color.G;
            colorOut[2] = color.B;
            colorOut[3] = color.A;
        }

        public void ClearBuffer()
        {
            this.pixelBuffer = null;
        }

        public class DrawingSession
        {
            private PixelBuffer pixelBuffer = null;
            public UniversalBitmap Parent { get; private set; }

            public DrawingSession Draw(
                UniversalBitmap bmp,
                int targetX, int targetY,
                int sourceX, int sourceY,
                int targetWidth, int targetHeight,
                int sourceWidth, int sourceHeight)
            {
                if (sourceX == 0 && sourceY == 0 && sourceWidth == bmp.Width && sourceHeight == bmp.Height)
                {
                    // attempting to draw the whole src onto the dest
                    this.BlitStretched(bmp, targetX, targetY, targetWidth, targetHeight);
                }
                else
                {
                    throw new System.NotImplementedException();
                }
                return this;
            }

            public DrawingSession SetPixel(int x, int y, int r, int g, int b, int a)
            {
                this.pixelBuffer.SetPixel(x, y, new Rgba32((byte)r, (byte)g, (byte)b, (byte)a));
                return this;
            }

            public void Flush()
            {
                this.pixelBuffer.Flush();
            }

            public DrawingSession(UniversalBitmap owner)
            {
                this.Parent = owner;
                this.pixelBuffer = owner.pixelBuffer ?? new PixelBuffer(owner.bitmap, owner.Width, owner.Height);
                owner.pixelBuffer = this.pixelBuffer;
                this.pixelBuffer.Reset();
            }

            public void Blit(UniversalBitmap bmp, int x, int y)
            {
                this.BlitStretched(bmp, x, y, bmp.Width, bmp.Height);
            }

            public void BlitStretched(UniversalBitmap bmp, int x, int y, int stretchWidth, int stretchHeight)
            {
                this.pixelBuffer.BlitStretched(this.Parent.bitmap, bmp.bitmap, x, y, stretchWidth, stretchHeight);
            }
        }
    }
}
