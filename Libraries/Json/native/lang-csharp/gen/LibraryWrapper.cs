using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Libraries.Json
{
    public static class LibraryWrapper
    {
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
