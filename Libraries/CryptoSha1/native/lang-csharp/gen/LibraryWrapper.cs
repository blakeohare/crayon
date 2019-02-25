using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Libraries.CryptoSha1
{
    public static class LibraryWrapper
    {
        private static readonly int[] PST_IntBuffer16 = new int[16];
        private static readonly double[] PST_FloatBuffer16 = new double[16];
        private static readonly string[] PST_StringBuffer16 = new string[16];
        private static readonly System.Random PST_Random = new System.Random();

        public static bool AlwaysTrue() { return true; }
        public static bool AlwaysFalse() { return false; }

        public static string PST_StringReverse(string value)
        {
            if (value.Length < 2) return value;
            char[] chars = value.ToCharArray();
            return new string(chars.Reverse().ToArray());
        }

        private static readonly string[] PST_SplitSep = new string[1];
        private static string[] PST_StringSplit(string value, string sep)
        {
            if (sep.Length == 1) return value.Split(sep[0]);
            if (sep.Length == 0) return value.ToCharArray().Select<char, string>(c => "" + c).ToArray();
            PST_SplitSep[0] = sep;
            return value.Split(PST_SplitSep, System.StringSplitOptions.None);
        }

        private static string PST_FloatToString(double value)
        {
            string output = value.ToString();
            if (output[0] == '.') output = "0" + output;
            if (!output.Contains('.')) output += ".0";
            return output;
        }

        private static readonly System.DateTime PST_UnixEpoch = new System.DateTime(1970, 1, 1);
        private static double PST_CurrentTime
        {
            get { return System.DateTime.UtcNow.Subtract(PST_UnixEpoch).TotalSeconds; }
        }

        private static string PST_Base64ToString(string b64Value)
        {
            byte[] utf8Bytes = System.Convert.FromBase64String(b64Value);
            string value = System.Text.Encoding.UTF8.GetString(utf8Bytes);
            return value;
        }

        // TODO: use a model like parse float to avoid double parsing.
        public static bool PST_IsValidInteger(string value)
        {
            if (value.Length == 0) return false;
            char c = value[0];
            if (value.Length == 1) return c >= '0' && c <= '9';
            int length = value.Length;
            for (int i = c == '-' ? 1 : 0; i < length; ++i)
            {
                c = value[i];
                if (c < '0' || c > '9') return false;
            }
            return true;
        }

        public static void PST_ParseFloat(string strValue, double[] output)
        {
            double num = 0.0;
            output[0] = double.TryParse(strValue, out num) ? 1 : -1;
            output[1] = num;
        }

        private static List<T> PST_ListConcat<T>(List<T> a, List<T> b)
        {
            List<T> output = new List<T>(a.Count + b.Count);
            output.AddRange(a);
            output.AddRange(b);
            return output;
        }

        private static List<Value> PST_MultiplyList(List<Value> items, int times)
        {
            List<Value> output = new List<Value>(items.Count * times);
            while (times-- > 0) output.AddRange(items);
            return output;
        }

        private static bool PST_SubstringIsEqualTo(string haystack, int index, string needle)
        {
            int needleLength = needle.Length;
            if (index + needleLength > haystack.Length) return false;
            if (needleLength == 0) return true;
            if (haystack[index] != needle[0]) return false;
            if (needleLength == 1) return true;
            for (int i = 1; i < needleLength; ++i)
            {
                if (needle[i] != haystack[index + i]) return false;
            }
            return true;
        }

        private static void PST_ShuffleInPlace<T>(List<T> list)
        {
            if (list.Count < 2) return;
            int length = list.Count;
            int tIndex;
            T tValue;
            for (int i = length - 1; i >= 0; --i)
            {
                tIndex = PST_Random.Next(length);
                tValue = list[tIndex];
                list[tIndex] = list[i];
                list[i] = tValue;
            }
        }

        public static int lib_cryptosha1_bitShiftRight(int value, int amount)
        {
            if ((amount == 0))
            {
                return value;
            }
            if ((value > 0))
            {
                return (value >> amount);
            }
            int mask = 2147483647;
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
