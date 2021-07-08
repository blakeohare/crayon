namespace Crayon
{
    internal class ExternalCompilationBundle
    {
        public string ByteCode { get; set; }
        public string DependencyTreeJson { get; set; }
        public bool UsesU3 { get; set; }
        public Common.Error[] Errors { get; set; }
        public bool HasErrors { get { return this.Errors != null && this.Errors.Length > 0; } }
    }
}
