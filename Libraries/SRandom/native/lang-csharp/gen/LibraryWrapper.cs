using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Libraries.SRandom
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

        public static Value lib_srandom_getBoolean(VmContext vm, Value[] args)
        {
            ListImpl intPtr = (ListImpl)args[0].internalValue;
            int value = 0;
            value = ((((int)intPtr.array[0].internalValue * 20077) + 12345) & 65535);
            intPtr.array[0] = Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, value);
            if (((value & 1) == 0))
            {
                return vm.globalFalse;
            }
            return vm.globalTrue;
        }

        public static Value lib_srandom_getFloat(VmContext vm, Value[] args)
        {
            ListImpl intPtr = (ListImpl)args[0].internalValue;
            int value1 = 0;
            value1 = ((((int)intPtr.array[0].internalValue * 20077) + 12345) & 65535);
            int value2 = (((value1 * 20077) + 12345) & 65535);
            int value3 = (((value2 * 20077) + 12345) & 65535);
            intPtr.array[0] = Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, value3);
            value1 = ((value1 >> 8) & 255);
            value2 = ((value2 >> 8) & 255);
            value3 = ((value3 >> 8) & 255);
            return Interpreter.Vm.CrayonWrapper.buildFloat(vm.globals, (((value1 << 16) | (value2 << 8) | value3)) / (16777216.0));
        }

        public static Value lib_srandom_getInteger(VmContext vm, Value[] args)
        {
            ListImpl intPtr = (ListImpl)args[0].internalValue;
            int value1 = 0;
            value1 = ((((int)intPtr.array[0].internalValue * 20077) + 12345) & 65535);
            int value2 = (((value1 * 20077) + 12345) & 65535);
            int value3 = (((value2 * 20077) + 12345) & 65535);
            int value4 = (((value3 * 20077) + 12345) & 65535);
            intPtr.array[0] = Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, value4);
            value1 = ((value1 >> 8) & 255);
            value2 = ((value2 >> 8) & 255);
            value3 = ((value3 >> 8) & 255);
            value4 = ((value4 >> 8) & 127);
            return Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, ((value4 << 24) | (value3 << 16) | (value2 << 8) | value1));
        }
    }
}
