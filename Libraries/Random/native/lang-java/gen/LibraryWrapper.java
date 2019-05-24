package org.crayonlang.libraries.random;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.PlatformTranslationHelper;
import org.crayonlang.interpreter.structs.*;
import org.crayonlang.interpreter.TranslationHelper;

public final class LibraryWrapper {

  private static java.util.Random PST_random = new java.util.Random();

  public static Value lib_random_random_bool(VmContext vm, Value[] args) {
    if ((PST_random.nextDouble() < 0.5)) {
      return vm.globalTrue;
    }
    return vm.globalFalse;
  }

  public static Value lib_random_random_float(VmContext vm, Value[] args) {
    return new Value(4, PST_random.nextDouble());
  }

  public static Value lib_random_random_int(VmContext vm, Value[] args) {
    if (((args[0].type != 3) || (args[1].type != 3))) {
      return vm.globalNull;
    }
    int lower = ((int) args[0].internalValue);
    int upper = ((int) args[1].internalValue);
    if ((lower >= upper)) {
      return vm.globalNull;
    }
    int value = ((int) ((PST_random.nextDouble() * (upper - lower))));
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildInteger(vm.globals, (lower + value));
  }
}
