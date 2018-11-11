using System;

namespace Interpreter.Libraries.ImageWebResources
{
    internal static class ImageDownloader
    {
        public static bool BytesToImage(object byteArrayObj, object[] output)
        {
            byte[] bytes = (byte[])byteArrayObj;
            try
            {
                System.Drawing.Bitmap image;
                using (System.IO.MemoryStream stream = new System.IO.MemoryStream(bytes))
                {
                    image = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromStream(stream);
                }
                output[0] = image;
                output[1] = image.Width;
                output[2] = image.Height;
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
