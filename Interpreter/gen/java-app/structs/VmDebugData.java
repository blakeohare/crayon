package org.crayonlang.interpreter.structs;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.structs.*;

public final class VmDebugData {
  public boolean[] hasBreakpoint;
  public BreakpointInfo[] breakpointInfo;
  public HashMap<Integer, Integer> breakpointIdToPc;
  public int nextBreakpointId;
  public int nextStepId;
  public static final VmDebugData[] EMPTY_ARRAY = new VmDebugData[0];

  public VmDebugData(boolean[] hasBreakpoint, BreakpointInfo[] breakpointInfo, HashMap<Integer, Integer> breakpointIdToPc, int nextBreakpointId, int nextStepId) {
    this.hasBreakpoint = hasBreakpoint;
    this.breakpointInfo = breakpointInfo;
    this.breakpointIdToPc = breakpointIdToPc;
    this.nextBreakpointId = nextBreakpointId;
    this.nextStepId = nextStepId;
  }
}
