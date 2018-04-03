using Parser.ParseTree;
using System.Collections.Generic;

namespace Parser
{
    public class LiteralLookup
    {
        private readonly List<Types> literalTypes;
        private readonly List<object> literalValues;
        private Dictionary<string, int> literalIndexByKey;

        private readonly List<string> names;
        private Dictionary<string, int> nameIndexByName;

        public List<string> Names { get { return this.names; } }
        public List<Types> LiteralTypes { get { return this.literalTypes; } }
        public List<object> LiteralValues { get { return this.literalValues; } }

        public LiteralLookup()
        {
            this.literalTypes = new List<Types>();
            this.literalValues = new List<object>();
            this.literalIndexByKey = new Dictionary<string, int>();
            this.names = new List<string>();
            this.nameIndexByName = new Dictionary<string, int>();
        }

        private int GetIdForValue(string key, Types type, object value)
        {
            int id;
            if (!literalIndexByKey.TryGetValue(key, out id))
            {
                id = literalTypes.Count;
                literalTypes.Add(type);
                literalValues.Add(value);
                literalIndexByKey[key] = id;
            }
            return id;
        }

        public int GetNullId()
        {
            return this.GetIdForValue("x", Types.NULL, null);
        }

        public int GetBoolId(bool value)
        {
            return this.GetIdForValue(value ? "b1" : "b0", Types.BOOLEAN, value);
        }

        public int GetIntId(int value)
        {
            return this.GetIdForValue("i" + value, Types.INTEGER, value);
        }

        public int GetFloatId(double value)
        {
            return this.GetIdForValue("f" + value, Types.FLOAT, value);
        }

        public int GetStringId(string value)
        {
            return this.GetIdForValue("s" + value, Types.STRING, value);
        }

        public int GetClassRefId(ClassDefinition value)
        {
            return this.GetIdForValue("c" + value.ClassID, Types.CLASS, value.ClassID);
        }

        public int GetLibFuncRefId(string funcName)
        {
            return this.GetIdForValue("l" + funcName, Types.FUNCTION, funcName);
        }

        public int GetNameId(string value)
        {
            int id;
            if (!this.nameIndexByName.TryGetValue(value, out id))
            {
                id = this.names.Count;
                this.names.Add(value);
                this.nameIndexByName[value] = id;
            }
            return id;
        }
    }
}
