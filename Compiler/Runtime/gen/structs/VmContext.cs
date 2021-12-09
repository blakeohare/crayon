using System.Collections.Generic;

namespace Interpreter.Structs
{
    public class VmContext
    {
        public Dictionary<int, ExecutionContext> executionContexts;
        public int lastExecutionContextId;
        public Code byteCode;
        public SymbolData symbolData;
        public VmMetadata metadata;
        public int instanceCounter;
        public bool initializationComplete;
        public List<int> classStaticInitializationStack;
        public Value[] funcArgs;
        public ResourceDB resourceDatabase;
        public List<Value> shutdownHandlers;
        public VmEnvironment environment;
        public NamedCallbackStore namedCallbacks;
        public VmGlobals globals;
        public Value globalNull;
        public Value globalTrue;
        public Value globalFalse;
        public int id;

        public VmContext(Dictionary<int, ExecutionContext> executionContexts, int lastExecutionContextId, Code byteCode, SymbolData symbolData, VmMetadata metadata, int instanceCounter, bool initializationComplete, List<int> classStaticInitializationStack, Value[] funcArgs, ResourceDB resourceDatabase, List<Value> shutdownHandlers, VmEnvironment environment, NamedCallbackStore namedCallbacks, VmGlobals globals, Value globalNull, Value globalTrue, Value globalFalse, int id)
        {
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
            this.id = id;
        }
    }

}
