using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Libraries.Resources
{
    public static class LibraryWrapper
    {
        private static Dictionary<string, System.Func<object[], object>> PST_ExtCallbacks = new Dictionary<string, System.Func<object[], object>>();

        public static void PST_RegisterExtensibleCallback(string name, System.Func<object[], object> func) {
            PST_ExtCallbacks[name] = func;
        }

        public static Value lib_resources_getResourceData(VmContext vm, Value[] args)
        {
            Value output = Interpreter.Vm.CrayonWrapper.buildList(vm.resourceDatabase.dataList);
            vm.resourceDatabase.dataList = null;
            return output;
        }

        public static Value lib_resources_readText(VmContext vm, Value[] args)
        {
            string string1 = Interpreter.ResourceReader.ReadTextResource((string)args[0].internalValue);
            return Interpreter.Vm.CrayonWrapper.buildString(vm.globals, string1);
        }
    }
}
