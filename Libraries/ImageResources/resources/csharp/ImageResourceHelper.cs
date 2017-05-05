using System;
using System.Collections.Generic;

namespace Interpreter.Libraries.ImageResources
{
    internal static class ImageResourceHelper
    {
        public static void CheckLoaderIsDone(
            object[] imageLoaderNativeData,
            object[] nativeImageDataNativeData,
            List<Value> output)
        {
            int status = 0;
            lock (imageLoaderNativeData[3])
            {
                status = (int)imageLoaderNativeData[2];
            }

            output[0] = CrayonWrapper.v_buildInteger(status);
        }

        public static void LoadAsync(
            string filename,
            object[] nativeImageDataNativeData,
            object[] imageLoaderNativeData)
        {
            imageLoaderNativeData[3] = new object();
            System.ComponentModel.BackgroundWorker bgw = new System.ComponentModel.BackgroundWorker();
            bgw.DoWork += (sender, args) =>
            {
                bool loaded = LoadSync(filename, nativeImageDataNativeData, null);

                lock (imageLoaderNativeData[3])
                {
                    imageLoaderNativeData[2] = loaded ? 1 : 2;
                }
            };

            bgw.RunWorkerAsync();
        }

        public static bool LoadSync(string filename, object[] nativeImageDataNativeData, List<Value> statusOutCheesy)
        {
            Android.Graphics.Bitmap nativeBmp = ResourceReader.ReadImageFile("ImageSheets/" + filename);
            if (nativeBmp != null)
            {
                AndroidBitmap bmp = new AndroidBitmap(nativeBmp);
                if (statusOutCheesy != null) statusOutCheesy.Reverse();
                nativeImageDataNativeData[0] = bmp;
                nativeImageDataNativeData[1] = bmp.Width;
                nativeImageDataNativeData[2] = bmp.Height;
                return true;
            }
            return false;
        }

        public static string GetManifestString()
        {
            return ResourceReader.ReadTextResource("imageSheetManifest.txt");
        }
    }
}
