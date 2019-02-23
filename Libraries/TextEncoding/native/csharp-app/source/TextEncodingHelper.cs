using System;
using System.Collections.Generic;
using Interpreter.Structs;

namespace Interpreter.Libraries.TextEncoding
{
    internal static class TextEncodingHelper
    {
        public static int BytesToText(int[] unwrappedBytes, int format, string[] strOut)
        {
            int length = unwrappedBytes.Length;
            int start = 0;
            int detectedFormat = -1;
            bool endianSwap = false;

            if (length >= 3 && unwrappedBytes[0] == 239 && unwrappedBytes[1] == 187 && unwrappedBytes[2] == 191)
            {
                detectedFormat = 2;
                start = 3;
            }
            else if (length >= 4 && unwrappedBytes[0] == 255 && unwrappedBytes[1] == 254 && unwrappedBytes[2] == 0 && unwrappedBytes[3] == 0)
            {
                detectedFormat = 4;
                start = 4;
            }
            else if (length >= 4 && unwrappedBytes[0] == 0 && unwrappedBytes[1] == 0 && unwrappedBytes[2] == 254 && unwrappedBytes[3] == 255)
            {
                detectedFormat = 4;
                start = 4;
                endianSwap = true;
            }
            else if (length >= 2 && unwrappedBytes[0] == 255 && unwrappedBytes[1] == 254)
            {
                detectedFormat = 3;
                start = 2;
            }
            else if (length >= 2 && unwrappedBytes[0] == 254 && unwrappedBytes[1] == 255)
            {
                detectedFormat = 3;
                start = 2;
                endianSwap = true;
            }

            if (detectedFormat != -1)
            {
                if (format != 0)
                {
                    if (format != detectedFormat)
                    {
                        return 4;
                    }
                }
                else
                {
                    format = detectedFormat;
                }
            }

            byte[] bytes = new byte[length - start];
            for (int i = start; i < length; ++i)
            {
                bytes[i - start] = (byte)unwrappedBytes[i];
            }

            if (format == 3 && length % 2 != 0) return 4;
            if (format == 4 && length % 4 != 0) return 4;

            if (endianSwap)
            {
                int wordSize = 2;
                if (detectedFormat == 4) wordSize = 4;
                byte swap;
                for (int i = 0; i < bytes.Length; i += wordSize)
                {
                    for (int j = 0; j < wordSize / 2; ++j)
                    {
                        swap = bytes[i + j];
                        bytes[i + j] = bytes[i + wordSize - j - 1];
                        bytes[i + wordSize - j - 1] = swap;
                    }
                }
            }

            switch (format)
            {
                case 1:
                    strOut[0] = System.Text.Encoding.ASCII.GetString(bytes);
                    break;
                case 2:
                    strOut[0] = System.Text.Encoding.UTF8.GetString(bytes);
                    break;
                case 3:
                    strOut[0] = System.Text.Encoding.Unicode.GetString(bytes);
                    break;
                case 4:
                    strOut[0] = System.Text.Encoding.UTF32.GetString(bytes);
                    break;
                default:
                    throw new NotImplementedException();
            }
            return 0;
        }

        public static int TextToBytes(
            string value,
            bool includeBom,
            int format,
            List<Value> byteList,
            Value[] positiveIntegers,
            int[] intOut)
        {
            intOut[0] = 0;
            byte[] bytes;
            switch (format)
            {
                case 1:
                case 2:
                    bytes = System.Text.Encoding.ASCII.GetBytes(value.ToCharArray());
                    break;

                case 3:
                    bytes = System.Text.Encoding.UTF8.GetBytes(value.ToCharArray());
                    break;

                case 4:
                case 5:
                    bytes = System.Text.Encoding.Unicode.GetBytes(value.ToCharArray());
                    if (format == 5)
                    {
                        intOut[0] = 2;
                    }
                    break;

                case 6:
                case 7:
                    bytes = System.Text.Encoding.UTF32.GetBytes(value.ToCharArray());
                    if (format == 7)
                    {
                        intOut[0] = 4;
                    }
                    break;

                default:
                    throw new InvalidOperationException();
            }

            if (includeBom)
            {
                switch (format)
                {
                    case 1:
                    case 2:
                        break;

                    case 3:
                        byteList.Add(positiveIntegers[239]);
                        byteList.Add(positiveIntegers[187]);
                        byteList.Add(positiveIntegers[191]);
                        break;

                    case 4:
                    case 5:
                        byteList.Add(positiveIntegers[255]);
                        byteList.Add(positiveIntegers[254]);
                        break;

                    case 6:
                    case 7:
                        byteList.Add(positiveIntegers[255]);
                        byteList.Add(positiveIntegers[254]);
                        byteList.Add(positiveIntegers[0]);
                        byteList.Add(positiveIntegers[0]);
                        break;
                }
            }

            int length = bytes.Length;
            int byteValue;
            for (int i = 0; i < length; ++i)
            {
                byteValue = bytes[i];
                if (byteValue < 0) byteValue += 256;
                byteList.Add(positiveIntegers[byteValue]);
            }

            return 0;
        }
    }
}
