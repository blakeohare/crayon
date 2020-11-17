using System;
using System.Collections.Generic;
using Interpreter.Structs;

namespace Interpreter.Vm
{
    internal static class TextEncodingHelper
    {
        public static int BytesToText(int[] unwrappedBytes, int format, string[] strOut)
        {
            int length = unwrappedBytes.Length;
            byte[] bytes = new byte[length];
            int t;
            switch (format)
            {
                case 1:

                    for (int i = 0; i < length; i += 1)
                    {
                        t = unwrappedBytes[i];
                        if (t >= 128) return 4;
                        bytes[i] = (byte)t;
                    }
                    break;
                case 5:
                    for (int i = 0; i < length; i += 2)
                    {
                        bytes[i] = (byte)unwrappedBytes[i | 1];
                        bytes[i | 1] = (byte)unwrappedBytes[i];
                    }
                    break;

                case 7:
                    for (int i = 0; i < length; i += 4)
                    {
                        bytes[i] = (byte)unwrappedBytes[i | 3];
                        bytes[i | 1] = (byte)unwrappedBytes[i | 2];
                        bytes[i | 2] = (byte)unwrappedBytes[i | 1];
                        bytes[i | 3] = (byte)unwrappedBytes[i];
                    }
                    break;

                default:
                    for (int i = 0; i < length; ++i)
                    {
                        bytes[i] = (byte)unwrappedBytes[i];
                    }
                    break;
            }

            try
            {
                switch (format)
                {
                    case 1:
                    case 2:
                        strOut[0] = System.Text.Encoding.ASCII.GetString(bytes);
                        break;
                    case 3:
                        strOut[0] = System.Text.Encoding.UTF8.GetString(bytes);
                        break;
                    case 4:
                    case 5:
                        strOut[0] = System.Text.Encoding.Unicode.GetString(bytes);
                        break;
                    case 6:
                    case 7:
                        strOut[0] = System.Text.Encoding.UTF32.GetString(bytes);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            catch (Exception)
            {
                return 4;
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
                    char[] chars = value.ToCharArray();
                    int limit = format == 1 ? 128 : 256;
                    for (int i = 0; i < chars.Length; ++i)
                    {
                        if (chars[i] >= limit) return 1;
                    }
                    bytes = System.Text.Encoding.ASCII.GetBytes(chars);
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
