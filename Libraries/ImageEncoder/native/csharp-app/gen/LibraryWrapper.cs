using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Libraries.ImageEncoder
{
    public static class LibraryWrapper
    {
        private static Dictionary<string, System.Func<object[], object>> PST_ExtCallbacks = new Dictionary<string, System.Func<object[], object>>();

        public static void PST_RegisterExtensibleCallback(string name, System.Func<object[], object> func) {
            PST_ExtCallbacks[name] = func;
        }

        public static Value lib_imageencoder_encodeToBytes(VmContext vm, Value[] args)
        {
            object platformBitmap = Interpreter.Vm.CrayonWrapper.getNativeDataItem(args[0], 0);
            int imageFormat = (int)args[1].internalValue;
            List<Value> byteOutputList = new List<Value>();
            int statusCode = ImageEncoderUtil.Encode(platformBitmap, imageFormat, byteOutputList, vm.globals.positiveIntegers);
            int length = byteOutputList.Count;
            ListImpl finalOutputList = (ListImpl)args[2].internalValue;
            int i = 0;
            while ((i < length))
            {
                Interpreter.Vm.CrayonWrapper.addToList(finalOutputList, byteOutputList[i]);
                i += 1;
            }
            return Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, statusCode);
        }
    }
}
