using CommonUtil.Images;
using CommonUtil.Resources;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Platform
{
    // Add some image sources
    // Add some desired sizes
    // It generates them using the best size match (where "best" is basically any size that's >= the desired size)
    public class IconSetGenerator
    {
        private HashSet<int> outputSizes = new HashSet<int>();
        private List<Bitmap> bitmaps = new List<Bitmap>();
        private List<int> bitmapMaxDimenion = new List<int>();

        // Temporary images whose lifetimes are owned by the icon generator.
        private HashSet<Bitmap> ownedBitmapReferences = new HashSet<Bitmap>();

        public IconSetGenerator() { }

        public IconSetGenerator AddOutputSize(int size)
        {
            this.outputSizes.Add(size);
            return this;
        }

        public IconSetGenerator AddInputImage(Bitmap bmp)
        {
            if (bmp.Width != bmp.Height)
            {
                int size = Math.Max(bmp.Width, bmp.Height);
                Bitmap newBmp = new Bitmap(size, size);
                this.ownedBitmapReferences.Add(newBmp);
                Bitmap.Graphics g = newBmp.MakeGraphics();
                int x = (size - bmp.Width) / 2;
                int y = (size - bmp.Height) / 2;
                g.Blit(bmp, x, y);
                bmp = newBmp;
            }
            this.bitmaps.Add(bmp);
            this.bitmapMaxDimenion.Add(Math.Max(bmp.Width, bmp.Height));
            return this;
        }

        public Dictionary<int, Bitmap> GenerateWithDefaultFallback()
        {
            if (this.bitmaps.Count == 0)
            {
                byte[] bytes = new ResourceStore(typeof(IconSetGenerator)).ReadAssemblyFileBytes("icons/crayon_logo.png");
                Bitmap defaultIcon = new Bitmap(bytes);
                this.ownedBitmapReferences.Add(defaultIcon);
                this.AddInputImage(defaultIcon);
            }

            return Generate();
        }

        public Dictionary<int, Bitmap> Generate()
        {
            Dictionary<int, Bitmap> lookup = new Dictionary<int, Bitmap>();

            Bitmap[] sources = this.bitmaps.OrderBy(b => -b.Width).ToArray();

            foreach (int desiredSize in this.outputSizes)
            {
                Bitmap source = this.FindBestMatch(sources, desiredSize);
                Bitmap bmp = new Bitmap(desiredSize, desiredSize);
                Bitmap.Graphics g = bmp.MakeGraphics();
                g.BlitStretched(source, 0, 0, desiredSize, desiredSize);
                lookup.Add(desiredSize, bmp);
            }

            return lookup;
        }

        private Bitmap FindBestMatch(Bitmap[] imagesFromLargestToSmallest, int desiredSize)
        {
            if (imagesFromLargestToSmallest.Length == 0) return null;
            Bitmap best = imagesFromLargestToSmallest[0];
            if (imagesFromLargestToSmallest.Length == 1) return best;

            for (int i = 1; i < imagesFromLargestToSmallest.Length; ++i)
            {
                Bitmap current = imagesFromLargestToSmallest[i];
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
