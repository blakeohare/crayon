using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Libraries.UserData
{
    public static class LibraryWrapper
    {
        public static Value lib_userdata_getProjectSandboxDirectory(VmContext vm, Value[] args)
        {
            Value output = vm.globalNull;
            Value arg1 = args[0];
            return output;
        }
    }
}
