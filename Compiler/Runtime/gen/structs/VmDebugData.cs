using System.Collections.Generic;

namespace Interpreter.Structs
{
    public class VmDebugData
    {
        public bool[] hasBreakpoint;
        public BreakpointInfo[] breakpointInfo;
        public Dictionary<int, int> breakpointIdToPc;
        public int nextBreakpointId;
        public int nextStepId;

        public VmDebugData(bool[] hasBreakpoint, BreakpointInfo[] breakpointInfo, Dictionary<int, int> breakpointIdToPc, int nextBreakpointId, int nextStepId)
        {
            this.hasBreakpoint = hasBreakpoint;
            this.breakpointInfo = breakpointInfo;
            this.breakpointIdToPc = breakpointIdToPc;
            this.nextBreakpointId = nextBreakpointId;
            this.nextStepId = nextStepId;
        }
    }

}
