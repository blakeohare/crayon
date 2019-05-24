package org.crayonlang.libraries.environment;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.PlatformTranslationHelper;
import org.crayonlang.interpreter.structs.*;
import org.crayonlang.interpreter.TranslationHelper;

public final class LibraryWrapper {

  public static Value lib_environment_get(VmContext vm, Value[] args) {
    String value = System.getenv(((String) args[0].internalValue));
    if ((value == null)) {
      return vm.globalNull;
    }
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildString(vm.globals, value);
  }
}
