using Interpreter.Structs;
using Interpreter.Vm;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Libraries.ImageResources
{
    public static class LibraryWrapper
    {
        public static Value lib_imageresources_blit(VmContext vm, Value[] args)
        {
            object object1 = null;
            ObjectInstance objInstance1 = (ObjectInstance)args[0].internalValue;
            ObjectInstance objInstance2 = (ObjectInstance)args[1].internalValue;
            object[] objArray1 = objInstance1.nativeData;
            object1 = objArray1[3];
            if ((object1 == null))
            {
                object1 = ImageResourceHelper.GetPixelEditSession(objArray1[0]);
                objArray1[3] = object1;
            }
            ImageResourceHelper.BlitImage(objInstance1.nativeData[0], objInstance2.nativeData[0], (int)args[2].internalValue, (int)args[3].internalValue, (int)args[4].internalValue, (int)args[5].internalValue, (int)args[6].internalValue, (int)args[7].internalValue, object1);
            return vm.globalNull;
        }

        public static Value lib_imageresources_checkLoaderIsDone(VmContext vm, Value[] args)
        {
            ObjectInstance objInstance1 = (ObjectInstance)args[0].internalValue;
            ObjectInstance objInstance2 = (ObjectInstance)args[1].internalValue;
            int status = ImageResourceHelper.CheckLoaderIsDone(objInstance1.nativeData, objInstance2.nativeData);
            return Interpreter.Vm.CrayonWrapper.buildInteger(vm.globals, status);
        }

        public static Value lib_imageresources_flushImageChanges(VmContext vm, Value[] args)
        {
            ObjectInstance objInstance1 = (ObjectInstance)args[0].internalValue;
            object[] objArray1 = objInstance1.nativeData;
            if ((objArray1 != null))
            {
                object object1 = objArray1[3];
                if ((object1 != null))
                {
                    ImageResourceHelper.FlushPixelEditSession(object1);
                    objArray1[3] = null;
                }
            }
            return vm.globalNull;
        }

        public static Value lib_imageresources_getManifestString(VmContext vm, Value[] args)
        {
            return Interpreter.Vm.CrayonWrapper.buildString(vm.globals, TranslationHelper.ImageSheetManifest);
        }

        public static Value lib_imageresources_loadAsynchronous(VmContext vm, Value[] args)
        {
            ObjectInstance objInstance1 = (ObjectInstance)args[0].internalValue;
            string filename = (string)args[1].internalValue;
            ObjectInstance objInstance2 = (ObjectInstance)args[2].internalValue;
            object[] objArray1 = new object[3];
            objInstance1.nativeData = objArray1;
            object[] objArray2 = new object[4];
            objArray2[2] = 0;
            objInstance2.nativeData = objArray2;
            ImageResourceHelper.ImageLoadAsync(filename, objArray1, objArray2);
            return vm.globalNull;
        }

        public static Value lib_imageresources_nativeImageDataInit(VmContext vm, Value[] args)
        {
            ObjectInstance objInstance1 = (ObjectInstance)args[0].internalValue;
            object[] nd = new object[4];
            int width = (int)args[1].internalValue;
            int height = (int)args[2].internalValue;
            nd[0] = ImageResourceHelper.GenerateNativeBitmapOfSize(width, height);
            nd[1] = width;
            nd[2] = height;
            nd[3] = null;
            objInstance1.nativeData = nd;
            return vm.globalNull;
        }
    }
}
