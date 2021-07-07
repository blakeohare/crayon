using Common;
using Parser.ByteCode;
using Parser.ParseTree;

namespace Parser
{
    public static class Compiler
    {
        public static CompilationBundle Compile(CompileRequest compileRequest, bool isRelease, CommonUtil.Wax.WaxHub waxHub)
        {
            CompilationBundle result;
            if (isRelease)
            {
                try
                {
                    result = CompileImpl(compileRequest, waxHub);
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
                catch (System.InvalidOperationException exception)
                {
                    result = new CompilationBundle()
                    {
                        Errors = new Error[] { new Error() { Message = exception.Message } },
                    };
                }
            }
            else
            {
                result = CompileImpl(compileRequest, waxHub);
            }
            return result;
        }

        private static CompilationBundle CompileImpl(CompileRequest compileRequest, CommonUtil.Wax.WaxHub waxHub)
        {
            using (new PerformanceSection("ExportBundle.Compile"))
            {
                ParserContext parserContext = new ParserContext(compileRequest, waxHub);
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
