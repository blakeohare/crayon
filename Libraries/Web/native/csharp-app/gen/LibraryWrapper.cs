using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Libraries.Web
{
    public static class LibraryWrapper
    {
        public static Value lib_web_launch_browser(VmContext vm, Value[] args)
        {
            string url = (string)args[0].internalValue;
            System.Diagnostics.Process.Start(url);
            return vm.globalNull;
        }
    }
}
