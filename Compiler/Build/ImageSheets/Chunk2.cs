using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using CommonUtil.Collections;
using CommonUtil.Images;

namespace Build.ImageSheets
{
    class Chunk2
    {
        public int ID { get; set; }

        public int MaxWidth { get; set; }
        public int MaxHeight { get; set; }

        public List<Image2> Images { get; set; }
        public List<int> X { get; set; }
        public List<int> Y { get; set; }

        public Bitmap FinalizedBitmap { get; set; }

        private int currentRowTop;
        private int currentRowBottom;
        private int currentX;

        private static string PadInt(int value)
        {
            string output = value + "";
            while (output.Length < 7) output = "0" + output;
            return output;
        }

        public Chunk2(int width, int height)
        {
            this.MaxWidth = width;
            this.MaxHeight = height;
            this.Images = new List<Image2>();
            this.X = new List<int>();
            this.Y = new List<int>();
        }

        public bool AttemptAllocation(IEnumerable<Image2> images)
        {
            this.currentRowTop = 0;
            this.currentRowBottom = 0;
            this.currentX = 0;
            Image2[] sortedImages = images.OrderBy(p => PadInt(999999 - p.Height) + "," + p.Path).ToArray();
            for (int i = 0; i < sortedImages.Length; ++i)
            {
                Image2 image = sortedImages[i];

                if (image.Width > this.MaxWidth) throw new Exception(); // this shouldn't happen.

                if (this.currentX + image.Width > this.MaxWidth)
                {
                    this.currentX = 0;
                    this.currentRowTop = this.currentRowBottom;
                }

                int newBottom = this.currentRowTop + image.Height;
                if (newBottom > this.currentRowBottom)
                {
                    this.currentRowBottom = newBottom;
                    if (newBottom > this.MaxHeight) return false;
                }
                this.Images.Add(image);
                this.X.Add(this.currentX);
                this.Y.Add(this.currentRowTop);

                this.currentX += image.Width;
            }
            this.size = null;
            return true;
        }

        public int Width { get { return this.GetSize()[0]; } }
        public int Height {  get { return this.GetSize()[1]; } }

        private int[] size = null;
        private int[] GetSize()
        {
            if (this.size != null) return this.size;
            int width = 0;
            int height = 0;
            for (int i = 0; i < this.Images.Count; ++i)
            {
                int right = this.Images[i].Width + this.X[i];
                int bottom = this.Images[i].Height + this.Y[i];
                width = right > width ? right : width;
                height = bottom > height ? bottom : height;
            }
            return new int[] { width, height };
        }
    }
}
