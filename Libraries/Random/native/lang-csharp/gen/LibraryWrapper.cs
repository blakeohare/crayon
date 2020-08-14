using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Libraries.Random
{
    public static class LibraryWrapper
    {
        private static readonly System.Random PST_Random = new System.Random();

        private static Dictionary<string, System.Func<object[], object>> PST_ExtCallbacks = new Dictionary<string, System.Func<object[], object>>();

        public static void PST_RegisterExtensibleCallback(string name, System.Func<object[], object> func) {
            PST_ExtCallbacks[name] = func;
        }

        public static Value lib_random_random_bool(VmContext vm, Value[] args)
        {
            if ((PST_Random.NextDouble() < 0.5))
            {
                return vm.globalTrue;
            }
            return vm.globalFalse;
        }

        public static Value lib_random_random_float(VmContext vm, Value[] args)
        {
            return new Value(4, PST_Random.NextDouble());
        }

        public static Value lib_random_random_int(VmContext vm, Value[] args)
        {
            if (((args[0].type != 3) || (args[1].type != 3)))
            {
                return vm.globalNull;
            }
            int lower = (int)args[0].internalValue;
            int upper = (int)args[1].internalValue;
            if ((lower >= upper))
            {
                return vm.globalNull;
            }
            int value = (int)((PST_Random.NextDouble() * (upper - lower)));
            return Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, (lower + value));
        }
    }
}
