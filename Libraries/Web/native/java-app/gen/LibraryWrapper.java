package org.crayonlang.libraries.web;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.PlatformTranslationHelper;
import org.crayonlang.interpreter.structs.*;
import org.crayonlang.interpreter.TranslationHelper;

public final class LibraryWrapper {

  public static Value lib_web_launch_browser(VmContext vm, Value[] args) {
    String url = ((String) args[0].internalValue);
    LibraryWebBrowserLauncher.launchBrowser(url);
    return vm.globalNull;
  }
}
