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
                parserContext.ScopeManager.ImportedAssemblyScopes.First(scope => scope.Dependencies.Length == 0),
                "_LIB_CORE_invoke");
            if (invokeFunctions.Length != 1) throw new Exception();
            parserContext.CoreLibInvokeFunction = invokeFunctions[0];
        }

        private static void FindMainFunction(ParserContext parserContext)
        {
            string delegateMainTo = parserContext.BuildContext.DelegateMainTo;

            CompilationScope scope = parserContext.RootScope;

            if (delegateMainTo != null)
            {
                scope = scope.Dependencies
                    .Where(d => d.Name == delegateMainTo)
                    .Select(lav => lav.Scope)
                    .FirstOrDefault();
                if (scope == null)
                {
                    throw new InvalidOperationException("delegate-main-to value is not a valid dependency of the main scope.");
                }
            }

            // TODO(localization): this will pose problems when the included scope is not the same locale.
            // The main function needs to be tagged as part of the scope's data when it's parsed.
            FunctionDefinition[] mainFunctions = FindFunctionImpl(
                parserContext,
                scope,
                parserContext.Keywords.MAIN_FUNCTION);

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
