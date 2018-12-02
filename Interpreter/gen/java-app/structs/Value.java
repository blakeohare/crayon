package org.crayonlang.interpreter.structs;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.PlatformTranslationHelper;;
import org.crayonlang.interpreter.structs.*;
import org.crayonlang.interpreter.TranslationHelper;;

public final class Value {
  public int type;
  public Object internalValue;
  public static final Value[] EMPTY_ARRAY = new Value[0];
  public int intValue;

  public Value(int type, Object internalValue) {
    this.type = type;
    this.internalValue = internalValue;
  }

  public Value(int intValue) {
    this.type = 3;
    this.intValue = intValue;
    this.internalValue = intValue;
  }

  public Value(boolean boolValue) {
    this.type = 2;
    this.intValue = boolValue ? 1 : 0;
    this.internalValue = boolValue;
  }
}
