package org.crayonlang.interpreter.structs;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.structs.*;

public final class ClassValue {
  public boolean isInterface;
  public int classId;
  public static final ClassValue[] EMPTY_ARRAY = new ClassValue[0];

  public ClassValue(boolean isInterface, int classId) {
    this.isInterface = isInterface;
    this.classId = classId;
  }
}
