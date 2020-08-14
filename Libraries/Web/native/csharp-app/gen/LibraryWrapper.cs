using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Libraries.Web
{
    public static class LibraryWrapper
    {
        private static Dictionary<string, System.Func<object[], object>> PST_ExtCallbacks = new Dictionary<string, System.Func<object[], object>>();

        public static void PST_RegisterExtensibleCallback(string name, System.Func<object[], object> func) {
            PST_ExtCallbacks[name] = func;
        }

        public static Value lib_web_launch_browser(VmContext vm, Value[] args)
        {
            string url = (string)args[0].internalValue;
            System.Diagnostics.Process.Start(url);
            return vm.globalNull;
        }
    }
}
