using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Libraries.ImageWebResources
{
    public static class LibraryWrapper
    {
        public static Value lib_imagewebresources_bytesToImage(VmContext vm, Value[] args)
        {
            ObjectInstance objInstance1 = (ObjectInstance)args[0].internalValue;
            object object1 = objInstance1.nativeData[0];
            ListImpl list1 = (ListImpl)args[1].internalValue;
            Value value = Interpreter.Vm.CrayonWrapper.getItemFromList(list1, 0);
            object[] objArray1 = new object[3];
            objInstance1 = (ObjectInstance)value.internalValue;
            objInstance1.nativeData = objArray1;
            if (ImageDownloader.BytesToImage(object1, objArray1))
            {
                Value width = Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, (int)objArray1[1]);
                Value height = Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, (int)objArray1[2]);
                list1.array[1] = width;
                list1.array[2] = height;
                return vm.globalTrue;
            }
            return vm.globalFalse;
        }

        public static Value lib_imagewebresources_jsDownload(VmContext vm, Value[] args)
        {
            return vm.globalNull;
        }

        public static Value lib_imagewebresources_jsGetImage(VmContext vm, Value[] args)
        {
            return vm.globalFalse;
        }

        public static Value lib_imagewebresources_jsPoll(VmContext vm, Value[] args)
        {
            return vm.globalFalse;
        }
    }
}
