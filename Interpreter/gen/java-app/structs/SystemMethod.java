package org.crayonlang.interpreter.structs;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.structs.*;

public final class SystemMethod {
  public Value context;
  public int id;
  public static final SystemMethod[] EMPTY_ARRAY = new SystemMethod[0];

  public SystemMethod(Value context, int id) {
    this.context = context;
    this.id = id;
  }
}
