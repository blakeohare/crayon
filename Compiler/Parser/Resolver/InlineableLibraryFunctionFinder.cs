using Common;
using Parser.ParseTree;
using System.Collections.Generic;
using System.Linq;

namespace Parser.Resolver
{
    internal static class InlineableLibraryFunctionFinder
    {
        public static HashSet<FunctionDefinition> Find(ICollection<TopLevelConstruct> entities)
        {
            using (new PerformanceSection("DetermineInlinableLibraryFunctions"))
            {
                HashSet<FunctionDefinition> inlineCandidates = new HashSet<FunctionDefinition>();

                List<FunctionDefinition> allFlattenedFunctions = new List<FunctionDefinition>(entities.OfType<FunctionDefinition>());
                foreach (ClassDefinition cd in entities.OfType<ClassDefinition>())
                {
                    allFlattenedFunctions.AddRange(cd.Methods.Where(fd => fd.IsStaticMethod));
                }

                foreach (FunctionDefinition funcDef in allFlattenedFunctions)
                {
                    // Look for function definitions that are in libraries that have one single line of code that's a return statement that
                    // invokes a native code.
                    if (funcDef.Library != null && funcDef.Code.Length == 1)
                    {
                        ReturnStatement returnStatement = funcDef.Code[0] as ReturnStatement;
                        if (returnStatement != null)
                        {
                            Expression[] argsFromLibOrCoreFunction = null;
                            if (returnStatement.Expression is LibraryFunctionCall)
                            {
                                argsFromLibOrCoreFunction = ((LibraryFunctionCall)returnStatement.Expression).Args;
                            }
                            else if (returnStatement.Expression is CoreFunctionInvocation)
                            {
                                argsFromLibOrCoreFunction = ((CoreFunctionInvocation)returnStatement.Expression).Args;
                            }

                            if (argsFromLibOrCoreFunction != null)
                            {
                                bool allSimpleVariables = true;
                                foreach (Expression expr in argsFromLibOrCoreFunction)
                                {
                                    if (!expr.IsInlineCandidate)
                                    {
                                        allSimpleVariables = false;
                                        break;
                                    }
                                }

                                if (allSimpleVariables)
                                {
                                    inlineCandidates.Add(funcDef);
                                }
                            }
                        }
                    }
                }

                return inlineCandidates;
            }

        }
    }
}
