package org.crayonlang.interpreter.structs;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.PlatformTranslationHelper;
import org.crayonlang.interpreter.structs.*;
import org.crayonlang.interpreter.TranslationHelper;

public final class LibRegObj {
  public ArrayList<Object> functionPointers;
  public ArrayList<String> functionNames;
  public ArrayList<Integer> argCounts;
  public static final LibRegObj[] EMPTY_ARRAY = new LibRegObj[0];

  public LibRegObj(ArrayList<Object> functionPointers, ArrayList<String> functionNames, ArrayList<Integer> argCounts) {
    this.functionPointers = functionPointers;
    this.functionNames = functionNames;
    this.argCounts = argCounts;
  }
}
