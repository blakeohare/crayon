package org.crayonlang.interpreter.structs;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.structs.*;

public final class DebugStepTracker {
  public int uniqueId;
  public int originatingFileId;
  public int originatingLineIndex;
  public static final DebugStepTracker[] EMPTY_ARRAY = new DebugStepTracker[0];

  public DebugStepTracker(int uniqueId, int originatingFileId, int originatingLineIndex) {
    this.uniqueId = uniqueId;
    this.originatingFileId = originatingFileId;
    this.originatingLineIndex = originatingLineIndex;
  }
}
