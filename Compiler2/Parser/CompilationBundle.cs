using System.Linq;

namespace Parser
{
    public class CompilationBundle
    {
        public string ByteCode { get; set; }
        internal CompilationScope RootScope { get; set; }
        public AssemblyResolver.AssemblyMetadata[] RootScopeDependencyMetadata { get { return this.RootScope.Dependencies.Select(d => d.Scope.Metadata).ToArray(); } }
        internal CompilationScope[] AllScopes { get; set; }
        public AssemblyResolver.AssemblyMetadata[] AllScopesMetadata { get { return this.AllScopes.Select(s => s.Metadata).ToArray(); } }
        public Common.Error[] Errors { get; set; }
        public bool HasErrors { get { return this.Errors != null && this.Errors.Length > 0; } }
    }
}
