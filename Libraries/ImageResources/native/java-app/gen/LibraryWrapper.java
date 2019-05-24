package org.crayonlang.libraries.imageresources;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.PlatformTranslationHelper;
import org.crayonlang.interpreter.structs.*;
import org.crayonlang.interpreter.TranslationHelper;

public final class LibraryWrapper {

  public static Value lib_imageresources_blit(VmContext vm, Value[] args) {
    Object object1 = null;
    ObjectInstance objInstance1 = ((ObjectInstance) args[0].internalValue);
    ObjectInstance objInstance2 = ((ObjectInstance) args[1].internalValue);
    ImageResourcesHelper.blitImage(objInstance1.nativeData[0], objInstance2.nativeData[0], ((int) args[2].internalValue), ((int) args[3].internalValue), ((int) args[4].internalValue), ((int) args[5].internalValue), ((int) args[6].internalValue), ((int) args[7].internalValue));
    return vm.globalNull;
  }

  public static Value lib_imageresources_checkLoaderIsDone(VmContext vm, Value[] args) {
    ObjectInstance objInstance1 = ((ObjectInstance) args[0].internalValue);
    ObjectInstance objInstance2 = ((ObjectInstance) args[1].internalValue);
    int status = ImageResourcesHelper.checkLoaderIsDone(objInstance1.nativeData, objInstance2.nativeData);
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, status);
  }

  public static Value lib_imageresources_flushImageChanges(VmContext vm, Value[] args) {
    return vm.globalNull;
  }

  public static Value lib_imageresources_getManifestString(VmContext vm, Value[] args) {
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildString(vm.globals, ImageResourcesHelper.getImageResourceManifestString());
  }

  public static Value lib_imageresources_loadAsynchronous(VmContext vm, Value[] args) {
    ObjectInstance objInstance1 = ((ObjectInstance) args[0].internalValue);
    String filename = ((String) args[1].internalValue);
    ObjectInstance objInstance2 = ((ObjectInstance) args[2].internalValue);
    Object[] objArray1 = new Object[3];
    objInstance1.nativeData = objArray1;
    Object[] objArray2 = new Object[4];
    objArray2[2] = 0;
    objInstance2.nativeData = objArray2;
    ImageResourcesHelper.imageLoadAsync(filename, objArray1, objArray2);
    return vm.globalNull;
  }

  public static Value lib_imageresources_nativeImageDataInit(VmContext vm, Value[] args) {
    ObjectInstance objInstance1 = ((ObjectInstance) args[0].internalValue);
    Object[] nd = new Object[4];
    int width = ((int) args[1].internalValue);
    int height = ((int) args[2].internalValue);
    nd[0] = ImageResourcesHelper.generateNativeBitmapOfSize(width, height);
    nd[1] = width;
    nd[2] = height;
    nd[3] = null;
    objInstance1.nativeData = nd;
    return vm.globalNull;
  }
}
