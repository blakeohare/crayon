package org.crayonlang.libraries.textencoding;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.PlatformTranslationHelper;
import org.crayonlang.interpreter.structs.*;
import org.crayonlang.interpreter.TranslationHelper;

public final class LibraryWrapper {

  private static final String[] PST_stringBuffer16 = new String[16];

  private static final int[] PST_intBuffer16 = new int[16];

  public static Value lib_textencoding_convertBytesToText(VmContext vm, Value[] args) {
    if ((args[0].type != 6)) {
      return org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, 2);
    }
    ListImpl byteList = ((ListImpl) args[0].internalValue);
    int format = ((int) args[1].internalValue);
    ListImpl output = ((ListImpl) args[2].internalValue);
    String[] strOut = PST_stringBuffer16;
    int length = byteList.size;
    int[] unwrappedBytes = new int[length];
    int i = 0;
    Value value = null;
    int c = 0;
    while ((i < length)) {
      value = byteList.array[i];
      if ((value.type != 3)) {
        return org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, 3);
      }
      c = ((int) value.internalValue);
      if (((c < 0) || (c > 255))) {
        return org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, 3);
      }
      unwrappedBytes[i] = c;
      i += 1;
    }
    int sc = crayonlib.textencoding.TextEncodingHelper.bytesToText(unwrappedBytes, format, strOut);
    if ((sc == 0)) {
      org.crayonlang.interpreter.vm.CrayonWrapper.addToList(output, org.crayonlang.interpreter.vm.CrayonWrapper.buildString(vm.globals, strOut[0]));
    }
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, sc);
  }

  public static Value lib_textencoding_convertTextToBytes(VmContext vm, Value[] args) {
    String value = ((String) args[0].internalValue);
    int format = ((int) args[1].internalValue);
    boolean includeBom = ((boolean) args[2].internalValue);
    ListImpl output = ((ListImpl) args[3].internalValue);
    ArrayList<Value> byteList = new ArrayList<Value>();
    int[] intOut = PST_intBuffer16;
    int sc = crayonlib.textencoding.TextEncodingHelper.textToBytes(value, includeBom, format, byteList, vm.globals.positiveIntegers, intOut);
    int swapWordSize = intOut[0];
    if ((swapWordSize != 0)) {
      int i = 0;
      int j = 0;
      int length = byteList.size();
      Value swap = null;
      int half = (swapWordSize >> 1);
      int k = 0;
      while ((i < length)) {
        k = (i + swapWordSize - 1);
        j = 0;
        while ((j < half)) {
          swap = byteList.get((i + j));
          byteList.set((i + j), byteList.get((k - j)));
          byteList.set((k - j), swap);
          j += 1;
        }
        i += swapWordSize;
      }
    }
    if ((sc == 0)) {
      org.crayonlang.interpreter.vm.CrayonWrapper.addToList(output, org.crayonlang.interpreter.vm.CrayonWrapper.buildList(byteList));
    }
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, sc);
  }
}
