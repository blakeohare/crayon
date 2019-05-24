package org.crayonlang.libraries.dispatcher;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.PlatformTranslationHelper;
import org.crayonlang.interpreter.structs.*;
import org.crayonlang.interpreter.TranslationHelper;

public final class LibraryWrapper {

  public static Value lib_dispatcher_flushNativeQueue(VmContext vm, Value[] args) {
    Object[] nd = (((ObjectInstance) args[0].internalValue)).nativeData;
    ArrayList<Value> output = new ArrayList<Value>();
    DispatcherHelper.flushNativeQueue(nd, output);
    if ((output.size() == 0)) {
      return vm.globalNull;
    }
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildList(output);
  }

  public static Value lib_dispatcher_initNativeQueue(VmContext vm, Value[] args) {
    ObjectInstance obj = ((ObjectInstance) args[0].internalValue);
    Object[] nd = new Object[2];
    nd[0] = new Object();
    nd[1] = new ArrayList<Value>();
    obj.nativeData = nd;
    return vm.globalNull;
  }
}
