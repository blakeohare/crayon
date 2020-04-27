namespace Parser
{
    public class CompilationBundle
    {
        public string ByteCode { get; set; }
        public CompilationScope RootScope { get; set; }
        public CompilationScope[] AllScopes { get; set; }
        public Common.Error[] Errors { get; set; }
        public bool HasErrors { get { return this.Errors != null && this.Errors.Length > 0; } }
    }
}
