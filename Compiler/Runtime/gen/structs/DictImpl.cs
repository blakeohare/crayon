using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Structs
{
    public class DictImpl
    {
        public int size;
        public int keyType;
        public int keyClassId;
        public int[] valueType;
        public Dictionary<int, int> intToIndex;
        public Dictionary<string, int> stringToIndex;
        public List<Value> keys;
        public List<Value> values;

        public DictImpl(int size, int keyType, int keyClassId, int[] valueType, Dictionary<int, int> intToIndex, Dictionary<string, int> stringToIndex, List<Value> keys, List<Value> values)
        {
            this.size = size;
            this.keyType = keyType;
            this.keyClassId = keyClassId;
            this.valueType = valueType;
            this.intToIndex = intToIndex;
            this.stringToIndex = stringToIndex;
            this.keys = keys;
            this.values = values;
        }
    }

}
