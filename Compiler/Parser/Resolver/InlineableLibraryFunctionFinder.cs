using Common;
using Parser.ParseTree;
using System.Collections.Generic;
using System.Linq;

namespace Parser.Resolver
{
    internal static class InlineableLibraryFunctionFinder
    {
        public static HashSet<FunctionDefinition> Find(ICollection<TopLevelEntity> entities)
        {
            using (new PerformanceSection("DetermineInlinableLibraryFunctions"))
            {
                HashSet<FunctionDefinition> inlineCandidates = new HashSet<FunctionDefinition>();

                List<FunctionDefinition> allFlattenedFunctions = new List<FunctionDefinition>(entities.OfType<FunctionDefinition>());
                foreach (ClassDefinition cd in entities.OfType<ClassDefinition>())
                {
                    allFlattenedFunctions.AddRange(cd.Methods.Where(fd => fd.Modifiers.HasStatic));
                }

                foreach (FunctionDefinition funcDef in allFlattenedFunctions)
                {
                    // Look for function definitions that are in libraries that have one single line of code that's a return statement that
                    // invokes a native code.
                    if (!funcDef.Assembly.IsUserDefined && funcDef.Code.Length == 1)
                    {
                        ReturnStatement returnStatement = funcDef.Code[0] as ReturnStatement;
                        if (returnStatement != null)
                        {
                            Expression[] argsFromCoreFunction = null;
                            if (returnStatement.Expression is CoreFunctionInvocation)
                            {
                                argsFromCoreFunction = ((CoreFunctionInvocation)returnStatement.Expression).Args;

                                bool allSimpleVariables = true;
                                foreach (Expression expr in argsFromCoreFunction)
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
