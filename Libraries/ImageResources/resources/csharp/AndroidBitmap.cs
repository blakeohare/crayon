using System;
using System.Collections.Generic;

namespace Interpreter.Libraries.ImageResources
{
    public class AndroidBitmap
    {
        private Android.Graphics.Bitmap bmp;
        private Android.Graphics.Canvas canvas;
        public int Width { get; set; }
        public int Height { get; set; }

        public AndroidBitmap(int width, int height)
        {
            this.bmp = Android.Graphics.Bitmap.CreateBitmap(
                width,
                height,
                Android.Graphics.Bitmap.Config.Argb4444);
            this.Width = width;
            this.Height = height;
        }

        public AndroidBitmap(Android.Graphics.Bitmap nativeBitmap)
        {
            this.bmp = nativeBitmap;
            this.Width = nativeBitmap.Width;
            this.Height = nativeBitmap.Height;
        }

        public void Blit(
            object otherBmpObj,
            int targetX,
            int targetY,
            int sourceX,
            int sourceY,
            int width,
            int height)
        {
            AndroidBitmap otherBmp = (AndroidBitmap)otherBmpObj;
            this.GetCanvas().DrawBitmap(otherBmp.bmp, targetX, targetY, null);
        }

        public Android.Graphics.Bitmap GetNativeBitmap()
        {
            return this.bmp;
        }

        public Android.Graphics.Canvas GetCanvas()
        {
            if (this.canvas == null)
            {
                this.canvas = new Android.Graphics.Canvas(this.bmp);
            }
            return this.canvas;
        }
    }
}
