package org.crayonlang.libraries.srandom;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.PlatformTranslationHelper;
import org.crayonlang.interpreter.structs.*;
import org.crayonlang.interpreter.TranslationHelper;

public final class LibraryWrapper {

  public static Value lib_srandom_getBoolean(VmContext vm, Value[] args) {
    ListImpl intPtr = ((ListImpl) args[0].internalValue);
    int value = 0;
    value = (((intPtr.array[0].intValue * 20077) + 12345) & 65535);
    intPtr.array[0] = org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, value);
    if (((value & 1) == 0)) {
      return vm.globalFalse;
    }
    return vm.globalTrue;
  }

  public static Value lib_srandom_getFloat(VmContext vm, Value[] args) {
    ListImpl intPtr = ((ListImpl) args[0].internalValue);
    int value1 = 0;
    value1 = (((intPtr.array[0].intValue * 20077) + 12345) & 65535);
    int value2 = (((value1 * 20077) + 12345) & 65535);
    int value3 = (((value2 * 20077) + 12345) & 65535);
    intPtr.array[0] = org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, value3);
    value1 = ((value1 >> 8) & 255);
    value2 = ((value2 >> 8) & 255);
    value3 = ((value3 >> 8) & 255);
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildFloat(vm.globals, ((value1 << 16) | (value2 << 8) | value3) / 16777216.0);
  }

  public static Value lib_srandom_getInteger(VmContext vm, Value[] args) {
    ListImpl intPtr = ((ListImpl) args[0].internalValue);
    int value1 = 0;
    value1 = (((intPtr.array[0].intValue * 20077) + 12345) & 65535);
    int value2 = (((value1 * 20077) + 12345) & 65535);
    int value3 = (((value2 * 20077) + 12345) & 65535);
    int value4 = (((value3 * 20077) + 12345) & 65535);
    intPtr.array[0] = org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, value4);
    value1 = ((value1 >> 8) & 255);
    value2 = ((value2 >> 8) & 255);
    value3 = ((value3 >> 8) & 255);
    value4 = ((value4 >> 8) & 127);
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, ((value4 << 24) | (value3 << 16) | (value2 << 8) | value1));
  }
}
