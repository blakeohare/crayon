using Builder.ByteCode;
using Builder.ParseTree;
using Wax;

namespace Builder
{
    internal static class Compiler
    {
        public static InternalCompilationBundle Compile(CompileRequest compileRequest, WaxHub waxHub)
        {
            InternalCompilationBundle result;
            if (waxHub.ErrorsAsExceptions)
            {
                result = CompileImpl(compileRequest, waxHub);
            }
            else
            {
                try
                {
                    result = CompileImpl(compileRequest, waxHub);
                }
                catch (MultiParserException multiException)
                {
                    result = new InternalCompilationBundle()
                    {
                        Errors = multiException.ToCompilerErrors(),
                    };
                }
                catch (ParserException exception)
                {
                    result = new InternalCompilationBundle()
                    {
                        Errors = new Error[] { exception.ToCompilerError() },
                    };
                }
                catch (System.InvalidOperationException exception)
                {
                    result = new InternalCompilationBundle()
                    {
                        Errors = new Error[] { new Error() { Message = exception.Message } },
                    };
                }
            }
            return result;
        }

        private static InternalCompilationBundle CompileImpl(CompileRequest compileRequest, Wax.WaxHub waxHub)
        {
            ParserContext parserContext = new ParserContext(compileRequest, waxHub);
            TopLevelEntity[] resolvedParseTree = parserContext.ParseAllTheThings();

            ByteCodeCompiler bcc = new ByteCodeCompiler();
            ByteBuffer buffer = bcc.GenerateByteCode(parserContext, resolvedParseTree);

            return new InternalCompilationBundle()
            {
                ByteCode = ByteCodeEncoder.Encode(buffer),
                RootScope = parserContext.RootScope,
                AllScopes = parserContext.ScopeManager.ImportedAssemblyScopes.ToArray(),
            };
        }
    }
}
