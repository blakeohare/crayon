using System;
using System.Collections.Generic;
using Interpreter.Structs;

namespace Interpreter.Libraries.ImageEncoder
{
    internal static class ImageEncoderUtil
    {
        /*
            Format:
                1 - PNG
                2 - JPEG
            
            Return status codes:
                0 - OK
        */
        public static int Encode(object imageObj, int format, List<Value> output, Value[] bytesAsValues)
        {
            System.Drawing.Bitmap image = (System.Drawing.Bitmap)imageObj;

            byte[] bytes = null;
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            switch (format)
            {
                case 1:
                    image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    bytes = ms.ToArray();
                    break;

                case 2:
                    image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    bytes = ms.ToArray();
                    break;

                default:
                    throw new Exception(); // Library code sent an invalid format flag.
            }
            int length = bytes.Length;
            int i = 0;
            while (i < length)
            {
                output.Add(bytesAsValues[bytes[i++]]);
            }

            return 0;
        }
    }
}
