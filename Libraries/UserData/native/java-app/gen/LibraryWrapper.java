package org.crayonlang.libraries.userdata;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.PlatformTranslationHelper;
import org.crayonlang.interpreter.structs.*;
import org.crayonlang.interpreter.TranslationHelper;

public final class LibraryWrapper {

  public static Value lib_userdata_getProjectSandboxDirectory(VmContext vm, Value[] args) {
    Value output = vm.globalNull;
    Value arg1 = args[0];
    return output;
  }
}
