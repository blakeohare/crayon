using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Libraries.Json
{
    public static class LibraryWrapper
    {
        private static Dictionary<string, System.Func<object[], object>> PST_ExtCallbacks = new Dictionary<string, System.Func<object[], object>>();

        public static void PST_RegisterExtensibleCallback(string name, System.Func<object[], object> func)
        {
            PST_ExtCallbacks[name] = func;
        }

        public static Value lib_json_parse(VmContext vm, Value[] args)
        {
            string raw = (string)args[0].internalValue;
            if ((raw.Length > 0))
            {
                Value output = JsonParser.ParseJsonIntoValue(vm.globals, raw);
                if ((output != null))
                {
                    return output;
                }
            }
            return vm.globalNull;
        }
    }
}
