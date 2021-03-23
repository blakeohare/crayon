using Common;
using CommonUtil;

namespace Build.BuildParseNodes
{
    public abstract class BuildItem
    {
        public string ProjectId { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public SourceItem[] Sources { get; set; }
        public SourceItem[] SourcesNonNull { get { return this.Sources ?? new SourceItem[0]; } }
        public string Output { get; set; }
        public string JsFilePrefix { get; set; }
        public bool JsFullPage { get { return (this.JsFullPageRaw ?? new NullableBoolean(false)).Value; } }
        internal NullableBoolean JsFullPageRaw { get; set; }
        public bool Minified { get { return (this.MinifiedRaw ?? new NullableBoolean(false)).Value; } }
        internal NullableBoolean MinifiedRaw { get; set; }
        public string ExportDebugByteCodeRaw { get; set; }
        public BuildVar[] Var { get; set; }
        public string GuidSeed { get; set; }
        public string[] IconFilePaths { get; set; }
        public bool HasLegacyIcon { get; set; }
        public string LaunchScreen { get; set; }
        public string ProjectTitle { get; set; }
        public bool HasLegacyTitle { get; set; }
        public string DelegateMainTo { get; set; }
        public string EnvFile { get; set; }
        public NullableBoolean RemoveSymbols { get; set; }

        // comma-delimited list of values
        // { portrait | upsidedown | landscape | landscapeleft | landscaperight | all }
        // landscape is a shortcut of landscapeleft,landscaperight
        // see Common/OrientationParser.cs
        public string Orientation { get; set; }
        public string[] LocalDeps { get; set; }
        public string[] RemoteDeps { get; set; }
        public string IosBundlePrefix { get; set; }
        public string JavaPackage { get; set; }
        public string CompilerLocale { get; set; }

        public bool ExportDebugByteCode
        {
            get { return BoolUtil.Parse(this.ExportDebugByteCodeRaw); }
        }
    }
}
