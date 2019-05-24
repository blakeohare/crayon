using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Libraries.ProcessUtil
{
    public static class LibraryWrapper
    {
        public static bool AlwaysTrue() { return true; }

        public static Value lib_processutil_isSupported(VmContext vm, Value[] args)
        {
            bool t = AlwaysTrue();
            return Interpreter.Vm.CrayonWrapper.buildBoolean(vm.globals, t);
        }

        public static Value lib_processutil_launchProcess(VmContext vm, Value[] args)
        {
            ObjectInstance bridge = (ObjectInstance)args[0].internalValue;
            bridge.nativeData = new object[5];
            bridge.nativeData[0] = true;
            bridge.nativeData[1] = 0;
            bridge.nativeData[2] = new List<string>();
            bridge.nativeData[3] = new List<string>();
            bridge.nativeData[4] = new object();
            string execName = (string)args[1].internalValue;
            ListImpl argsRaw = (ListImpl)args[2].internalValue;
            bool isAsync = (bool)args[3].internalValue;
            Value cb = args[4];
            ObjectInstance dispatcherQueue = (ObjectInstance)args[5].internalValue;
            List<string> argStrings = new List<string>();
            int i = 0;
            while ((i < argsRaw.size))
            {
                Value a = Interpreter.Vm.CrayonWrapper.getItemFromList(argsRaw, i);
                argStrings.Add((string)a.internalValue);
                i += 1;
            }
            ProcessUtilHelper.LaunchProcessImpl(bridge.nativeData, execName, argStrings, isAsync, cb, vm);
            return vm.globalNull;
        }

        public static Value lib_processutil_readBridge(VmContext vm, Value[] args)
        {
            ObjectInstance bridge = (ObjectInstance)args[0].internalValue;
            ListImpl outputList = (ListImpl)args[1].internalValue;
            int type = (int)args[2].internalValue;
            object mtx = bridge.nativeData[4];
            if ((type == 1))
            {
                int outputInt = ProcessUtilHelper.ReadBridgeInt(mtx, bridge.nativeData, type);
                Interpreter.Vm.CrayonWrapper.addToList(outputList, Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, outputInt));
            }
            else
            {
                List<string> output = new List<string>();
                ProcessUtilHelper.ReadBridgeStrings(mtx, bridge.nativeData, type, output);
                int i = 0;
                while ((i < output.Count))
                {
                    Interpreter.Vm.CrayonWrapper.addToList(outputList, Interpreter.Vm.CrayonWrapper.buildString(vm.globals, output[i]));
                    i += 1;
                }
            }
            return vm.globalNull;
        }
    }
}
