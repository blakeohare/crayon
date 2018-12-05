namespace Build.BuildParseNodes
{
    public class BuildVar
    {
        public string Id { get; set; }

        public VarType Type { get; set; }

        public object Value { get; set; }
    }
}
