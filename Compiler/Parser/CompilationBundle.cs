using Common;
using System.Linq;

namespace Parser
{
    public class InternalCompilationBundle
    {
        public string ByteCode { get; set; }
        internal CompilationScope RootScope { get; set; }
        public ExternalAssemblyMetadata[] RootScopeDependencyMetadata { get { return this.RootScope.Dependencies.Select(d => d.Scope.Metadata).ToArray(); } }
        internal CompilationScope[] AllScopes { get; set; }
        public ExternalAssemblyMetadata[] AllScopesMetadata { get { return this.AllScopes.Select(s => s.Metadata).ToArray(); } }
        public Common.Error[] Errors { get; set; }
        public bool HasErrors { get { return this.Errors != null && this.Errors.Length > 0; } }
    }

    public class ExternalCompilationBundle
    {
        public string ByteCode { get; set; }
        public string DependencyTreeJson { get; set; }
        public bool UsesU3 { get; set; }
        public Common.Error[] Errors { get; set; }
        public bool HasErrors { get { return this.Errors != null && this.Errors.Length > 0; } }
    }

    public static class CompilerTODO
    {
        public static ExternalCompilationBundle Convert(InternalCompilationBundle icb)
        {
            string depTree = AssemblyDependencyUtil.GetDependencyTreeJson(icb.RootScopeDependencyMetadata).Trim();
            return new ExternalCompilationBundle()
            {
                DependencyTreeJson = depTree,
                ByteCode = icb.ByteCode,
                UsesU3 = icb.AllScopesMetadata.Any(a => a.ID == "U3Direct"),
                Errors = icb.Errors,
            };
        }
    }
}
