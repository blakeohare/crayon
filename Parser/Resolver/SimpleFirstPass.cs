using Common;
using Parser.ParseTree;
using System.Collections.Generic;
using System.Linq;

namespace Parser.Resolver
{
    // This will go through everything and call Resolve on it. This will drop staticly-identifiable dead code, resolve
    // constants (such as consts and enums) into literal values, among other things.
    // Note that the constant and enum definitions are resolved first and then dropped from the final output as they are
    // no longer needed.
    public class SimpleFirstPass
    {
        public TopLevelConstruct[] Run(ParserContext parser, TopLevelConstruct[] currentCode)
        {
            using (new PerformanceSection("SimpleFirstPassResolution"))
            {
                List<TopLevelConstruct> enumsAndConstants = new List<TopLevelConstruct>();
                List<TopLevelConstruct> everythingElse = new List<TopLevelConstruct>();
                foreach (TopLevelConstruct ex in currentCode)
                {
                    if (ex is EnumDefinition || ex is ConstStatement)
                    {
                        enumsAndConstants.Add(ex);
                    }
                    else
                    {
                        everythingElse.Add(ex);
                    }
                }
                List<TopLevelConstruct> output = new List<TopLevelConstruct>();
                foreach (TopLevelConstruct ex in enumsAndConstants.Concat(everythingElse))
                {
                    ex.Resolve(parser);
                }

                return everythingElse.ToArray();
            }
        }
    }
}
