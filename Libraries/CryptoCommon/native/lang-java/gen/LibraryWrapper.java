package org.crayonlang.libraries.cryptocommon;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.PlatformTranslationHelper;
import org.crayonlang.interpreter.structs.*;
import org.crayonlang.interpreter.TranslationHelper;

public final class LibraryWrapper {

  public static Value lib_cryptocommon_addBytes(VmContext vm, Value[] args) {
    ObjectInstance obj = ((ObjectInstance) args[0].internalValue);
    ListImpl fromByteList = ((ListImpl) args[1].internalValue);
    ArrayList<Integer> toByteList = ((ArrayList<Integer>) obj.nativeData[0]);
    int length = fromByteList.size;
    int i = 0;
    while ((i < length)) {
      toByteList.add(fromByteList.array[i].intValue);
      i += 1;
    }
    return vm.globalFalse;
  }

  public static Value lib_cryptocommon_initHash(VmContext vm, Value[] args) {
    ObjectInstance obj = ((ObjectInstance) args[0].internalValue);
    obj.nativeData = new Object[1];
    obj.nativeData[0] = new ArrayList<Integer>();
    return vm.globalNull;
  }
}
