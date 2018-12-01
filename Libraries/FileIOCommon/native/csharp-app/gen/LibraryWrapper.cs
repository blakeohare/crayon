using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Libraries.FileIOCommon
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

        public static Value lib_fileiocommon_directoryCreate(VmContext vm, Value[] args)
        {
            bool bool1 = false;
            int i = 0;
            int int1 = 0;
            List<string> stringList1 = null;
            Value hostObject = args[0];
            string path = (string)args[1].internalValue;
            if ((bool)args[2].internalValue)
            {
                int1 = 0;
                if (!FileIOCommonHelper.DirectoryExists(FileIOCommonHelper.GetDirRoot(path)))
                {
                    int1 = 4;
                }
                else
                {
                    stringList1 = new List<string>();
                    bool1 = true;
                    while ((bool1 && !FileIOCommonHelper.DirectoryExists(path)))
                    {
                        stringList1.Add(path);
                        int1 = FileIOCommonHelper.GetDirParent(path, PST_StringBuffer16);
                        path = PST_StringBuffer16[0];
                        if ((int1 != 0))
                        {
                            bool1 = false;
                        }
                    }
                    if (bool1)
                    {
                        i = (stringList1.Count - 1);
                        while ((i >= 0))
                        {
                            path = stringList1[i];
                            int1 = FileIOCommonHelper.CreateDirectory(path);
                            if ((int1 != 0))
                            {
                                i = -1;
                            }
                            i -= 1;
                        }
                    }
                }
            }
            else
            {
                int1 = FileIOCommonHelper.CreateDirectory(path);
            }
            return Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, int1);
        }

        public static Value lib_fileiocommon_directoryDelete(VmContext vm, Value[] args)
        {
            int sc = FileIOCommonHelper.DeleteDirectory((string)args[1].internalValue);
            return Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, sc);
        }

        public static Value lib_fileiocommon_directoryList(VmContext vm, Value[] args)
        {
            Value diskhost = args[0];
            string path = (string)args[1].internalValue;
            bool useFullPath = (bool)args[2].internalValue;
            ListImpl outputList = (ListImpl)args[3].internalValue;
            List<string> stringList1 = new List<string>();
            int sc = FileIOCommonHelper.GetDirectoryList(path, useFullPath, stringList1);
            if ((sc == 0))
            {
                int i = 0;
                while ((i < stringList1.Count))
                {
                    Interpreter.Vm.CrayonWrapper.addToList(outputList, Interpreter.Vm.CrayonWrapper.buildString(vm.globals, stringList1[i]));
                    i += 1;
                }
            }
            return Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, sc);
        }

        public static Value lib_fileiocommon_directoryMove(VmContext vm, Value[] args)
        {
            int statusCode = FileIOCommonHelper.MoveDirectory((string)args[1].internalValue, (string)args[2].internalValue);
            return Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, statusCode);
        }

        public static Value lib_fileiocommon_fileDelete(VmContext vm, Value[] args)
        {
            int statusCode = FileIOCommonHelper.FileDelete((string)args[1].internalValue);
            return Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, statusCode);
        }

        public static Value lib_fileiocommon_fileInfo(VmContext vm, Value[] args)
        {
            int mask = (int)args[2].internalValue;
            FileIOCommonHelper.GetFileInfo((string)args[1].internalValue, mask, PST_IntBuffer16, PST_FloatBuffer16);
            ListImpl outputList = (ListImpl)args[3].internalValue;
            Interpreter.Vm.CrayonWrapper.clearList(outputList);
            VmGlobals globals = vm.globals;
            Interpreter.Vm.CrayonWrapper.addToList(outputList, Interpreter.Vm.CrayonWrapper.buildBoolean(globals, (PST_IntBuffer16[0] > 0)));
            Interpreter.Vm.CrayonWrapper.addToList(outputList, Interpreter.Vm.CrayonWrapper.buildBoolean(globals, (PST_IntBuffer16[1] > 0)));
            if (((mask & 1) != 0))
            {
                Interpreter.Vm.CrayonWrapper.addToList(outputList, Interpreter.Vm.CrayonWrapper.buildInteger(globals, PST_IntBuffer16[2]));
            }
            else
            {
                Interpreter.Vm.CrayonWrapper.addToList(outputList, globals.valueNull);
            }
            if (((mask & 2) != 0))
            {
                Interpreter.Vm.CrayonWrapper.addToList(outputList, Interpreter.Vm.CrayonWrapper.buildBoolean(globals, (PST_IntBuffer16[3] > 0)));
            }
            else
            {
                Interpreter.Vm.CrayonWrapper.addToList(outputList, globals.valueNull);
            }
            if (((mask & 4) != 0))
            {
                Interpreter.Vm.CrayonWrapper.addToList(outputList, Interpreter.Vm.CrayonWrapper.buildFloat(globals, PST_FloatBuffer16[0]));
            }
            else
            {
                Interpreter.Vm.CrayonWrapper.addToList(outputList, globals.valueNull);
            }
            if (((mask & 8) != 0))
            {
                Interpreter.Vm.CrayonWrapper.addToList(outputList, Interpreter.Vm.CrayonWrapper.buildFloat(globals, PST_FloatBuffer16[1]));
            }
            else
            {
                Interpreter.Vm.CrayonWrapper.addToList(outputList, globals.valueNull);
            }
            return args[3];
        }

        public static Value lib_fileiocommon_fileMove(VmContext vm, Value[] args)
        {
            int statusCode = FileIOCommonHelper.FileMove((string)args[1].internalValue, (string)args[2].internalValue, (bool)args[3].internalValue, (bool)args[4].internalValue);
            return Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, statusCode);
        }

        public static Value lib_fileiocommon_fileRead(VmContext vm, Value[] args)
        {
            Value diskHostObject = args[0];
            string sandboxedPath = (string)args[1].internalValue;
            bool readDataAsBytes = (bool)args[2].internalValue;
            ListImpl outputList = (ListImpl)args[3].internalValue;
            List<Value> tList = new List<Value>();
            int statusCode = FileIOCommonHelper.FileRead(sandboxedPath, readDataAsBytes, PST_StringBuffer16, vm.globals.positiveIntegers, tList);
            if (((statusCode == 0) && !readDataAsBytes))
            {
                Interpreter.Vm.CrayonWrapper.addToList(outputList, Interpreter.Vm.CrayonWrapper.buildString(vm.globals, PST_StringBuffer16[0]));
            }
            else
            {
                Value t = Interpreter.Vm.CrayonWrapper.buildList(tList);
                ListImpl tListImpl = (ListImpl)t.internalValue;
                outputList.array = tListImpl.array;
                outputList.capacity = tListImpl.capacity;
                outputList.size = tList.Count;
            }
            return Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, statusCode);
        }

        public static Value lib_fileiocommon_fileWrite(VmContext vm, Value[] args)
        {
            Value[] ints = vm.globals.positiveIntegers;
            if ((args[3].type != 3))
            {
                return ints[3];
            }
            int statusCode = 0;
            string contentString = null;
            object byteArrayRef = null;
            int format = (int)args[3].internalValue;
            if ((format == 0))
            {
                byteArrayRef = lib_fileiocommon_listToBytes((List<Value>)args[2].internalValue);
                if ((byteArrayRef == null))
                {
                    return ints[6];
                }
            }
            else
            {
                if ((args[2].type != 5))
                {
                    return ints[6];
                }
                else
                {
                    contentString = (string)args[2].internalValue;
                }
            }
            if ((statusCode == 0))
            {
                statusCode = FileIOCommonHelper.FileWrite((string)args[1].internalValue, format, contentString, byteArrayRef);
            }
            return Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, statusCode);
        }

        public static Value lib_fileiocommon_getCurrentDirectory(VmContext vm, Value[] args)
        {
            return Interpreter.Vm.CrayonWrapper.buildString(vm.globals, FileIOCommonHelper.GetCurrentDirectory());
        }

        public static object lib_fileiocommon_getDiskObject(Value diskObjectArg)
        {
            ObjectInstance objInst = (ObjectInstance)diskObjectArg.internalValue;
            return objInst.nativeData[0];
        }

        public static Value lib_fileiocommon_getUserDirectory(VmContext vm, Value[] args)
        {
            return Interpreter.Vm.CrayonWrapper.buildString(vm.globals, FileIOCommonHelper.GetUserDirectory());
        }

        public static Value lib_fileiocommon_initializeDisk(VmContext vm, Value[] args)
        {
            ObjectInstance objInstance1 = (ObjectInstance)args[0].internalValue;
            object[] objArray1 = new object[1];
            objInstance1.nativeData = objArray1;
            object object1 = AlwaysFalse();
            objArray1[0] = object1;
            return vm.globals.valueNull;
        }

        public static Value lib_fileiocommon_isWindows(VmContext vm, Value[] args)
        {
            if (FileIOCommonHelper.IsWindows())
            {
                return vm.globals.boolTrue;
            }
            return vm.globals.boolFalse;
        }

        public static object lib_fileiocommon_listToBytes(List<Value> listOfMaybeInts)
        {
            byte[] bytes = new byte[listOfMaybeInts.Count];
            Value intValue = null;
            int byteValue = 0;
            int i = (listOfMaybeInts.Count - 1);
            while ((i >= 0))
            {
                intValue = listOfMaybeInts[i];
                if ((intValue.type != 3))
                {
                    return null;
                }
                byteValue = (int)intValue.internalValue;
                if ((byteValue >= 256))
                {
                    return null;
                }
                if ((byteValue < 0))
                {
                    if ((byteValue < -128))
                    {
                        return null;
                    }
                    byteValue += 256;
                }
                bytes[i] = (byte)byteValue;
                i -= 1;
            }
            return bytes;
        }

        public static Value lib_fileiocommon_textToLines(VmContext vm, Value[] args)
        {
            lib_fileiocommon_textToLinesImpl(vm.globals, (string)args[0].internalValue, (ListImpl)args[1].internalValue);
            return args[1];
        }

        public static int lib_fileiocommon_textToLinesImpl(VmGlobals globals, string text, ListImpl output)
        {
            List<string> stringList = new List<string>();
            FileIOCommonHelper.TextToLines(text, stringList);
            int _len = stringList.Count;
            int i = 0;
            while ((i < _len))
            {
                Interpreter.Vm.CrayonWrapper.addToList(output, Interpreter.Vm.CrayonWrapper.buildString(globals, stringList[i]));
                i += 1;
            }
            return 0;
        }
    }
}
