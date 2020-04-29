namespace Parser
{
    public class CompileRequest
    {
        public Build.BuildContext BuildContext { get; set; }
        public string ProjectId { get { return this.BuildContext.ProjectID; } }
        public Build.AssemblyContext TopLevelAssembly { get { return this.BuildContext.TopLevelAssembly; } }
        public string DelegateMainTo { get { return this.BuildContext.DelegateMainTo; } }
        public Localization.Locale CompilerLocale { get { return this.BuildContext.CompilerLocale; } }
        public string[] LocalDeps { get { return this.BuildContext.LocalDeps; } }
        public string[] RemoteDeps { get { return this.BuildContext.RemoteDeps; } }
        public string ProjectDirectory { get { return this.BuildContext.ProjectDirectory; } }
    }
}
