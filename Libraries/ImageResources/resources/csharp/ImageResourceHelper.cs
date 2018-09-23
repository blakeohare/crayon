using Interpreter.Structs;
using System.Collections.Generic;

namespace Interpreter.Libraries.ImageResources
{
    internal static class ImageResourceHelper
    {
        public static int CheckLoaderIsDone(object[] imageLoaderNativeData, object[] nativeImageDataNativeData)
        {
            int status = 0;
            lock (imageLoaderNativeData[3])
            {
                status = (int)imageLoaderNativeData[2];
            }

            return status;
        }

        public static void ImageLoadAsync(
            string filename,
            object[] nativeImageDataNativeData,
            object[] imageLoaderNativeData)
        {
            imageLoaderNativeData[3] = new object();
            System.ComponentModel.BackgroundWorker bgw = new System.ComponentModel.BackgroundWorker();
            bgw.DoWork += (sender, args) =>
            {
                bool loaded = ImageLoadSync(filename, nativeImageDataNativeData);

                lock (imageLoaderNativeData[3])
                {
                    imageLoaderNativeData[2] = loaded ? 1 : 2;
                }
            };

            bgw.RunWorkerAsync();
        }

        private static bool ImageLoadSync(string filename, object[] nativeImageDataNativeData)
        {
            UniversalBitmap bmp = ResourceReader.ReadImageResource(filename);
            if (bmp != null)
            {
                nativeImageDataNativeData[0] = bmp;
                nativeImageDataNativeData[1] = bmp.Width;
                nativeImageDataNativeData[2] = bmp.Height;
                return true;
            }
            return false;
        }

        public static object GenerateNativeBitmapOfSize(int width, int height)
        {
            return new UniversalBitmap(width, height);
        }

        public static void BlitImage(
            object targetBmp, object sourceBmp,
            int targetX, int targetY,
            int sourceX, int sourceY,
            int width, int height,
            object graphicsSession)
        {
            UniversalBitmap target = (UniversalBitmap)targetBmp;
            UniversalBitmap source = (UniversalBitmap)sourceBmp;
            ((UniversalBitmap.DrawingSession)graphicsSession).Draw(source, targetX, targetY, sourceX, sourceY, width, height);
        }

        public static object GetPixelEditSession(object nativeImageResource)
        {
            return ((UniversalBitmap)nativeImageResource).GetActiveDrawingSession();
        }

        public static void FlushPixelEditSession(object graphicsObj)
        {
            ((UniversalBitmap.DrawingSession)graphicsObj).Flush();
        }
    }
}
