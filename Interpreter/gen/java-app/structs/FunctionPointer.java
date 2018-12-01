package org.crayonlang.interpreter.structs;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.structs.*;

public final class FunctionPointer {
  public int type;
  public Value context;
  public int classId;
  public int functionId;
  public HashMap<Integer, ClosureValuePointer> closureVariables;
  public static final FunctionPointer[] EMPTY_ARRAY = new FunctionPointer[0];

  public FunctionPointer(int type, Value context, int classId, int functionId, HashMap<Integer, ClosureValuePointer> closureVariables) {
    this.type = type;
    this.context = context;
    this.classId = classId;
    this.functionId = functionId;
    this.closureVariables = closureVariables;
  }
}
