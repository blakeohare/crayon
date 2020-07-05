using Common;
using Parser.ByteCode;
using Parser.ParseTree;

namespace Parser
{
    public static class Compiler
    {
        public static CompilationBundle Compile(CompileRequest compileRequest, bool isRelease)
        {
            CompilationBundle result;
            if (isRelease)
            {
                try
                {
                    result = CompileImpl(compileRequest);
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
                result = CompileImpl(compileRequest);
            }
            return result;
        }

        private static CompilationBundle CompileImpl(CompileRequest compileRequest)
        {
            using (new PerformanceSection("ExportBundle.Compile"))
            {
                ParserContext parserContext = new ParserContext(compileRequest);
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
