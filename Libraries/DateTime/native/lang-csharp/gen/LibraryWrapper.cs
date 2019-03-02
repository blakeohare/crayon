using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Libraries.DateTime
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

        public static object lib_datetime_getNativeTimezone(Value value)
        {
            ObjectInstance tzObj = (ObjectInstance)value.internalValue;
            if ((tzObj.nativeData == null))
            {
                return null;
            }
            return tzObj.nativeData[0];
        }

        public static Value lib_datetime_getUtcOffsetAt(VmContext vm, Value[] args)
        {
            object nativeTz = lib_datetime_getNativeTimezone(args[0]);
            int unixTime = (int)args[1].internalValue;
            int offsetSeconds = DateTimeHelper.GetUtcOffsetAt(nativeTz, unixTime);
            return Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, offsetSeconds);
        }

        public static Value lib_datetime_initTimeZone(VmContext vm, Value[] args)
        {
            ObjectInstance timezone = (ObjectInstance)args[0].internalValue;
            timezone.nativeData = new object[1];
            object nativeTzRef = null;
            string readableName = null;
            int offsetFromUtc = 0;
            int isDstObserved = 0;
            string fingerprint = null;
            if ((args[1].type == 1))
            {
                string[] strOut = PST_StringBuffer16;
                int[] intOut = PST_IntBuffer16;
                nativeTzRef = DateTimeHelper.GetDataForLocalTimeZone(strOut, intOut);
                readableName = strOut[0];
                fingerprint = strOut[1];
                offsetFromUtc = intOut[0];
                isDstObserved = intOut[1];
            }
            else
            {
                return vm.globalNull;
            }
            timezone.nativeData = new object[5];
            timezone.nativeData[0] = nativeTzRef;
            timezone.nativeData[1] = readableName;
            timezone.nativeData[2] = offsetFromUtc;
            timezone.nativeData[3] = (isDstObserved == 1);
            timezone.nativeData[4] = fingerprint;
            List<Value> values = new List<Value>();
            values.Add(Interpreter.Vm.CrayonWrapper.buildString(vm.globals, readableName));
            values.Add(Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, offsetFromUtc));
            values.Add(Interpreter.Vm.CrayonWrapper.buildBoolean(vm.globals, (isDstObserved == 1)));
            values.Add(Interpreter.Vm.CrayonWrapper.buildString(vm.globals, fingerprint));
            return Interpreter.Vm.CrayonWrapper.buildList(values);
        }

        public static Value lib_datetime_initTimeZoneList(VmContext vm, Value[] args)
        {
            ObjectInstance obj = (ObjectInstance)args[0].internalValue;
            obj.nativeData = new object[1];
            object[] timezones = DateTimeHelper.InitializeTimeZoneList();
            obj.nativeData[0] = timezones;
            int length = timezones.Length;
            return Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, length);
        }

        public static Value lib_datetime_isDstOccurringAt(VmContext vm, Value[] args)
        {
            object nativeTz = lib_datetime_getNativeTimezone(args[0]);
            int unixtime = (int)args[1].internalValue;
            return Interpreter.Vm.CrayonWrapper.buildBoolean(vm.globals, DateTimeHelper.IsDstOccurringAt(nativeTz, unixtime));
        }

        public static Value lib_datetime_parseDate(VmContext vm, Value[] args)
        {
            int year = (int)args[0].internalValue;
            int month = (int)args[1].internalValue;
            int day = (int)args[2].internalValue;
            int hour = (int)args[3].internalValue;
            int minute = (int)args[4].internalValue;
            int microseconds = (int)args[5].internalValue;
            object nullableTimeZone = lib_datetime_getNativeTimezone(args[6]);
            if (((year >= 1970) && (year < 2100) && (month >= 1) && (month <= 12) && (day >= 1) && (day <= 31) && (hour >= 0) && (hour < 24) && (minute >= 0) && (minute < 60) && (microseconds >= 0) && (microseconds < 60000000)))
            {
                int[] intOut = PST_IntBuffer16;
                DateTimeHelper.ParseDate(intOut, nullableTimeZone, year, month, day, hour, minute, microseconds);
                if ((intOut[0] == 1))
                {
                    double unixFloat = (intOut[1] + (intOut[2]) / (1000000.0));
                    return Interpreter.Vm.CrayonWrapper.buildFloat(vm.globals, unixFloat);
                }
            }
            return vm.globalNull;
        }

        public static Value lib_datetime_unixToStructured(VmContext vm, Value[] args)
        {
            double unixTime = (double)args[0].internalValue;
            object nullableTimeZone = lib_datetime_getNativeTimezone(args[1]);
            List<Value> output = new List<Value>();
            int[] intOut = PST_IntBuffer16;
            bool success = DateTimeHelper.UnixToStructured(intOut, nullableTimeZone, unixTime);
            if (!success)
            {
                return vm.globalNull;
            }
            int i = 0;
            while ((i < 9))
            {
                output.Add(Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, intOut[i]));
                i += 1;
            }
            return Interpreter.Vm.CrayonWrapper.buildList(output);
        }
    }
}
