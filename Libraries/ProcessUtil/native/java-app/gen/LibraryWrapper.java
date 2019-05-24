package org.crayonlang.libraries.processutil;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.PlatformTranslationHelper;
import org.crayonlang.interpreter.structs.*;
import org.crayonlang.interpreter.TranslationHelper;

public final class LibraryWrapper {

  public static Value lib_processutil_isSupported(VmContext vm, Value[] args) {
    boolean t = TranslationHelper.alwaysFalse();
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildBoolean(vm.globals, t);
  }

  public static Value lib_processutil_launchProcess(VmContext vm, Value[] args) {
    ObjectInstance bridge = ((ObjectInstance) args[0].internalValue);
    bridge.nativeData = new Object[5];
    bridge.nativeData[0] = true;
    bridge.nativeData[1] = 0;
    bridge.nativeData[2] = new ArrayList<String>();
    bridge.nativeData[3] = new ArrayList<String>();
    bridge.nativeData[4] = new Object();
    String execName = ((String) args[1].internalValue);
    ListImpl argsRaw = ((ListImpl) args[2].internalValue);
    boolean isAsync = ((boolean) args[3].internalValue);
    Value cb = args[4];
    ObjectInstance dispatcherQueue = ((ObjectInstance) args[5].internalValue);
    ArrayList<String> argStrings = new ArrayList<String>();
    int i = 0;
    while ((i < argsRaw.size)) {
      Value a = org.crayonlang.interpreter.vm.CrayonWrapper.getItemFromList(argsRaw, i);
      argStrings.add(((String) a.internalValue));
      i += 1;
    }
    TranslationHelper.alwaysFalse();
    return vm.globalNull;
  }

  public static Value lib_processutil_readBridge(VmContext vm, Value[] args) {
    ObjectInstance bridge = ((ObjectInstance) args[0].internalValue);
    ListImpl outputList = ((ListImpl) args[1].internalValue);
    int type = ((int) args[2].internalValue);
    Object mtx = bridge.nativeData[4];
    if ((type == 1)) {
      int outputInt = 0;;
      org.crayonlang.interpreter.vm.CrayonWrapper.addToList(outputList, org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, outputInt));
    } else {
      ArrayList<String> output = new ArrayList<String>();
      TranslationHelper.alwaysFalse();
      int i = 0;
      while ((i < output.size())) {
        org.crayonlang.interpreter.vm.CrayonWrapper.addToList(outputList, org.crayonlang.interpreter.vm.CrayonWrapper.buildString(vm.globals, output.get(i)));
        i += 1;
      }
    }
    return vm.globalNull;
  }
}
