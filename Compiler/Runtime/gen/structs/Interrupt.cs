using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Structs
{
    public class Interrupt
    {
        public int type;
        public int exceptionType;
        public string exceptionMessage;
        public double sleepDurationSeconds;
        public DebugStepTracker debugStepData;

        public Interrupt(int type, int exceptionType, string exceptionMessage, double sleepDurationSeconds, DebugStepTracker debugStepData)
        {
            this.type = type;
            this.exceptionType = exceptionType;
            this.exceptionMessage = exceptionMessage;
            this.sleepDurationSeconds = sleepDurationSeconds;
            this.debugStepData = debugStepData;
        }
    }

}
