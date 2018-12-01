using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Structs
{
    public class DebugStepTracker
    {
        public int uniqueId;
        public int originatingFileId;
        public int originatingLineIndex;

        public DebugStepTracker(int uniqueId, int originatingFileId, int originatingLineIndex)
        {
            this.uniqueId = uniqueId;
            this.originatingFileId = originatingFileId;
            this.originatingLineIndex = originatingLineIndex;
        }
    }

}
