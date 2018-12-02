package org.crayonlang.interpreter.structs;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.PlatformTranslationHelper;;
import org.crayonlang.interpreter.structs.*;
import org.crayonlang.interpreter.TranslationHelper;;

public final class FunctionInfo {
  public int id;
  public int nameId;
  public int pc;
  public int minArgs;
  public int maxArgs;
  public int type;
  public int associatedClassId;
  public int localsSize;
  public int[] pcOffsetsForOptionalArgs;
  public String name;
  public int[] closureIds;
  public static final FunctionInfo[] EMPTY_ARRAY = new FunctionInfo[0];

  public FunctionInfo(int id, int nameId, int pc, int minArgs, int maxArgs, int type, int associatedClassId, int localsSize, int[] pcOffsetsForOptionalArgs, String name, int[] closureIds) {
    this.id = id;
    this.nameId = nameId;
    this.pc = pc;
    this.minArgs = minArgs;
    this.maxArgs = maxArgs;
    this.type = type;
    this.associatedClassId = associatedClassId;
    this.localsSize = localsSize;
    this.pcOffsetsForOptionalArgs = pcOffsetsForOptionalArgs;
    this.name = name;
    this.closureIds = closureIds;
  }
}
