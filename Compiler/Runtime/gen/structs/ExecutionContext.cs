namespace Interpreter.Structs
{
    public class ExecutionContext
    {
        public int id;
        public StackFrame stackTop;
        public int currentValueStackSize;
        public int valueStackCapacity;
        public Value[] valueStack;
        public Value[] localsStack;
        public int[] localsStackSet;
        public int localsStackSetToken;
        public int executionCounter;
        public bool activeExceptionHandled;
        public Value activeException;
        public bool executionStateChange;
        public int executionStateChangeCommand;
        public Interrupt activeInterrupt;

        public ExecutionContext(int id, StackFrame stackTop, int currentValueStackSize, int valueStackCapacity, Value[] valueStack, Value[] localsStack, int[] localsStackSet, int localsStackSetToken, int executionCounter, bool activeExceptionHandled, Value activeException, bool executionStateChange, int executionStateChangeCommand, Interrupt activeInterrupt)
        {
            this.id = id;
            this.stackTop = stackTop;
            this.currentValueStackSize = currentValueStackSize;
            this.valueStackCapacity = valueStackCapacity;
            this.valueStack = valueStack;
            this.localsStack = localsStack;
            this.localsStackSet = localsStackSet;
            this.localsStackSetToken = localsStackSetToken;
            this.executionCounter = executionCounter;
            this.activeExceptionHandled = activeExceptionHandled;
            this.activeException = activeException;
            this.executionStateChange = executionStateChange;
            this.executionStateChangeCommand = executionStateChangeCommand;
            this.activeInterrupt = activeInterrupt;
        }
    }

}
