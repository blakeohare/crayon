using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Structs
{
    public class StackFrame
    {
        public int pc;
        public int localsStackSetToken;
        public int localsStackOffset;
        public int localsStackOffsetEnd;
        public StackFrame previous;
        public bool returnValueUsed;
        public Value objectContext;
        public int valueStackPopSize;
        public int markClassAsInitialized;
        public int depth;
        public int postFinallyBehavior;
        public Value returnValueTempStorage;
        public Dictionary<int, ClosureValuePointer> closureVariables;
        public DebugStepTracker debugStepTracker;

        public StackFrame(int pc, int localsStackSetToken, int localsStackOffset, int localsStackOffsetEnd, StackFrame previous, bool returnValueUsed, Value objectContext, int valueStackPopSize, int markClassAsInitialized, int depth, int postFinallyBehavior, Value returnValueTempStorage, Dictionary<int, ClosureValuePointer> closureVariables, DebugStepTracker debugStepTracker)
        {
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

}
