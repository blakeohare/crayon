namespace Build.BuildParseNodes
{
    public class BuildVar
    {
        public string Id { get; set; }
        public object Value { get; set; }
        public string EnvFileReference { get; set; }

        public VarType Type
        {
            get
            {
                object value = this.Value;
                if (value == null) return VarType.NULL;
                if (value is int) return VarType.INT;
                if (value is float || value is double) return VarType.FLOAT;
                if (value is bool) return VarType.BOOLEAN;
                if (value is string) return VarType.STRING;

                return VarType.UNKNOWN;
            }
        }

        public void ResolveEnvFileReference(System.Collections.Generic.IDictionary<string, object> envLookup)
        {
            if (envLookup.ContainsKey(this.EnvFileReference))
            {
                this.Value = envLookup[this.EnvFileReference];
            }
            else
            {
                this.Value = null;
            }
            this.EnsureTypeValid();
        }

        public void EnsureTypeValid()
        {
            if (this.Type == VarType.UNKNOWN) throw new System.InvalidOperationException("Complex types are not allowed for build variables.");
        }

    }
}
