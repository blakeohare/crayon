namespace Build
{
    public enum VarType
    {
        NULL,
        BOOLEAN,
        INT,
        FLOAT,
        STRING,
        UNKNOWN,
    }

    public class BuildVarCanonicalized
    {
        public BuildVarCanonicalized(string id, object value)
        {
            this.ID = id;
            if (value == null)
            {
                this.Type = VarType.NULL;
            }
            else if (value is string)
            {
                this.Type = VarType.STRING;
                this.StringValue = value.ToString();
            }
            else if (value is int)
            {
                this.Type = VarType.INT;
                this.IntValue = (int)value;
            }
            else if (value is double)
            {
                this.Type = VarType.FLOAT;
                this.FloatValue = (double)value;
            }
            else if (value is bool)
            {
                this.Type = VarType.BOOLEAN;
                this.BoolValue = (bool)value;
            }
            else
            {
                this.Type = VarType.UNKNOWN;
            }
        }

        public string ID { get; set; }
        public VarType Type { get; set; }
        public string StringValue { get; set; }
        public int IntValue { get; set; }
        public bool BoolValue { get; set; }
        public double FloatValue { get; set; }
    }
}
