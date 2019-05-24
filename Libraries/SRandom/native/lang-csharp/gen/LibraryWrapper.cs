using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Libraries.SRandom
{
    public static class LibraryWrapper
    {
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
