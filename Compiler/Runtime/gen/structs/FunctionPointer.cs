using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Structs
{
    public class FunctionPointer
    {
        public int type;
        public Value context;
        public int classId;
        public int functionId;
        public Dictionary<int, ClosureValuePointer> closureVariables;

        public FunctionPointer(int type, Value context, int classId, int functionId, Dictionary<int, ClosureValuePointer> closureVariables)
        {
            this.type = type;
            this.context = context;
            this.classId = classId;
            this.functionId = functionId;
            this.closureVariables = closureVariables;
        }
    }

}
