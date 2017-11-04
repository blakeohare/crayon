using Parser.ParseTree;
using System;
using System.Linq;

namespace Parser.Resolver
{
    internal static class MainFunctionFinder
    {
        public static FunctionDefinition Find(ParserContext parserContext)
        {
            FunctionDefinition[] mainFunctions = parserContext.UserCodeCompilationScope.GetTopLevelConstructs()
                .OfType<FunctionDefinition>()
                .Where(fd => fd.NameToken.Value == parserContext.Keywords.MAIN_FUNCTION)
                .ToArray();

            if (mainFunctions.Length == 0) throw new InvalidOperationException("No main(args) function was defined.");
            if (mainFunctions.Length > 1) throw new ParserException(mainFunctions[0].FirstToken, "Multiple main methods found.");

            FunctionDefinition mainFunction = mainFunctions[0];

            if (mainFunction.ArgNames.Length > 1)
            {
                throw new ParserException(mainFunctions[0].ArgNames[1], "main function cannot have more than one argument.");
            }

            parserContext.MainFunctionHasArg = mainFunction.ArgNames.Length == 1;

            return mainFunction;
        }
    }
}
