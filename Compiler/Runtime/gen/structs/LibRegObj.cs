using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Structs
{
    public class LibRegObj
    {
        public List<object> functionPointers;
        public List<string> functionNames;
        public List<int> argCounts;

        public LibRegObj(List<object> functionPointers, List<string> functionNames, List<int> argCounts)
        {
            this.functionPointers = functionPointers;
            this.functionNames = functionNames;
            this.argCounts = argCounts;
        }
    }

}
