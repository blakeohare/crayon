using Builder.ParseTree;
using System.Collections.Generic;

namespace Builder.Resolver
{
    // Rearranges the class definitions so that all base class declarations come first.
    // The makes metadata initialization at runtime easier.
    internal static class DependencyBasedClassSorter
    {
        public static TopLevelEntity[] Run(IList<TopLevelEntity> code)
        {
            // Rearrange class definitions so that base classes always come first.

            HashSet<int> classIdsIncluded = new HashSet<int>();
            List<ClassDefinition> classDefinitions = new List<ClassDefinition>();
            List<FunctionDefinition> functionDefinitions = new List<FunctionDefinition>();
            List<TopLevelEntity> output = new List<TopLevelEntity>();
            foreach (TopLevelEntity exec in code)
            {
                if (exec is FunctionDefinition)
                {
                    functionDefinitions.Add((FunctionDefinition)exec);
                }
                else if (exec is ClassDefinition)
                {
                    classDefinitions.Add((ClassDefinition)exec);
                }
                else
                {
                    throw new ParserException(exec, "Unexpected item.");
                }
            }

            output.AddRange(functionDefinitions);

            foreach (ClassDefinition cd in classDefinitions)
            {
                RearrangeClassDefinitionsHelper(cd, classIdsIncluded, output);
            }

            return output.ToArray();
        }

        private static void RearrangeClassDefinitionsHelper(ClassDefinition def, HashSet<int> idsAlreadyIncluded, List<TopLevelEntity> output)
        {
            if (!idsAlreadyIncluded.Contains(def.ClassID))
            {
                if (def.BaseClass != null)
                {
                    RearrangeClassDefinitionsHelper(def.BaseClass, idsAlreadyIncluded, output);
                }

                output.Add(def);
                idsAlreadyIncluded.Add(def.ClassID);
            }
        }
    }
}
