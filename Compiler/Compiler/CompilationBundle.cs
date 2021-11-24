using System.Linq;

namespace Parser
{
    internal class InternalCompilationBundle
    {
        public string ByteCode { get; set; }
        internal CompilationScope RootScope { get; set; }
        public ExternalAssemblyMetadata[] RootScopeDependencyMetadata { get { return this.RootScope.Dependencies.Select(d => d.Scope.Metadata).ToArray(); } }
        internal CompilationScope[] AllScopes { get; set; }
        public ExternalAssemblyMetadata[] AllScopesMetadata { get { return this.AllScopes.Select(s => s.Metadata).ToArray(); } }
        public Wax.Error[] Errors { get; set; }
        public bool HasErrors { get { return this.Errors != null && this.Errors.Length > 0; } }
    }
}
