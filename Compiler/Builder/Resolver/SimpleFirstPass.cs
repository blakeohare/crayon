using Builder.ParseTree;
using System.Collections.Generic;
using System.Linq;

namespace Builder.Resolver
{
    // This will go through everything and call Resolve on it. This will drop staticly-identifiable dead code, resolve
    // constants (such as consts and enums) into literal values, among other things.
    // Note that the constant and enum definitions are resolved first and then dropped from the final output as they are
    // no longer needed.
    internal static class SimpleFirstPass
    {
        public static TopLevelEntity[] Run(ParserContext parser, TopLevelEntity[] currentCode)
        {
            List<TopLevelEntity> enumsAndConstants = new List<TopLevelEntity>();
            List<TopLevelEntity> everythingElse = new List<TopLevelEntity>();
            foreach (TopLevelEntity ex in currentCode)
            {
                if (ex is EnumDefinition || ex is ConstDefinition)
                {
                    enumsAndConstants.Add(ex);
                }
                else
                {
                    everythingElse.Add(ex);
                }
            }
            List<TopLevelEntity> output = new List<TopLevelEntity>();
            foreach (TopLevelEntity ex in enumsAndConstants.Concat(everythingElse))
            {
                ex.Resolve(parser);
            }

            return everythingElse.ToArray();
        }
    }
}
