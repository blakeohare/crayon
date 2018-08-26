using Parser.ParseTree;
using System;
using System.Linq;

namespace Parser.Resolver
{
    internal static class SpecialFunctionFinder
    {
        public static void Run(ParserContext parserContext)
        {
            FindLibCoreInvoke(parserContext);
            FindMainFunction(parserContext);
        }

        private static FunctionDefinition[] FindFunctionImpl(ParserContext parser, CompilationScope scope, string name)
        {
            return scope.GetTopLevelConstructs()
                .OfType<FunctionDefinition>()
                .Where(fd => fd.NameToken.Value == name)
                .ToArray();
        }

        private static void FindLibCoreInvoke(ParserContext parserContext)
        {
            FunctionDefinition[] invokeFunctions = FindFunctionImpl(
                parserContext,
                parserContext.LibraryManager.ImportedLibraries.First(lib => lib.Dependencies.Length == 0),
                "_LIB_CORE_invoke");
            if (invokeFunctions.Length != 1) throw new Exception();
            parserContext.CoreLibInvokeFunction = invokeFunctions[0];
        }

        private static void FindMainFunction(ParserContext parserContext)
        {
            FunctionDefinition[] mainFunctions = FindFunctionImpl(parserContext, parserContext.UserCodeCompilationScope, parserContext.Keywords.MAIN_FUNCTION);

            if (mainFunctions.Length == 0) throw new InvalidOperationException("No main(args) function was defined.");
            if (mainFunctions.Length > 1) throw new ParserException(mainFunctions[0], "Multiple main methods found.");

            FunctionDefinition mainFunction = mainFunctions[0];

            if (mainFunction.ArgNames.Length > 1)
            {
                throw new ParserException(mainFunctions[0].ArgNames[1], "main function cannot have more than one argument.");
            }

            parserContext.MainFunction = mainFunction;
            parserContext.MainFunctionHasArg = mainFunction.ArgNames.Length == 1;
        }
    }
}
