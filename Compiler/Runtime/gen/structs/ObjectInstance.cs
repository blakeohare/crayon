using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Structs
{
    public class ObjectInstance
    {
        public int classId;
        public int objectId;
        public Value[] members;
        public object[] nativeData;
        public object nativeObject;

        public ObjectInstance(int classId, int objectId, Value[] members, object[] nativeData, object nativeObject)
        {
            this.classId = classId;
            this.objectId = objectId;
            this.members = members;
            this.nativeData = nativeData;
            this.nativeObject = nativeObject;
        }
    }

}
