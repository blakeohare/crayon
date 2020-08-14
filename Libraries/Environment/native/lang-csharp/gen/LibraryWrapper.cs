using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Libraries.Environment
{
    public static class LibraryWrapper
    {
        private static Dictionary<string, System.Func<object[], object>> PST_ExtCallbacks = new Dictionary<string, System.Func<object[], object>>();

        public static void PST_RegisterExtensibleCallback(string name, System.Func<object[], object> func) {
            PST_ExtCallbacks[name] = func;
        }

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
