package org.crayonlang.libraries.json;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.PlatformTranslationHelper;
import org.crayonlang.interpreter.structs.*;
import org.crayonlang.interpreter.TranslationHelper;

public final class LibraryWrapper {

  public static Value lib_json_parse(VmContext vm, Value[] args) {
    String raw = ((String) args[0].internalValue);
    if ((raw.length() > 0)) {
      Value output = JsonParser.parseJsonIntoValue(vm.globals, raw);
      if ((output != null)) {
        return output;
      }
    }
    return vm.globalNull;
  }
}
