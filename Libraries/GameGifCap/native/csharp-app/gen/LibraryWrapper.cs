using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Libraries.GameGifCap
{
    public static class LibraryWrapper
    {
        public static Value lib_gamegifcap_createGifContext(VmContext vm, Value[] args)
        {
            int ms = (int)args[1].internalValue;
            ObjectInstance oi = (ObjectInstance)args[0].internalValue;
            oi.nativeData[0] = new Libraries.GameGifCap.GifRecorderContext(ms);
            return vm.globalNull;
        }

        public static Value lib_gamegifcap_isSupported(VmContext vm, Value[] args)
        {
            if (Libraries.GameGifCap.GameGifCapHelper.IsSupported())
            {
                return vm.globalTrue;
            }
            return vm.globalFalse;
        }

        public static Value lib_gamegifcap_saveToDisk(VmContext vm, Value[] args)
        {
            ObjectInstance oi = (ObjectInstance)args[0].internalValue;
            object ctx = oi.nativeData[0];
            string path = (string)args[1].internalValue;
            int sc = Libraries.GameGifCap.GameGifCapHelper.SaveToDisk(ctx, path);
            return Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, sc);
        }

        public static Value lib_gamegifcap_screenCap(VmContext vm, Value[] args)
        {
            ObjectInstance oiCtx = (ObjectInstance)args[0].internalValue;
            ObjectInstance oiGw = (ObjectInstance)args[1].internalValue;
            int sc = Libraries.GameGifCap.GameGifCapHelper.ScreenCap(oiCtx.nativeData[0], oiGw.nativeData[0]);
            return Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, sc);
        }

        public static Value lib_gamegifcap_setRecordSize(VmContext vm, Value[] args)
        {
            ObjectInstance oi = (ObjectInstance)args[0].internalValue;
            int w = (int)args[1].internalValue;
            int h = (int)args[2].internalValue;
            Libraries.GameGifCap.GameGifCapHelper.SetRecordSize(oi.nativeData[0], w, h);
            return vm.globalNull;
        }
    }
}
