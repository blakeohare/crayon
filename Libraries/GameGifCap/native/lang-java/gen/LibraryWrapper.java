package org.crayonlang.libraries.gamegifcap;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.PlatformTranslationHelper;
import org.crayonlang.interpreter.structs.*;
import org.crayonlang.interpreter.TranslationHelper;

public final class LibraryWrapper {

  public static Value lib_gamegifcap_createGifContext(VmContext vm, Value[] args) {
    int ms = ((int) args[1].internalValue);
    ObjectInstance oi = ((ObjectInstance) args[0].internalValue);
    oi.nativeData[0] = new Object();
    return vm.globalNull;
  }

  public static Value lib_gamegifcap_isSupported(VmContext vm, Value[] args) {
    if (TranslationHelper.alwaysFalse()) {
      return vm.globalTrue;
    }
    return vm.globalFalse;
  }

  public static Value lib_gamegifcap_saveToDisk(VmContext vm, Value[] args) {
    ObjectInstance oi = ((ObjectInstance) args[0].internalValue);
    Object ctx = oi.nativeData[0];
    String path = ((String) args[1].internalValue);
    int sc = 0;
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, sc);
  }

  public static Value lib_gamegifcap_screenCap(VmContext vm, Value[] args) {
    ObjectInstance oiCtx = ((ObjectInstance) args[0].internalValue);
    ObjectInstance oiGw = ((ObjectInstance) args[1].internalValue);
    int sc = 0;
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, sc);
  }

  public static Value lib_gamegifcap_setRecordSize(VmContext vm, Value[] args) {
    ObjectInstance oi = ((ObjectInstance) args[0].internalValue);
    int w = ((int) args[1].internalValue);
    int h = ((int) args[2].internalValue);
    TranslationHelper.alwaysFalse();
    return vm.globalNull;
  }
}
