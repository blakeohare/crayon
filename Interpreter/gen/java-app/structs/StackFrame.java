package org.crayonlang.interpreter.structs;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.structs.*;

public final class StackFrame {
  public int pc;
  public int localsStackSetToken;
  public int localsStackOffset;
  public int localsStackOffsetEnd;
  public StackFrame previous;
  public boolean returnValueUsed;
  public Value objectContext;
  public int valueStackPopSize;
  public int markClassAsInitialized;
  public int depth;
  public int postFinallyBehavior;
  public Value returnValueTempStorage;
  public HashMap<Integer, ClosureValuePointer> closureVariables;
  public DebugStepTracker debugStepTracker;
  public static final StackFrame[] EMPTY_ARRAY = new StackFrame[0];

  public StackFrame(int pc, int localsStackSetToken, int localsStackOffset, int localsStackOffsetEnd, StackFrame previous, boolean returnValueUsed, Value objectContext, int valueStackPopSize, int markClassAsInitialized, int depth, int postFinallyBehavior, Value returnValueTempStorage, HashMap<Integer, ClosureValuePointer> closureVariables, DebugStepTracker debugStepTracker) {
    this.pc = pc;
    this.localsStackSetToken = localsStackSetToken;
    this.localsStackOffset = localsStackOffset;
    this.localsStackOffsetEnd = localsStackOffsetEnd;
    this.previous = previous;
    this.returnValueUsed = returnValueUsed;
    this.objectContext = objectContext;
    this.valueStackPopSize = valueStackPopSize;
    this.markClassAsInitialized = markClassAsInitialized;
    this.depth = depth;
    this.postFinallyBehavior = postFinallyBehavior;
    this.returnValueTempStorage = returnValueTempStorage;
    this.closureVariables = closureVariables;
    this.debugStepTracker = debugStepTracker;
  }
}
