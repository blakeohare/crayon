using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Libraries.Http
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

        public static Value lib_http_fastEnsureAllBytes(VmContext vm, Value[] args)
        {
            if ((args[0].type == 6))
            {
                ListImpl list1 = (ListImpl)args[0].internalValue;
                int i = list1.size;
                int int1 = 0;
                int[] intArray1 = new int[i];
                Value value = null;
                while ((i > 0))
                {
                    i -= 1;
                    value = list1.array[i];
                    if ((value.type != 3))
                    {
                        return vm.globalFalse;
                    }
                    int1 = (int)value.internalValue;
                    if ((int1 < 0))
                    {
                        if ((int1 < -128))
                        {
                            return vm.globalFalse;
                        }
                        int1 += 256;
                    }
                    else
                    {
                        if ((int1 >= 256))
                        {
                            return vm.globalFalse;
                        }
                    }
                    intArray1[i] = int1;
                }
                object[] objArray1 = new object[1];
                objArray1[0] = intArray1;
                ObjectInstance objInstance1 = (ObjectInstance)args[1].internalValue;
                objInstance1.nativeData = objArray1;
                return vm.globalTrue;
            }
            return vm.globalFalse;
        }

        public static Value lib_http_getResponseBytes(VmContext vm, Value[] args)
        {
            Value outputListValue = args[1];
            ObjectInstance objInstance1 = (ObjectInstance)args[0].internalValue;
            object[] objArray1 = objInstance1.nativeData;
            List<Value> tList = new List<Value>();
            HttpHelper.GetResponseBytes(objArray1[0], vm.globals.positiveIntegers, tList);
            ListImpl outputList = (ListImpl)outputListValue.internalValue;
            Value t = Interpreter.Vm.CrayonWrapper.buildList(tList);
            ListImpl otherList = (ListImpl)t.internalValue;
            outputList.capacity = otherList.capacity;
            outputList.array = otherList.array;
            outputList.size = tList.Count;
            return outputListValue;
        }

        public static Value lib_http_pollRequest(VmContext vm, Value[] args)
        {
            ObjectInstance objInstance1 = (ObjectInstance)args[0].internalValue;
            object[] objArray1 = objInstance1.nativeData;
            if (HttpHelper.PollRequest(objArray1))
            {
                return vm.globalTrue;
            }
            return vm.globalFalse;
        }

        public static Value lib_http_populateResponse(VmContext vm, Value[] args)
        {
            Value arg2 = args[1];
            Value arg3 = args[2];
            Value arg4 = args[3];
            ObjectInstance objInstance1 = (ObjectInstance)args[0].internalValue;
            object object1 = objInstance1.nativeData[0];
            object[] objArray1 = new object[1];
            List<string> stringList1 = new List<string>();
            HttpHelper.ReadResponseData(object1, PST_IntBuffer16, PST_StringBuffer16, objArray1, stringList1);
            objInstance1 = (ObjectInstance)arg2.internalValue;
            objInstance1.nativeData = objArray1;
            ListImpl outputList = (ListImpl)arg3.internalValue;
            Interpreter.Vm.CrayonWrapper.addToList(outputList, Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, PST_IntBuffer16[0]));
            Interpreter.Vm.CrayonWrapper.addToList(outputList, Interpreter.Vm.CrayonWrapper.buildString(vm.globals, PST_StringBuffer16[0]));
            Value value = vm.globalNull;
            Value value2 = vm.globalTrue;
            if ((PST_IntBuffer16[1] == 0))
            {
                value = Interpreter.Vm.CrayonWrapper.buildString(vm.globals, PST_StringBuffer16[1]);
                value2 = vm.globalFalse;
            }
            Interpreter.Vm.CrayonWrapper.addToList(outputList, value);
            Interpreter.Vm.CrayonWrapper.addToList(outputList, value2);
            ListImpl list1 = (ListImpl)arg4.internalValue;
            int i = 0;
            while ((i < stringList1.Count))
            {
                Interpreter.Vm.CrayonWrapper.addToList(list1, Interpreter.Vm.CrayonWrapper.buildString(vm.globals, stringList1[i]));
                i += 1;
            }
            return vm.globalNull;
        }

        public static Value lib_http_sendRequest(VmContext vm, Value[] args)
        {
            Value body = args[5];
            ObjectInstance objInstance1 = (ObjectInstance)args[0].internalValue;
            object[] objArray1 = new object[3];
            objInstance1.nativeData = objArray1;
            objArray1[2] = false;
            string method = (string)args[2].internalValue;
            string url = (string)args[3].internalValue;
            List<string> headers = new List<string>();
            ListImpl list1 = (ListImpl)args[4].internalValue;
            int i = 0;
            while ((i < list1.size))
            {
                headers.Add((string)list1.array[i].internalValue);
                i += 1;
            }
            object bodyRawObject = body.internalValue;
            int bodyState = 0;
            if ((body.type == 5))
            {
                bodyState = 1;
            }
            else
            {
                if ((body.type == 8))
                {
                    objInstance1 = (ObjectInstance)bodyRawObject;
                    bodyRawObject = objInstance1.nativeData[0];
                    bodyState = 2;
                }
                else
                {
                    bodyRawObject = null;
                }
            }
            bool getResponseAsText = ((int)args[6].internalValue == 1);
            if ((bool)args[1].internalValue)
            {
                HttpHelper.SendRequestAsync(objArray1, method, url, headers, bodyState, bodyRawObject, getResponseAsText);
            }
            else
            {
                if (HttpHelper.SendRequestSync(objArray1, method, url, headers, bodyState, bodyRawObject, getResponseAsText))
                {
                    Interpreter.Vm.CrayonWrapper.vm_suspend(vm, 1);
                }
            }
            return vm.globalNull;
        }
    }
}