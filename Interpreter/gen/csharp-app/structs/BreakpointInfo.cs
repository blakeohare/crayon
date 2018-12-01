using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Structs
{
    public class BreakpointInfo
    {
        public int breakpointId;
        public bool isTransient;
        public Token token;

        public BreakpointInfo(int breakpointId, bool isTransient, Token token)
        {
            this.breakpointId = breakpointId;
            this.isTransient = isTransient;
            this.token = token;
        }
    }

}
