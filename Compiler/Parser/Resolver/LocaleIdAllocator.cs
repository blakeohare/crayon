using System.Collections.Generic;

namespace Parser
{
    // Assigns each locale an ID from 0 to n - 1
    internal static class LocaleIdAllocator
    {
        // TODO: merge this with the current ID allocator in ParserContext.
        public static void Run(ParserContext parser, IList<CompilationScope> scopes)
        {
            /*
            foreach (CompilationScope scope in scopes)
            {
                if (!parser.LocaleIds.ContainsKey(scope.Locale))
                {
                    parser.LocaleIds.Add(scope.Locale, parser.LocaleIds.Count);
                }
            }//*/
        }
    }
}
