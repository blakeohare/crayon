using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Libraries.Nori
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

        public static Value lib_nori_closeFrame(VmContext vm, Value[] args)
        {
            ObjectInstance frameObj = (ObjectInstance)args[0].internalValue;
            object nativeFrameHandle = frameObj.nativeData[0];
            NoriHelper.CloseFrame(nativeFrameHandle);
            return vm.globalNull;
        }

        public static string lib_nori_encodeListToWireFormat(Value v)
        {
            ListImpl args = (ListImpl)v.internalValue;
            List<string> sb = new List<string>();
            List<Value> valueList = null;
            Value[] valueArray = null;
            int i = 0;
            int blindCopy = 0;
            int intValue = 0;
            int length = args.size;
            valueArray = args.array;
            blindCopy = (2 + (int)valueArray[1].internalValue);
            i = 0;
            while ((i < blindCopy))
            {
                if ((i > 0))
                {
                    sb.Add(",");
                }
                intValue = (int)valueArray[i].internalValue;
                sb.Add((intValue).ToString());
                i += 1;
            }
            int childCount = 0;
            int propertyCount = 0;
            int j = 0;
            string key = "";
            string value = "";
            while ((i < length))
            {
                sb.Add(",");
                sb.Add((string)valueArray[i].internalValue);
                sb.Add(",");
                sb.Add(((int)valueArray[(i + 1)].internalValue).ToString());
                childCount = (int)valueArray[(i + 2)].internalValue;
                propertyCount = (int)valueArray[(i + 3)].internalValue;
                sb.Add(",");
                sb.Add((childCount).ToString());
                sb.Add(",");
                sb.Add((propertyCount).ToString());
                i += 4;
                j = 0;
                while ((j < childCount))
                {
                    sb.Add(",");
                    intValue = (int)valueArray[(i + j)].internalValue;
                    sb.Add((intValue).ToString());
                    j += 1;
                }
                i += childCount;
                j = 0;
                while ((j < propertyCount))
                {
                    key = (string)valueArray[i].internalValue;
                    value = (string)valueArray[(i + 1)].internalValue;
                    sb.Add(",");
                    sb.Add(key);
                    sb.Add(",");
                    sb.Add(NoriHelper.EscapeStringHex(value));
                    i += 2;
                    j += 1;
                }
            }
            return string.Join("", sb);
        }

        public static Value lib_nori_flushUpdatesToFrame(VmContext vm, Value[] args)
        {
            ObjectInstance frameObj = (ObjectInstance)args[0].internalValue;
            object nativeFrameHandle = frameObj.nativeData[0];
            string data = (string)args[1].internalValue;
            NoriHelper.FlushUpdatesToFrame(nativeFrameHandle, data);
            return vm.globalNull;
        }

        public static Value lib_nori_runEventWatcher(VmContext vm, Value[] args)
        {
            ObjectInstance frameObj = (ObjectInstance)args[0].internalValue;
            int execContextIdForResume = (int)args[1].internalValue;
            Value eventCallback = args[2];
            Value postShowCallback = args[3];
            ExecutionContext ec = Interpreter.Vm.CrayonWrapper.getExecutionContext(vm, execContextIdForResume);
            Interpreter.Vm.CrayonWrapper.vm_suspend_for_context(ec, 1);
            NoriHelper.EventWatcher(vm, execContextIdForResume, args[0], eventCallback, postShowCallback);
            return vm.globalNull;
        }

        public static Value lib_nori_showFrame(VmContext vm, Value[] args)
        {
            ObjectInstance frameObj = (ObjectInstance)args[0].internalValue;
            string title = (string)args[1].internalValue;
            int width = (int)args[2].internalValue;
            int height = (int)args[3].internalValue;
            string data = (string)args[4].internalValue;
            int execId = (int)args[5].internalValue;
            frameObj.nativeData = new object[1];
            frameObj.nativeData[0] = NoriHelper.ShowFrame(args[0], title, width, height, data, execId);
            return vm.globalNull;
        }
    }
}
