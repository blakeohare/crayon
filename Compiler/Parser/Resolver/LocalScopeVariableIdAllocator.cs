using Common;
using Parser.ParseTree;
using System;
using System.Collections.Generic;

namespace Parser.Resolver
{
    internal static class LocalScopeVariableIdAllocator
    {
        public static void Run(ParserContext parser, IList<TopLevelConstruct> code)
        {
            using (new PerformanceSection("AllocateLocalScopeIds"))
            {
                foreach (TopLevelConstruct item in code)
                {
                    if (item is FunctionDefinition)
                    {
                        ((FunctionDefinition)item).AllocateLocalScopeIds(parser);
                    }
                    else if (item is ClassDefinition)
                    {
                        ((ClassDefinition)item).AllocateLocalScopeIds(parser);
                    }
                    else
                    {
                        throw new InvalidOperationException(); // everything else in the root scope should have thrown before now.
                    }
                }
            }
        }
    }
}
