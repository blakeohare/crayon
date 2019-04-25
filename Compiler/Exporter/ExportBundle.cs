using Build;
using Common;
using Exporter.ByteCode;
using Parser;
using System.Collections.Generic;

namespace Exporter
{
    public class ExportBundle
    {
        public ByteBuffer ByteCode { get; set; }
        public string ProjectID { get; set; }
        public string GuidSeed { get; set; }
        public ICollection<CompilationScope> LibraryScopesUsed { get; set; }
        public CompilationScope UserCodeScope { get; set; }
        public string[] IconPaths { get; set; }
        public string LaunchScreenPath { get; set; }
        public string DefaultTitle { get; set; }
        public string JsFilePrefix { get; set; }
        public bool JsFullPage { get; set; }
        public string IosBundlePrefix { get; set; }
        public string JavaPackage { get; set; }
        public NullableInteger WindowWidth { get; set; }
        public NullableInteger WindowHeight { get; set; }
        public string Orientations { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }

        public static ExportBundle Compile(BuildContext buildContext)
        {
            using (new PerformanceSection("ExportBundle.Compile"))
            {
                ParserContext parserContext = new ParserContext(buildContext);
                Parser.ParseTree.TopLevelEntity[] resolvedParseTree = parserContext.ParseAllTheThings();

                ByteCodeCompiler bcc = new ByteCodeCompiler();
                ByteBuffer buffer = bcc.GenerateByteCode(parserContext, resolvedParseTree);

                string jsFilePrefix = buildContext.JsFilePrefix;
                jsFilePrefix = (jsFilePrefix == null || jsFilePrefix == "" || jsFilePrefix == "/")
                    ? ""
                    : ("/" + buildContext.JsFilePrefix.Trim('/') + "/");

                return new ExportBundle()
                {
                    ByteCode = buffer,
                    UserCodeScope = parserContext.RootScope,
                    LibraryScopesUsed = parserContext.ScopeManager.ImportedAssemblyScopes.ToArray(),
                    ProjectID = buildContext.ProjectID,
                    Version = buildContext.TopLevelAssembly.Version,
                    Description = buildContext.TopLevelAssembly.Description,
                    GuidSeed = buildContext.GuidSeed,
                    DefaultTitle = buildContext.DefaultTitle,
                    JsFilePrefix = jsFilePrefix,
                    JsFullPage = buildContext.JsFullPage,
                    IosBundlePrefix = buildContext.IosBundlePrefix,
                    JavaPackage = buildContext.JavaPackage,
                    IconPaths = buildContext.IconFilePaths,
                    LaunchScreenPath = buildContext.LaunchScreenPath,
                    WindowWidth = buildContext.WindowWidth,
                    WindowHeight = buildContext.WindowHeight,
                    Orientations = buildContext.Orientation,
                };
            }
        }
    }
}
