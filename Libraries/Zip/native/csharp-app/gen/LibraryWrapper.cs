using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Libraries.Zip
{
    public static class LibraryWrapper
    {
        public static bool AlwaysFalse() { return false; }

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

        public static void lib_zip_initAsyncCallback(ListImpl scOut, object[] nativeData, object nativeZipArchive, VmContext vm, int execContext)
        {
            int sc = 0;
            if ((nativeZipArchive == null))
            {
                sc = 2;
            }
            Interpreter.Vm.CrayonWrapper.setItemInList(scOut, 0, Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, sc));
            nativeData[0] = nativeZipArchive;
            Interpreter.Vm.CrayonWrapper.runInterpreter(vm, execContext);
        }

        public static Value lib_zip_initializeZipReader(VmContext vm, Value[] args)
        {
            int sc = 0;
            ListImpl scOut = (ListImpl)args[2].internalValue;
            int execId = (int)args[3].internalValue;
            int[] byteArray = lib_zip_validateByteList(args[1], true);
            if ((byteArray == null))
            {
                sc = 1;
            }
            else
            {
                ObjectInstance obj = (ObjectInstance)args[0].internalValue;
                obj.nativeData = new object[2];
                obj.nativeData[0] = ZipHelper.CreateZipReader(byteArray);
                obj.nativeData[1] = 0;
                if ((obj.nativeData[0] == null))
                {
                    sc = 2;
                }
                else
                {
                    sc = 0;
                }
                if (AlwaysFalse())
                {
                    sc = 3;
                    Interpreter.Vm.CrayonWrapper.vm_suspend_context_by_id(vm, execId, 1);
                }
            }
            Interpreter.Vm.CrayonWrapper.setItemInList(scOut, 0, Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, sc));
            return vm.globalNull;
        }

        public static Value lib_zip_readerPeekNextEntry(VmContext vm, Value[] args)
        {
            ObjectInstance obj = (ObjectInstance)args[0].internalValue;
            object[] nd = obj.nativeData;
            ListImpl output = (ListImpl)args[1].internalValue;
            int execId = (int)args[2].internalValue;
            bool[] boolOut = new bool[3];
            string[] nameOut = new string[1];
            List<int> integers = new List<int>();
            ZipHelper.ReadNextZipEntry(nd[0], (int)nd[1], boolOut, nameOut, integers);
            if (AlwaysFalse())
            {
                Interpreter.Vm.CrayonWrapper.vm_suspend_context_by_id(vm, execId, 1);
                return vm.globalTrue;
            }
            return lib_zip_readerPeekNextEntryCallback(!boolOut[0], boolOut[1], boolOut[2], nameOut[0], integers, nd, output, vm);
        }

        public static Value lib_zip_readerPeekNextEntryCallback(bool problemsEncountered, bool foundAnything, bool isDirectory, string name, List<int> bytesAsIntList, object[] nativeData, ListImpl output, VmContext vm)
        {
            if (problemsEncountered)
            {
                return vm.globalFalse;
            }
            nativeData[1] = (1 + (int)nativeData[1]);
            Interpreter.Vm.CrayonWrapper.setItemInList(output, 0, Interpreter.Vm.CrayonWrapper.buildBoolean(vm.globals, foundAnything));
            if (!foundAnything)
            {
                return vm.globalTrue;
            }
            Interpreter.Vm.CrayonWrapper.setItemInList(output, 1, Interpreter.Vm.CrayonWrapper.buildString(vm.globals, name));
            if (isDirectory)
            {
                Interpreter.Vm.CrayonWrapper.setItemInList(output, 2, Interpreter.Vm.CrayonWrapper.buildBoolean(vm.globals, isDirectory));
                return vm.globalTrue;
            }
            ListImpl byteValues = (ListImpl)Interpreter.Vm.CrayonWrapper.getItemFromList(output, 3).internalValue;
            int length = bytesAsIntList.Count;
            int i = 0;
            Value[] positiveNumbers = vm.globals.positiveIntegers;
            Value[] valuesOut = new Value[length];
            i = 0;
            while ((i < length))
            {
                valuesOut[i] = positiveNumbers[bytesAsIntList[i]];
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
