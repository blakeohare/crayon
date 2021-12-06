namespace Build.BuildParseNodes
{
    public abstract class BuildItem
    {
        public Wax.BuildArg[] BuildArgs { get; set; }
        public Wax.ExtensionArg[] ExtensionArgs { get; set; }
        public BuildVarCanonicalized[] BuildVars { get; set; }
    }
}
