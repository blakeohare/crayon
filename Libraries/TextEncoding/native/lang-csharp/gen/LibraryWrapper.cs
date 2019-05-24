using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Libraries.TextEncoding
{
    public static class LibraryWrapper
    {
        private static readonly string[] PST_StringBuffer16 = new string[16];

        private static readonly int[] PST_IntBuffer16 = new int[16];

        public static Value lib_textencoding_convertBytesToText(VmContext vm, Value[] args)
        {
            if ((args[0].type != 6))
            {
                return Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, 2);
            }
            ListImpl byteList = (ListImpl)args[0].internalValue;
            int format = (int)args[1].internalValue;
            ListImpl output = (ListImpl)args[2].internalValue;
            string[] strOut = PST_StringBuffer16;
            int length = byteList.size;
            int[] unwrappedBytes = new int[length];
            int i = 0;
            Value value = null;
            int c = 0;
            while ((i < length))
            {
                value = byteList.array[i];
                if ((value.type != 3))
                {
                    return Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, 3);
                }
                c = (int)value.internalValue;
                if (((c < 0) || (c > 255)))
                {
                    return Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, 3);
                }
                unwrappedBytes[i] = c;
                i += 1;
            }
            int sc = TextEncodingHelper.BytesToText(unwrappedBytes, format, strOut);
            if ((sc == 0))
            {
                Interpreter.Vm.CrayonWrapper.addToList(output, Interpreter.Vm.CrayonWrapper.buildString(vm.globals, strOut[0]));
            }
            return Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, sc);
        }

        public static Value lib_textencoding_convertTextToBytes(VmContext vm, Value[] args)
        {
            string value = (string)args[0].internalValue;
            int format = (int)args[1].internalValue;
            bool includeBom = (bool)args[2].internalValue;
            ListImpl output = (ListImpl)args[3].internalValue;
            List<Value> byteList = new List<Value>();
            int[] intOut = PST_IntBuffer16;
            int sc = TextEncodingHelper.TextToBytes(value, includeBom, format, byteList, vm.globals.positiveIntegers, intOut);
            int swapWordSize = intOut[0];
            if ((swapWordSize != 0))
            {
                int i = 0;
                int j = 0;
                int length = byteList.Count;
                Value swap = null;
                int half = (swapWordSize >> 1);
                int k = 0;
                while ((i < length))
                {
                    k = (i + swapWordSize - 1);
                    j = 0;
                    while ((j < half))
                    {
                        swap = byteList[(i + j)];
                        byteList[(i + j)] = byteList[(k - j)];
                        byteList[(k - j)] = swap;
                        j += 1;
                    }
                    i += swapWordSize;
                }
            }
            if ((sc == 0))
            {
                Interpreter.Vm.CrayonWrapper.addToList(output, Interpreter.Vm.CrayonWrapper.buildList(byteList));
            }
            return Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, sc);
        }
    }
}
