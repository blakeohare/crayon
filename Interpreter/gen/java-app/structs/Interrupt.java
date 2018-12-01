package org.crayonlang.interpreter.structs;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.structs.*;

public final class Interrupt {
  public int type;
  public int exceptionType;
  public String exceptionMessage;
  public double sleepDurationSeconds;
  public DebugStepTracker debugStepData;
  public static final Interrupt[] EMPTY_ARRAY = new Interrupt[0];

  public Interrupt(int type, int exceptionType, String exceptionMessage, double sleepDurationSeconds, DebugStepTracker debugStepData) {
    this.type = type;
    this.exceptionType = exceptionType;
    this.exceptionMessage = exceptionMessage;
    this.sleepDurationSeconds = sleepDurationSeconds;
    this.debugStepData = debugStepData;
  }
}
