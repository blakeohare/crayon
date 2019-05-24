package org.crayonlang.libraries.imageencoder;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.PlatformTranslationHelper;
import org.crayonlang.interpreter.structs.*;
import org.crayonlang.interpreter.TranslationHelper;

public final class LibraryWrapper {

  public static Value lib_imageencoder_encodeToBytes(VmContext vm, Value[] args) {
    Object platformBitmap = org.crayonlang.interpreter.vm.CrayonWrapper.getNativeDataItem(args[0], 0);
    int imageFormat = ((int) args[1].internalValue);
    ArrayList<Value> byteOutputList = new ArrayList<Value>();
    int statusCode = ImageEncoderUtil.encode(platformBitmap, imageFormat, byteOutputList, vm.globals.positiveIntegers);
    int length = byteOutputList.size();
    ListImpl finalOutputList = ((ListImpl) args[2].internalValue);
    int i = 0;
    while ((i < length)) {
      org.crayonlang.interpreter.vm.CrayonWrapper.addToList(finalOutputList, byteOutputList.get(i));
      i += 1;
    }
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, statusCode);
  }
}
