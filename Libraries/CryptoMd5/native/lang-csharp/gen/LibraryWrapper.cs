using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Libraries.CryptoMd5
{
    public static class LibraryWrapper
    {
        private static Dictionary<string, System.Func<object[], object>> PST_ExtCallbacks = new Dictionary<string, System.Func<object[], object>>();

        public static void PST_RegisterExtensibleCallback(string name, System.Func<object[], object> func) {
            PST_ExtCallbacks[name] = func;
        }

        public static int lib_md5_bitShiftRight(int value, int amount)
        {
            if ((amount == 0))
            {
                return value;
            }
            value = (value & lib_md5_uint32Hack(65535, 65535));
            int mask = 2147483647;
            if ((value > 0))
            {
                return (value >> amount);
            }
            return (((value >> amount)) & ((mask >> ((amount - 1)))));
        }

        public static int lib_md5_bitwiseNot(int x)
        {
            return (-x - 1);
        }

        public static int lib_md5_createWordsForBlock(int startIndex, List<int> byteList, int[] mWords)
        {
            int i = 0;
            while ((i < 64))
            {
                mWords[(i >> 2)] = ((byteList[(startIndex + i)]) | ((byteList[(startIndex + i + 1)] << 8)) | ((byteList[(startIndex + i + 2)] << 16)) | ((byteList[(startIndex + i + 3)] << 24)));
                i += 4;
            }
            return 0;
        }

        public static Value lib_md5_digestMd5(VmContext vm, Value[] args)
        {
            ObjectInstance obj = (ObjectInstance)args[0].internalValue;
            ListImpl output = (ListImpl)args[1].internalValue;
            List<int> byteList = (List<int>)obj.nativeData[0];
            int[] resultBytes = lib_md5_digestMd5Impl(byteList);
            int i = 0;
            while ((i < 16))
            {
                int b = resultBytes[i];
                Interpreter.Vm.CrayonWrapper.addToList(output, vm.globals.positiveIntegers[b]);
                i += 1;
            }
            return args[1];
        }

        public static int[] lib_md5_digestMd5Impl(List<int> inputBytes)
        {
            int originalLength = (inputBytes.Count * 8);
            int[] shiftTable = new int[64];
            int[] K = new int[64];
            int i = 0;
            while ((i < 16))
            {
                shiftTable[i] = 7;
                shiftTable[(i + 1)] = 12;
                shiftTable[(i + 2)] = 17;
                shiftTable[(i + 3)] = 22;
                shiftTable[(i + 16)] = 5;
                shiftTable[(i + 17)] = 9;
                shiftTable[(i + 18)] = 14;
                shiftTable[(i + 19)] = 20;
                shiftTable[(i + 32)] = 4;
                shiftTable[(i + 33)] = 11;
                shiftTable[(i + 34)] = 16;
                shiftTable[(i + 35)] = 23;
                shiftTable[(i + 48)] = 6;
                shiftTable[(i + 49)] = 10;
                shiftTable[(i + 50)] = 15;
                shiftTable[(i + 51)] = 21;
                i += 4;
            }
            K[0] = lib_md5_uint32Hack(55146, 42104);
            K[1] = lib_md5_uint32Hack(59591, 46934);
            K[2] = lib_md5_uint32Hack(9248, 28891);
            K[3] = lib_md5_uint32Hack(49597, 52974);
            K[4] = lib_md5_uint32Hack(62844, 4015);
            K[5] = lib_md5_uint32Hack(18311, 50730);
            K[6] = lib_md5_uint32Hack(43056, 17939);
            K[7] = lib_md5_uint32Hack(64838, 38145);
            K[8] = lib_md5_uint32Hack(27008, 39128);
            K[9] = lib_md5_uint32Hack(35652, 63407);
            K[10] = lib_md5_uint32Hack(65535, 23473);
            K[11] = lib_md5_uint32Hack(35164, 55230);
            K[12] = lib_md5_uint32Hack(27536, 4386);
            K[13] = lib_md5_uint32Hack(64920, 29075);
            K[14] = lib_md5_uint32Hack(42617, 17294);
            K[15] = lib_md5_uint32Hack(18868, 2081);
            K[16] = lib_md5_uint32Hack(63006, 9570);
            K[17] = lib_md5_uint32Hack(49216, 45888);
            K[18] = lib_md5_uint32Hack(9822, 23121);
            K[19] = lib_md5_uint32Hack(59830, 51114);
            K[20] = lib_md5_uint32Hack(54831, 4189);
            K[21] = lib_md5_uint32Hack(580, 5203);
            K[22] = lib_md5_uint32Hack(55457, 59009);
            K[23] = lib_md5_uint32Hack(59347, 64456);
            K[24] = lib_md5_uint32Hack(8673, 52710);
            K[25] = lib_md5_uint32Hack(49975, 2006);
            K[26] = lib_md5_uint32Hack(62677, 3463);
            K[27] = lib_md5_uint32Hack(17754, 5357);
            K[28] = lib_md5_uint32Hack(43491, 59653);
            K[29] = lib_md5_uint32Hack(64751, 41976);
            K[30] = lib_md5_uint32Hack(26479, 729);
            K[31] = lib_md5_uint32Hack(36138, 19594);
            K[32] = lib_md5_uint32Hack(65530, 14658);
            K[33] = lib_md5_uint32Hack(34673, 63105);
            K[34] = lib_md5_uint32Hack(28061, 24866);
            K[35] = lib_md5_uint32Hack(64997, 14348);
            K[36] = lib_md5_uint32Hack(42174, 59972);
            K[37] = lib_md5_uint32Hack(19422, 53161);
            K[38] = lib_md5_uint32Hack(63163, 19296);
            K[39] = lib_md5_uint32Hack(48831, 48240);
            K[40] = lib_md5_uint32Hack(10395, 32454);
            K[41] = lib_md5_uint32Hack(60065, 10234);
            K[42] = lib_md5_uint32Hack(54511, 12421);
            K[43] = lib_md5_uint32Hack(1160, 7429);
            K[44] = lib_md5_uint32Hack(55764, 53305);
            K[45] = lib_md5_uint32Hack(59099, 39397);
            K[46] = lib_md5_uint32Hack(8098, 31992);
            K[47] = lib_md5_uint32Hack(50348, 22117);
            K[48] = lib_md5_uint32Hack(62505, 8772);
            K[49] = lib_md5_uint32Hack(17194, 65431);
            K[50] = lib_md5_uint32Hack(43924, 9127);
            K[51] = lib_md5_uint32Hack(64659, 41017);
            K[52] = lib_md5_uint32Hack(25947, 22979);
            K[53] = lib_md5_uint32Hack(36620, 52370);
            K[54] = lib_md5_uint32Hack(65519, 62589);
            K[55] = lib_md5_uint32Hack(34180, 24017);
            K[56] = lib_md5_uint32Hack(28584, 32335);
            K[57] = lib_md5_uint32Hack(65068, 59104);
            K[58] = lib_md5_uint32Hack(41729, 17172);
            K[59] = lib_md5_uint32Hack(19976, 4513);
            K[60] = lib_md5_uint32Hack(63315, 32386);
            K[61] = lib_md5_uint32Hack(48442, 62005);
            K[62] = lib_md5_uint32Hack(10967, 53947);
            K[63] = lib_md5_uint32Hack(60294, 54161);
            int A = lib_md5_uint32Hack(26437, 8961);
            int B = lib_md5_uint32Hack(61389, 43913);
            int C = lib_md5_uint32Hack(39098, 56574);
            int D = lib_md5_uint32Hack(4146, 21622);
            inputBytes.Add(128);
            while (((inputBytes.Count % 64) != 56))
            {
                inputBytes.Add(0);
            }
            inputBytes.Add(((originalLength >> 0) & 255));
            inputBytes.Add(((originalLength >> 8) & 255));
            inputBytes.Add(((originalLength >> 16) & 255));
            inputBytes.Add(((originalLength >> 24) & 255));
            inputBytes.Add(0);
            inputBytes.Add(0);
            inputBytes.Add(0);
            inputBytes.Add(0);
            int[] mWords = new int[16];
            int mask32 = lib_md5_uint32Hack(65535, 65535);
            int chunkIndex = 0;
            while ((chunkIndex < inputBytes.Count))
            {
                lib_md5_createWordsForBlock(chunkIndex, inputBytes, mWords);
                int A_init = A;
                int B_init = B;
                int C_init = C;
                int D_init = D;
                int j = 0;
                while ((j < 64))
                {
                    A = lib_md5_magicShuffle(mWords, K, shiftTable, mask32, A, B, C, D, j);
                    D = lib_md5_magicShuffle(mWords, K, shiftTable, mask32, D, A, B, C, (j | 1));
                    C = lib_md5_magicShuffle(mWords, K, shiftTable, mask32, C, D, A, B, (j | 2));
                    B = lib_md5_magicShuffle(mWords, K, shiftTable, mask32, B, C, D, A, (j | 3));
                    j += 4;
                }
                A = (((A_init + A)) & mask32);
                B = (((B_init + B)) & mask32);
                C = (((C_init + C)) & mask32);
                D = (((D_init + D)) & mask32);
                chunkIndex += 64;
            }
            int[] output = new int[16];
            output[0] = (A & 255);
            output[1] = ((A >> 8) & 255);
            output[2] = ((A >> 16) & 255);
            output[3] = ((A >> 24) & 255);
            output[4] = (B & 255);
            output[5] = ((B >> 8) & 255);
            output[6] = ((B >> 16) & 255);
            output[7] = ((B >> 24) & 255);
            output[8] = (C & 255);
            output[9] = ((C >> 8) & 255);
            output[10] = ((C >> 16) & 255);
            output[11] = ((C >> 24) & 255);
            output[12] = (D & 255);
            output[13] = ((D >> 8) & 255);
            output[14] = ((D >> 16) & 255);
            output[15] = ((D >> 24) & 255);
            return output;
        }

        public static int lib_md5_leftRotate(int value, int amt)
        {
            if ((amt == 0))
            {
                return value;
            }
            int a = (value << amt);
            int b = lib_md5_bitShiftRight(value, (32 - amt));
            int result = (a | b);
            return result;
        }

        public static int lib_md5_magicShuffle(int[] mWords, int[] sineValues, int[] shiftValues, int mask32, int a, int b, int c, int d, int counter)
        {
            int roundNumber = (counter >> 4);
            int t = 0;
            int shiftAmount = shiftValues[counter];
            int sineValue = sineValues[counter];
            int mWord = 0;
            if ((roundNumber == 0))
            {
                t = (((b & c)) | ((lib_md5_bitwiseNot(b) & d)));
                mWord = mWords[counter];
            }
            else if ((roundNumber == 1))
            {
                t = (((b & d)) | ((c & lib_md5_bitwiseNot(d))));
                mWord = mWords[((((5 * counter) + 1)) & 15)];
            }
            else if ((roundNumber == 2))
            {
                t = (b ^ c ^ d);
                mWord = mWords[((((3 * counter) + 5)) & 15)];
            }
            else
            {
                t = (c ^ ((b | lib_md5_bitwiseNot(d))));
                mWord = mWords[(((7 * counter)) & 15)];
            }
            t = (((a + t + mWord + sineValue)) & mask32);
            t = (b + lib_md5_leftRotate(t, shiftAmount));
            return (t & mask32);
        }

        public static int lib_md5_uint32Hack(int left, int right)
        {
            return (((left << 16)) | right);
        }
    }
}
