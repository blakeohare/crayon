using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Libraries.Zip
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

        public static Value lib_zip_ensureValidArchiveInfo(VmContext vm, Value[] args)
        {
            int sc = 0;
            if ((args[0].type != 5))
            {
                sc = 1;
            }
            if (((sc == 0) && (lib_zip_validateByteList(args[1], false) != null)))
            {
                sc = 2;
            }
            return Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, sc);
        }

        public static Value lib_zip_initializeZipReader(VmContext vm, Value[] args)
        {
            int sc = 0;
            int[] byteArray = lib_zip_validateByteList(args[1], true);
            if ((byteArray == null))
            {
                sc = 1;
            }
            else
            {
                ObjectInstance obj = (ObjectInstance)args[0].internalValue;
                obj.nativeData = new object[1];
                obj.nativeData[0] = ZipHelper.CreateZipReader(byteArray);
                obj.nativeData[1] = 0;
                if ((obj.nativeData[0] == null))
                {
                    sc = 2;
                }
            }
            return Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, sc);
        }

        public static Value lib_zip_readerPeekNextEntry(VmContext vm, Value[] args)
        {
            ObjectInstance obj = (ObjectInstance)args[0].internalValue;
            object[] nd = obj.nativeData;
            ListImpl output = (ListImpl)args[1].internalValue;
            bool[] boolOut = new bool[3];
            string[] nameOut = new string[1];
            List<int> integers = new List<int>();
            ZipHelper.ReadNextZipEntry(nd[0], (int)nd[1], boolOut, nameOut, integers);
            bool problemsEncountered = !boolOut[0];
            bool foundAnything = boolOut[1];
            bool isDirectory = boolOut[2];
            if (problemsEncountered)
            {
                return vm.globalFalse;
            }
            nd[1] = (1 + (int)nd[1]);
            Interpreter.Vm.CrayonWrapper.setItemInList(output, 0, Interpreter.Vm.CrayonWrapper.buildBoolean(vm.globals, foundAnything));
            if (!foundAnything)
            {
                return vm.globalTrue;
            }
            Interpreter.Vm.CrayonWrapper.setItemInList(output, 1, Interpreter.Vm.CrayonWrapper.buildString(vm.globals, nameOut[0]));
            if (isDirectory)
            {
                Interpreter.Vm.CrayonWrapper.setItemInList(output, 2, Interpreter.Vm.CrayonWrapper.buildBoolean(vm.globals, isDirectory));
                return vm.globalTrue;
            }
            ListImpl byteValues = (ListImpl)Interpreter.Vm.CrayonWrapper.getItemFromList(output, 3).internalValue;
            int length = integers.Count;
            int i = 0;
            Value[] positiveNumbers = vm.globals.positiveIntegers;
            Value[] valuesOut = new Value[length];
            i = 0;
            while ((i < length))
            {
                valuesOut[i] = positiveNumbers[integers[i]];
                i += 1;
            }
            byteValues.array = valuesOut;
            byteValues.capacity = length;
            byteValues.size = length;
            return vm.globalTrue;
        }

        public static int[] lib_zip_validateByteList(Value byteListValue, bool convert)
        {
            if ((byteListValue.type != 6))
            {
                return null;
            }
            int[] output = null;
            ListImpl bytes = (ListImpl)byteListValue.internalValue;
            int length = bytes.size;
            if (convert)
            {
                output = new int[length];
            }
            else
            {
                output = new int[1];
                output[0] = 1;
            }
            Value value = null;
            int b = 0;
            int i = 0;
            while ((i < length))
            {
                value = bytes.array[i];
                if ((value.type != 3))
                {
                    return null;
                }
                b = (int)value.internalValue;
                if ((b > 255))
                {
                    return null;
                }
                if ((b < 0))
                {
                    if ((b >= -128))
                    {
                        b += 255;
                    }
                    else
                    {
                        return null;
                    }
                }
                if (convert)
                {
                    output[i] = b;
                }
                i += 1;
            }
            return output;
        }
    }
}
