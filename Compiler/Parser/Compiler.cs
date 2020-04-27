using Build;
using Common;
using Parser.ByteCode;
using Parser.ParseTree;

namespace Parser
{
    public static class Compiler
    {
        public static CompilationBundle Compile(BuildContext buildContext, bool isRelease)
        {
            CompilationBundle result;
            if (isRelease)
            {
                try
                {
                    result = CompileImpl(buildContext);
                }
                catch (MultiParserException multiException)
                {
                    result = new CompilationBundle()
                    {
                        Errors = multiException.ToCompilerErrors(),
                    };
                }
                catch (ParserException exception)
                {
                    result = new CompilationBundle()
                    {
                        Errors = new Error[] { exception.ToCompilerError() },
                    };
                }
            }
            else
            {
                result = CompileImpl(buildContext);
            }
            return result;
        }

        private static CompilationBundle CompileImpl(BuildContext buildContext)
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
