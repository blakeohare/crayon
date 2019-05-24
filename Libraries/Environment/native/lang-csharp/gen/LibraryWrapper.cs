using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Libraries.Environment
{
    public static class LibraryWrapper
    {
        public static Value lib_environment_get(VmContext vm, Value[] args)
        {
            string value = System.Environment.GetEnvironmentVariable((string)args[0].internalValue);
            if ((value == null))
            {
                return vm.globalNull;
            }
            return Interpreter.Vm.CrayonWrapper.buildString(vm.globals, value);
        }
    }
}
