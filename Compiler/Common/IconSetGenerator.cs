using System;
using System.Collections.Generic;
using System.Linq;

namespace Common
{
    // Add some image sources
    // Add some desired sizes
    // It generates them using the best size match (where "best" is basically any size that's >= the desired size)
    public class IconSetGenerator
    {
        private HashSet<int> outputSizes = new HashSet<int>();
        private List<SystemBitmap> bitmaps = new List<SystemBitmap>();
        private List<int> bitmapMaxDimenion = new List<int>();

        // Temporary images whose lifetimes are owned by the icon generator.
        private HashSet<SystemBitmap> ownedBitmapReferences = new HashSet<SystemBitmap>();

        public IconSetGenerator() { }

        public IconSetGenerator AddOutputSize(int size)
        {
            this.outputSizes.Add(size);
            return this;
        }

        public IconSetGenerator AddInputImage(SystemBitmap bmp)
        {
            if (bmp.Width != bmp.Height)
            {
                int size = Math.Max(bmp.Width, bmp.Height);
                SystemBitmap newBmp = new SystemBitmap(size, size);
                this.ownedBitmapReferences.Add(newBmp);
                SystemBitmap.Graphics g = newBmp.MakeGraphics();
                int x = (size - bmp.Width) / 2;
                int y = (size - bmp.Height) / 2;
                g.Blit(bmp, x, y);
                g.Cleanup();
                bmp = newBmp;
            }
            this.bitmaps.Add(bmp);
            this.bitmapMaxDimenion.Add(Math.Max(bmp.Width, bmp.Height));
            return this;
        }

        public Dictionary<int, SystemBitmap> GenerateWithDefaultFallback()
        {
            if (this.bitmaps.Count == 0)
            {
                byte[] bytes = Util.ReadAssemblyFileBytes(typeof(Util).Assembly, "icons/crayon_logo.png");
                SystemBitmap defaultIcon = new SystemBitmap(bytes);
                this.ownedBitmapReferences.Add(defaultIcon);
                this.AddInputImage(defaultIcon);
            }

            return Generate();
        }

        public Dictionary<int, SystemBitmap> Generate()
        {
            Dictionary<int, SystemBitmap> lookup = new Dictionary<int, SystemBitmap>();

            SystemBitmap[] sources = this.bitmaps.OrderBy(b => -b.Width).ToArray();

            foreach (int desiredSize in this.outputSizes)
            {
                SystemBitmap source = this.FindBestMatch(sources, desiredSize);
                SystemBitmap bmp = new SystemBitmap(desiredSize, desiredSize);
                SystemBitmap.Graphics g = bmp.MakeGraphics();
                g.Blit(source, 0, 0, desiredSize, desiredSize);
                g.Cleanup();
                lookup.Add(desiredSize, bmp);
            }

            this.Cleanup();

            return lookup;
        }

        private void Cleanup()
        {
            foreach (SystemBitmap bmp in this.ownedBitmapReferences)
            {
                bmp.CheesyCleanup();
            }
        }

        private SystemBitmap FindBestMatch(SystemBitmap[] imagesFromLargestToSmallest, int desiredSize)
        {
            if (imagesFromLargestToSmallest.Length == 0) return null;
            SystemBitmap best = imagesFromLargestToSmallest[0];
            if (imagesFromLargestToSmallest.Length == 1) return best;

            for (int i = 1; i < imagesFromLargestToSmallest.Length; ++i)
            {
                SystemBitmap current = imagesFromLargestToSmallest[i];
                if (current.Width == desiredSize) return current;
                if (current.Width < desiredSize)
                {
                    return best;
                }
                best = current;
            }
            return best;
        }
    }
}
