using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Structs
{
    public class SystemMethod
    {
        public Value context;
        public int id;

        public SystemMethod(Value context, int id)
        {
            this.context = context;
            this.id = id;
        }
    }

}
