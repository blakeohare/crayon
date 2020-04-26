using Build;
using Common;
using Parser.ByteCode;
using Parser.ParseTree;

namespace Parser
{
    public class CompilationBundle
    {
        public string ByteCode { get; set; }
        public CompilationScope RootScope { get; set; }
        public CompilationScope[] AllScopes { get; set; }

        public static CompilationBundle Compile(BuildContext buildContext)
        {
            using (new PerformanceSection("ExportBundle.Compile"))
            {
                ParserContext parserContext = new ParserContext(buildContext);
                TopLevelEntity[] resolvedParseTree = parserContext.ParseAllTheThings();

                ByteCodeCompiler bcc = new ByteCodeCompiler();
                ByteBuffer buffer = bcc.GenerateByteCode(parserContext, resolvedParseTree);

                return new CompilationBundle()
                {
                    ByteCode = ByteCodeEncoder.Encode(buffer),
                    RootScope = parserContext.RootScope,
                    AllScopes = parserContext.ScopeManager.ImportedAssemblyScopes.ToArray(),
                };
            }
        }
    }
}
