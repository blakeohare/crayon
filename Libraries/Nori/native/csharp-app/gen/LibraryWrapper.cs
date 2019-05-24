using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Libraries.Nori
{
    public static class LibraryWrapper
    {
        public static Value lib_nori_closeFrame(VmContext vm, Value[] args)
        {
            ObjectInstance frameObj = (ObjectInstance)args[0].internalValue;
            object nativeFrameHandle = frameObj.nativeData[0];
            NoriHelper.CloseFrame(nativeFrameHandle);
            return vm.globalNull;
        }

        public static Value lib_nori_flushUpdatesToFrame(VmContext vm, Value[] args)
        {
            ObjectInstance frameObj = (ObjectInstance)args[0].internalValue;
            object nativeFrameHandle = frameObj.nativeData[0];
            string data = (string)args[1].internalValue;
            NoriHelper.FlushUpdatesToFrame(nativeFrameHandle, data);
            return vm.globalNull;
        }

        public static Value lib_nori_prepImageResource(VmContext vm, Value[] args)
        {
            ObjectInstance frameWrapped = (ObjectInstance)args[0].internalValue;
            object frame = frameWrapped.nativeData[0];
            ObjectInstance obj = (ObjectInstance)args[2].internalValue;
            object nativeImageData = obj.nativeData[0];
            int id = (int)args[1].internalValue;
            int x = (int)args[3].internalValue;
            int y = (int)args[4].internalValue;
            int width = (int)args[5].internalValue;
            int height = (int)args[6].internalValue;
            NoriHelper.SendImageToRenderer(frame, id, nativeImageData, x, y, width, height);
            return vm.globalNull;
        }

        public static Value lib_nori_runEventWatcher(VmContext vm, Value[] args)
        {
            ObjectInstance frameObj = (ObjectInstance)args[0].internalValue;
            int execContextIdForResume = (int)args[1].internalValue;
            Value eventCallback = args[2];
            Value postShowCallback = args[3];
            ExecutionContext ec = Interpreter.Vm.CrayonWrapper.getExecutionContext(vm, execContextIdForResume);
            Interpreter.Vm.CrayonWrapper.vm_suspend_for_context(ec, 1);
            NoriHelper.EventWatcher(vm, execContextIdForResume, args[0], eventCallback, postShowCallback);
            return vm.globalNull;
        }

        public static Value lib_nori_showFrame(VmContext vm, Value[] args)
        {
            ObjectInstance frameObj = (ObjectInstance)args[0].internalValue;
            string title = (string)args[1].internalValue;
            int width = (int)args[2].internalValue;
            int height = (int)args[3].internalValue;
            string data = (string)args[4].internalValue;
            int execId = (int)args[5].internalValue;
            frameObj.nativeData = new object[1];
            frameObj.nativeData[0] = NoriHelper.ShowFrame(args[0], title, width, height, data, execId);
            return vm.globalNull;
        }
    }
}
