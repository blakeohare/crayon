using Builder.ParseTree;
using System;
using System.Collections.Generic;

namespace Builder.Resolver
{
    internal static class LocalScopeVariableIdAllocator
    {
        public static void Run(ParserContext parser, IEnumerable<TopLevelEntity> code)
        {
            foreach (TopLevelEntity item in code)
            {
                if (item is FunctionDefinition)
                {
                    ((FunctionDefinition)item).ResolveVariableOrigins(parser);
                }
                else if (item is ClassDefinition)
                {
                    ((ClassDefinition)item).ResolveVariableOrigins(parser);
                }
                else
                {
                    throw new InvalidOperationException(); // everything else in the root scope should have thrown before now.
                }
            }
        }
    }
}
