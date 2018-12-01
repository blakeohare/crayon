package org.crayonlang.interpreter.structs;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.structs.*;

public final class MagicNumbers {
  public int coreExceptionClassId;
  public int coreGenerateExceptionFunctionId;
  public int totalLocaleCount;
  public static final MagicNumbers[] EMPTY_ARRAY = new MagicNumbers[0];

  public MagicNumbers(int coreExceptionClassId, int coreGenerateExceptionFunctionId, int totalLocaleCount) {
    this.coreExceptionClassId = coreExceptionClassId;
    this.coreGenerateExceptionFunctionId = coreGenerateExceptionFunctionId;
    this.totalLocaleCount = totalLocaleCount;
  }
}
