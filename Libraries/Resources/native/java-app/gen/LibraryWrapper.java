package org.crayonlang.libraries.resources;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.PlatformTranslationHelper;
import org.crayonlang.interpreter.structs.*;
import org.crayonlang.interpreter.TranslationHelper;

public final class LibraryWrapper {

  public static Value lib_resources_getResourceData(VmContext vm, Value[] args) {
    Value output = org.crayonlang.interpreter.vm.CrayonWrapper.buildList(vm.resourceDatabase.dataList);
    vm.resourceDatabase.dataList = null;
    return output;
  }

  public static Value lib_resources_readText(VmContext vm, Value[] args) {
    String string1 = org.crayonlang.interpreter.ResourceReader.readFileText("resources/text/" + ((String) args[0].internalValue));
    return org.crayonlang.interpreter.vm.CrayonWrapper.buildString(vm.globals, string1);
  }
}
