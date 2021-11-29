using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Structs
{
    public class ClassValue
    {
        public bool isInterface;
        public int classId;

        public ClassValue(bool isInterface, int classId)
        {
            this.isInterface = isInterface;
            this.classId = classId;
        }
    }

}
