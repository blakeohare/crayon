using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Libraries.CryptoCommon
{
    public static class LibraryWrapper
    {
        private static Dictionary<string, System.Func<object[], object>> PST_ExtCallbacks = new Dictionary<string, System.Func<object[], object>>();

        public static void PST_RegisterExtensibleCallback(string name, System.Func<object[], object> func) {
            PST_ExtCallbacks[name] = func;
        }

        public static Value lib_cryptocommon_addBytes(VmContext vm, Value[] args)
        {
            ObjectInstance obj = (ObjectInstance)args[0].internalValue;
            ListImpl fromByteList = (ListImpl)args[1].internalValue;
            List<int> toByteList = (List<int>)obj.nativeData[0];
            int length = fromByteList.size;
            int i = 0;
            while ((i < length))
            {
                toByteList.Add((int)fromByteList.array[i].internalValue);
                i += 1;
            }
            return vm.globalFalse;
        }

        public static Value lib_cryptocommon_initHash(VmContext vm, Value[] args)
        {
            ObjectInstance obj = (ObjectInstance)args[0].internalValue;
            obj.nativeData = new object[1];
            obj.nativeData[0] = new List<int>();
            return vm.globalNull;
        }
    }
}
