using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Libraries.FileIOCommon
{
    public static class LibraryWrapper
    {
        public static bool AlwaysFalse() { return false; }

        private static readonly string[] PST_StringBuffer16 = new string[16];

        private static readonly int[] PST_IntBuffer16 = new int[16];

        private static readonly double[] PST_FloatBuffer16 = new double[16];

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
                byteArrayRef = lib_fileiocommon_listToBytes((ListImpl)args[2].internalValue);
                if ((byteArrayRef == null))
                {
                    return ints[6];
                }
            }
            else if ((args[2].type != 5))
            {
                return ints[6];
            }
            else
            {
                contentString = (string)args[2].internalValue;
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

        public static object lib_fileiocommon_listToBytes(ListImpl listOfMaybeInts)
        {
            byte[] bytes = new byte[listOfMaybeInts.size];
            Value intValue = null;
            int byteValue = 0;
            int i = (listOfMaybeInts.size - 1);
            while ((i >= 0))
            {
                intValue = listOfMaybeInts.array[i];
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
