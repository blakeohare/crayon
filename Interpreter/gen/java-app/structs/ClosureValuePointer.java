package org.crayonlang.interpreter.structs;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.PlatformTranslationHelper;
import org.crayonlang.interpreter.structs.*;
import org.crayonlang.interpreter.TranslationHelper;

public final class ClosureValuePointer {
  public Value value;
  public static final ClosureValuePointer[] EMPTY_ARRAY = new ClosureValuePointer[0];

  public ClosureValuePointer(Value value) {
    this.value = value;
  }
}
