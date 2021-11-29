using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Structs
{
    public class Value
    {
        public int type;
        public object internalValue;

        public Value(int type, object internalValue)
        {
            this.type = type;
            this.internalValue = internalValue;
        }
    }

}
