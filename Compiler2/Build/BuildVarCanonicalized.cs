namespace Build
{
    public enum VarType
    {
        BOOLEAN,
        INT,
        FLOAT,
        STRING
    }

    public class BuildVarCanonicalized
    {
        public string ID { get; set; }
        public VarType Type { get; set; }
        public string StringValue { get; set; }
        public int IntValue { get; set; }
        public bool BoolValue { get; set; }
        public double FloatValue { get; set; }
    }
}
