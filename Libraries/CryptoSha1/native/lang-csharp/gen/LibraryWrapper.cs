using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Libraries.CryptoSha1
{
    public static class LibraryWrapper
    {
        public static int lib_cryptosha1_bitShiftRight(int value, int amount)
        {
            if ((amount == 0))
            {
                return value;
            }
            int mask = 2147483647;
            value = (value & lib_cryptosha1_uint32Hack(65535, 65535));
            if ((value > 0))
            {
                return (value >> amount);
            }
            return (((value >> amount)) & ((mask >> ((amount - 1)))));
        }

        public static int lib_cryptosha1_bitwiseNot(int x)
        {
            return (-x - 1);
        }

        public static int lib_cryptosha1_createWordsForBlock(int startIndex, List<int> byteList, int[] mWords)
        {
            int i = 0;
            while ((i < 64))
            {
                mWords[(i >> 2)] = (((byteList[(startIndex + i)] << 24)) | ((byteList[(startIndex + i + 1)] << 16)) | ((byteList[(startIndex + i + 2)] << 8)) | (byteList[(startIndex + i + 3)]));
                i += 4;
            }
            return 0;
        }

        public static Value lib_cryptosha1_digest(VmContext vm, Value[] args)
        {
            ObjectInstance obj = (ObjectInstance)args[0].internalValue;
            ListImpl output = (ListImpl)args[1].internalValue;
            List<int> byteList = (List<int>)obj.nativeData[0];
            int[] resultBytes = lib_cryptosha1_digestImpl(byteList);
            int i = 0;
            while ((i < 20))
            {
                int b = resultBytes[i];
                Interpreter.Vm.CrayonWrapper.addToList(output, vm.globals.positiveIntegers[b]);
                i += 1;
            }
            return args[1];
        }

        public static int[] lib_cryptosha1_digestImpl(List<int> inputBytes)
        {
            int originalLength = (inputBytes.Count * 8);
            int h0 = lib_cryptosha1_uint32Hack(26437, 8961);
            int h1 = lib_cryptosha1_uint32Hack(61389, 43913);
            int h2 = lib_cryptosha1_uint32Hack(39098, 56574);
            int h3 = lib_cryptosha1_uint32Hack(4146, 21622);
            int h4 = lib_cryptosha1_uint32Hack(50130, 57840);
            inputBytes.Add(128);
            while (((inputBytes.Count % 64) != 56))
            {
                inputBytes.Add(0);
            }
            inputBytes.Add(0);
            inputBytes.Add(0);
            inputBytes.Add(0);
            inputBytes.Add(0);
            inputBytes.Add(((originalLength >> 24) & 255));
            inputBytes.Add(((originalLength >> 16) & 255));
            inputBytes.Add(((originalLength >> 8) & 255));
            inputBytes.Add(((originalLength >> 0) & 255));
            int[] mWords = new int[80];
            int mask32 = lib_cryptosha1_uint32Hack(65535, 65535);
            int f = 0;
            int temp = 0;
            int k = 0;
            int[] kValues = new int[4];
            kValues[0] = lib_cryptosha1_uint32Hack(23170, 31129);
            kValues[1] = lib_cryptosha1_uint32Hack(28377, 60321);
            kValues[2] = lib_cryptosha1_uint32Hack(36635, 48348);
            kValues[3] = lib_cryptosha1_uint32Hack(51810, 49622);
            int chunkIndex = 0;
            while ((chunkIndex < inputBytes.Count))
            {
                lib_cryptosha1_createWordsForBlock(chunkIndex, inputBytes, mWords);
                int i = 16;
                while ((i < 80))
                {
                    mWords[i] = lib_cryptosha1_leftRotate((mWords[(i - 3)] ^ mWords[(i - 8)] ^ mWords[(i - 14)] ^ mWords[(i - 16)]), 1);
                    i += 1;
                }
                int a = h0;
                int b = h1;
                int c = h2;
                int d = h3;
                int e = h4;
                int j = 0;
                while ((j < 80))
                {
                    if ((j < 20))
                    {
                        f = (((b & c)) | ((lib_cryptosha1_bitwiseNot(b) & d)));
                        k = kValues[0];
                    }
                    else
                    {
                        if ((j < 40))
                        {
                            f = (b ^ c ^ d);
                            k = kValues[1];
                        }
                        else
                        {
                            if ((j < 60))
                            {
                                f = (((b & c)) | ((b & d)) | ((c & d)));
                                k = kValues[2];
                            }
                            else
                            {
                                f = (b ^ c ^ d);
                                k = kValues[3];
                            }
                        }
                    }
                    temp = (lib_cryptosha1_leftRotate(a, 5) + f + e + k + mWords[j]);
                    e = d;
                    d = c;
                    c = lib_cryptosha1_leftRotate(b, 30);
                    b = a;
                    a = (temp & mask32);
                    j += 1;
                }
                h0 = (((h0 + a)) & mask32);
                h1 = (((h1 + b)) & mask32);
                h2 = (((h2 + c)) & mask32);
                h3 = (((h3 + d)) & mask32);
                h4 = (((h4 + e)) & mask32);
                chunkIndex += 64;
            }
            int[] output = new int[20];
            output[0] = ((h0 >> 24) & 255);
            output[1] = ((h0 >> 16) & 255);
            output[2] = ((h0 >> 8) & 255);
            output[3] = (h0 & 255);
            output[4] = ((h1 >> 24) & 255);
            output[5] = ((h1 >> 16) & 255);
            output[6] = ((h1 >> 8) & 255);
            output[7] = (h1 & 255);
            output[8] = ((h2 >> 24) & 255);
            output[9] = ((h2 >> 16) & 255);
            output[10] = ((h2 >> 8) & 255);
            output[11] = (h2 & 255);
            output[12] = ((h3 >> 24) & 255);
            output[13] = ((h3 >> 16) & 255);
            output[14] = ((h3 >> 8) & 255);
            output[15] = (h3 & 255);
            output[16] = ((h4 >> 24) & 255);
            output[17] = ((h4 >> 16) & 255);
            output[18] = ((h4 >> 8) & 255);
            output[19] = (h4 & 255);
            return output;
        }

        public static int lib_cryptosha1_leftRotate(int value, int amt)
        {
            if ((amt == 0))
            {
                return value;
            }
            int a = (value << amt);
            int b = lib_cryptosha1_bitShiftRight(value, (32 - amt));
            int result = (a | b);
            return result;
        }

        public static int lib_cryptosha1_uint32Hack(int left, int right)
        {
            return (((left << 16)) | right);
        }
    }
}
