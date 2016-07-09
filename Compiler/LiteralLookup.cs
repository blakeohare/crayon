using System.Collections.Generic;

namespace Crayon
{
    internal class LiteralLookup
    {
        private readonly List<Types> literalTypes;
        private readonly List<object> literalValues;
        private Dictionary<string, int> literalIndexByKey;

        private readonly List<string> names;
        private Dictionary<string, int> nameIndexByName;

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

        public ByteBuffer BuildByteCode()
        {
            ByteBuffer output = new ByteBuffer();
            int size = this.literalTypes.Count;
            for (int i = 0; i < size; ++i)
            {
                Types type = this.literalTypes[i];
                object value = this.literalValues[i];
                switch (type)
                {
                    case Types.NULL:
                        output.Add(null, OpCode.ADD_LITERAL, (int)Types.NULL);
                        break;
                    case Types.BOOLEAN:
                        output.Add(null, OpCode.ADD_LITERAL, (int)Types.BOOLEAN, ((bool)value) ? 1 : 0);
                        break;
                    case Types.FLOAT:
                        output.Add(null, OpCode.ADD_LITERAL, value.ToString(), (int)Types.FLOAT);
                        break;
                    case Types.INTEGER:
                        output.Add(null, OpCode.ADD_LITERAL, (int)Types.INTEGER, (int)value);
                        break;
                    case Types.STRING:
                        output.Add(null, OpCode.ADD_LITERAL, value.ToString(), (int)Types.STRING);
                        break;
                }
            }

            size = this.names.Count;
            for (int i = 0; i < size; ++i)
            {
                output.Add(null, OpCode.ADD_NAME, this.names[i]);
            }

            return output;
        }
    }
}
