namespace Builder.Build.BuildParseNodes
{
    public class Target : BuildItem
    {
        public string Name { get; set; }
        public string InheritFrom { get; set; }
    }
}
