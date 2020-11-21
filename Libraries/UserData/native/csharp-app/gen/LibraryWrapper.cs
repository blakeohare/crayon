using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Libraries.UserData
{
    public static class LibraryWrapper
    {
        private static Dictionary<string, System.Func<object[], object>> PST_ExtCallbacks = new Dictionary<string, System.Func<object[], object>>();

        public static void PST_RegisterExtensibleCallback(string name, System.Func<object[], object> func)
        {
            PST_ExtCallbacks[name] = func;
        }

        public static Value lib_userdata_getProjectSandboxDirectory(VmContext vm, Value[] args)
        {
            Value output = vm.globalNull;
            Value arg1 = args[0];
            return output;
        }
    }
}
