using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Libraries.DateTime
{
    public static class LibraryWrapper
    {
        private static readonly string[] PST_StringBuffer16 = new string[16];

        private static readonly int[] PST_IntBuffer16 = new int[16];

        private static Dictionary<string, System.Func<object[], object>> PST_ExtCallbacks = new Dictionary<string, System.Func<object[], object>>();

        public static void PST_RegisterExtensibleCallback(string name, System.Func<object[], object> func) {
            PST_ExtCallbacks[name] = func;
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
