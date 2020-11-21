using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Libraries.Dispatcher
{
    public static class LibraryWrapper
    {
        private static Dictionary<string, System.Func<object[], object>> PST_ExtCallbacks = new Dictionary<string, System.Func<object[], object>>();

        public static void PST_RegisterExtensibleCallback(string name, System.Func<object[], object> func)
        {
            PST_ExtCallbacks[name] = func;
        }

        public static Value lib_dispatcher_flushNativeQueue(VmContext vm, Value[] args)
        {
            object[] nd = ((ObjectInstance)args[0].internalValue).nativeData;
            List<Value> output = new List<Value>();
            DispatcherHelper.FlushNativeQueue(nd, output);
            if ((output.Count == 0))
            {
                return vm.globalNull;
            }
            return Interpreter.Vm.CrayonWrapper.buildList(output);
        }

        public static Value lib_dispatcher_initNativeQueue(VmContext vm, Value[] args)
        {
            ObjectInstance obj = (ObjectInstance)args[0].internalValue;
            object[] nd = new object[2];
            nd[0] = new object();
            nd[1] = new List<Value>();
            obj.nativeData = nd;
            return vm.globalNull;
        }
    }
}
