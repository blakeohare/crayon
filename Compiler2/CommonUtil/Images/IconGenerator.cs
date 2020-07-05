using System;
using System.Collections.Generic;
using System.Linq;

namespace CommonUtil.Images
{
    public class IconGenerator
    {
        private Dictionary<int, Bitmap> bitmaps = new Dictionary<int, Bitmap>();

        public void AddImage(Bitmap bmp)
        {
            int width = bmp.Width;
            int height = bmp.Height;
            int largest = System.Math.Max(width, height);
            if (width > 256 || height > 256) throw new InvalidOperationException("Icon images cannot be larger than 256 x 256");
            foreach (int size in new int[] { 256, 128, 64, 32 })
            {
                if (largest > size / 2)
                {
                    this.bitmaps[size] = bmp;
                    return;
                }
            }
            this.bitmaps[16] = bmp;
        }

        public byte[] GenerateIconFile()
        {
            int[] sizes = this.bitmaps.Keys.OrderBy(i => i).ToArray();
            if (sizes.Length == 0) throw new InvalidOperationException("Cannot generate empty icon file.");
            List<byte> imageBytes = new List<byte>();
            List<int> startingPositions = new List<int>();
            int startingPosition = 6 + 16 * sizes.Length; // ico header is 6 bytes and each file header is 16 bytes.
            List<byte> icoHeader = new List<byte>();
            List<byte> pngHeaders = new List<byte>();
            List<byte> pngPayloads = new List<byte>();

            ToLittleEndian(0, 2, icoHeader); // first two bytes are always 0
            ToLittleEndian(1, 2, icoHeader); // 1 for ICO format (2 is CUR)
            ToLittleEndian(sizes.Length, 2, icoHeader); // number of files

            foreach (int size in sizes)
            {
                Bitmap originalImage = this.bitmaps[size];
                int width = originalImage.Width;
                int height = originalImage.Height;
                int x = (size - width) / 2;
                int y = (size - height) / 2;
                Bitmap resource = new Bitmap(size, size);
                Bitmap.Graphics g = resource.MakeGraphics();
                g.Blit(originalImage, x, y);
                g.Cleanup();

                byte[] pngBytes = resource.SaveBytes(ImageFormat.PNG);
                pngPayloads.AddRange(pngBytes);

                ToLittleEndian(size == 256 ? 0 : size, 1, pngHeaders);
                ToLittleEndian(size == 256 ? 0 : size, 1, pngHeaders);
                ToLittleEndian(0, 1, pngHeaders); // 0 for not using a color palette
                ToLittleEndian(0, 1, pngHeaders); // reserved, always 0
                ToLittleEndian(0, 2, pngHeaders); // 0 color planes.
                ToLittleEndian(32, 2, pngHeaders); // 32 bits per pixel
                ToLittleEndian(pngBytes.Length, 4, pngHeaders); // file size in bytes
                ToLittleEndian(startingPosition, 4, pngHeaders); // byte position from the beginning of the file

                startingPosition += pngBytes.Length;
            }

            List<byte> finalOutput = icoHeader;
            finalOutput.AddRange(pngHeaders);
            finalOutput.AddRange(pngPayloads);

            return finalOutput.ToArray();
        }

        private static void ToLittleEndian(int value, int bytes, List<byte> byteBuffer)
        {
            while (bytes-- > 0)
            {
                byte b = (byte)(value & 255);
                byteBuffer.Add(b);
                value >>= 8;
            }
        }
    }
}
