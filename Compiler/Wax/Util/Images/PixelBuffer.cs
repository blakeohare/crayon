﻿using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Wax.Util.Images
{
    public class PixelBuffer
    {
        private int counter = 1;
        private int left = -1;
        private int top = -1;
        private int right = -1;
        private int bottom = -1;
        private Rgba32[] pixels;
        private int[] counters;
        private int width;
        private int capacity;
        private Image<Rgba32> bitmap;

        public PixelBuffer(Image<Rgba32> bitmap, int width, int height)
        {
            this.bitmap = bitmap;
            this.capacity = width * height;
            this.width = width;
            this.pixels = new Rgba32[this.capacity];
            this.counters = new int[this.capacity];
            this.Reset();
        }

        public void Reset()
        {
            this.counter++;
            if (this.counter > 2000000)
            {
                this.counters = new int[this.capacity];
                this.counter = 1;
            }
            this.left = -1;
        }

        public void SetPixel(int x, int y, Rgba32 color)
        {
            if (this.left == -1)
            {
                this.left = x;
                this.right = x + 1;
                this.top = y;
                this.bottom = y + 1;
            }
            else
            {
                this.left = Math.Min(x, this.left);
                this.right = Math.Max(x + 1, this.right);
                this.top = Math.Min(y, this.top);
                this.bottom = Math.Max(y + 1, this.bottom);
            }

            int index = this.width * y + x;
            this.counters[index] = this.counter;
            this.pixels[index] = color;
        }

        public void BlitStretched(Image<Rgba32> dest, Image<Rgba32> src, int x, int y, int stretchWidth, int stretchHeight)
        {
            int sourceWidth = src.Width;
            int sourceHeight = src.Height;
            int originalTargetLeft = x;
            int originalTargetTop = y;
            int targetLeft = Math.Max(0, originalTargetLeft);
            int targetTop = Math.Max(0, originalTargetTop);
            int originalTargetRight = originalTargetLeft + stretchWidth;
            int originalTargetBottom = originalTargetTop + stretchHeight;
            int targetRight = Math.Min(dest.Width, originalTargetRight);
            int targetBottom = Math.Min(dest.Height, originalTargetBottom);

            if (stretchWidth <= 0 || stretchHeight <= 0) return;

            int targetX, targetY, sourceX, sourceY, index, weight1, weight2, totalWeight;
            Rgba32 color = src[0, 0];
            Rgba32 underColor = color;
            int counter = this.counter;

            if (this.left == -1)
            {
                this.left = targetLeft;
                this.right = targetRight;
                this.top = targetTop;
                this.bottom = targetBottom;
            }
            else
            {
                this.left = Math.Min(targetLeft, this.left);
                this.right = Math.Max(targetRight, this.right);
                this.top = Math.Min(targetTop, this.top);
                this.bottom = Math.Max(targetBottom, this.bottom);
            }

            for (targetY = targetTop; targetY < targetBottom; ++targetY)
            {
                sourceY = (targetY - originalTargetTop) * sourceHeight / stretchHeight;
                index = targetLeft + targetY * this.width;

                for (targetX = targetLeft; targetX < targetRight; ++targetX)
                {
                    sourceX = (targetX - originalTargetLeft) * sourceWidth / stretchWidth;
                    color = src[sourceX, sourceY];
                    if (color.A == 255)
                    {
                        this.pixels[index] = color;
                        this.counters[index] = counter;
                    }
                    else if (color.A > 0)
                    {
                        underColor = (this.counters[index] == counter) ? this.pixels[index] : src[sourceX, sourceY];
                        if (underColor.A == 0)
                        {
                            this.pixels[index] = color;
                        }
                        else
                        {
                            weight1 = underColor.A;
                            weight2 = color.A;
                            totalWeight = weight1 + weight2;
                            this.pixels[index] = new Rgba32(
                                (byte)((underColor.R * weight1 + color.R * weight2) / totalWeight),
                                (byte)((underColor.G * weight1 + color.G * weight2) / totalWeight),
                                (byte)((underColor.B * weight1 + color.B * weight2) / totalWeight),
                                (byte)(255 - (255 - weight1) * (255 - weight2) / 255));
                        }
                        this.counters[index] = counter;
                    }
                    index += 1;
                }
            }
        }

        public void Flush()
        {
            if (this.left != -1)
            {
                int x, y;
                int index;
                for (y = this.top; y < this.bottom; ++y)
                {
                    index = y * this.width + this.left;
                    for (x = this.left; x < this.right; ++x)
                    {
                        if (this.counters[index] == this.counter)
                        {
                            this.bitmap[x, y] = this.pixels[index];
                        }
                        index += 1;
                    }
                }
            }
        }
    }
}
