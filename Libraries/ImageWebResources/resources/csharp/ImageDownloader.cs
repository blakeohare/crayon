using System;

namespace %%%PROJECT_ID%%%.Library.ImageWebResources
{
    internal static class ImageDownloader
    {
        public static bool BytesToImage(object byteArrayObj, object[] output)
        {
            byte[] bytes = (byte[])byteArrayObj;
            try
            {
                System.Drawing.Bitmap image = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromStream(new System.IO.MemoryStream(bytes));
                output[0] = image;
                output[1] = image.Width;
                output[2] = image.Height;

            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }
    }
}
