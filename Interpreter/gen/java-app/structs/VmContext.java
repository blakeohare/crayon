package org.crayonlang.interpreter.structs;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.PlatformTranslationHelper;;
import org.crayonlang.interpreter.structs.*;
import org.crayonlang.interpreter.TranslationHelper;;

public final class VmContext {
  public HashMap<Integer, ExecutionContext> executionContexts;
  public int lastExecutionContextId;
  public Code byteCode;
  public SymbolData symbolData;
  public VmMetadata metadata;
  public int instanceCounter;
  public boolean initializationComplete;
  public ArrayList<Integer> classStaticInitializationStack;
  public Value[] funcArgs;
  public ResourceDB resourceDatabase;
  public ArrayList<Value> shutdownHandlers;
  public VmEnvironment environment;
  public NamedCallbackStore namedCallbacks;
  public VmGlobals globals;
  public Value globalNull;
  public Value globalTrue;
  public Value globalFalse;
  public static final VmContext[] EMPTY_ARRAY = new VmContext[0];

  public VmContext(HashMap<Integer, ExecutionContext> executionContexts, int lastExecutionContextId, Code byteCode, SymbolData symbolData, VmMetadata metadata, int instanceCounter, boolean initializationComplete, ArrayList<Integer> classStaticInitializationStack, Value[] funcArgs, ResourceDB resourceDatabase, ArrayList<Value> shutdownHandlers, VmEnvironment environment, NamedCallbackStore namedCallbacks, VmGlobals globals, Value globalNull, Value globalTrue, Value globalFalse) {
    this.executionContexts = executionContexts;
    this.lastExecutionContextId = lastExecutionContextId;
    this.byteCode = byteCode;
    this.symbolData = symbolData;
    this.metadata = metadata;
    this.instanceCounter = instanceCounter;
    this.initializationComplete = initializationComplete;
    this.classStaticInitializationStack = classStaticInitializationStack;
    this.funcArgs = funcArgs;
    this.resourceDatabase = resourceDatabase;
    this.shutdownHandlers = shutdownHandlers;
    this.environment = environment;
    this.namedCallbacks = namedCallbacks;
    this.globals = globals;
    this.globalNull = globalNull;
    this.globalTrue = globalTrue;
    this.globalFalse = globalFalse;
  }
}
