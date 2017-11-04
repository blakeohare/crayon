using Common;
using Parser.ParseTree;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Parser.Resolver
{
    internal static class ResolverPipeline
    {
        public static TopLevelConstruct[] Resolve(ParserContext parser, ICollection<CompilationScope> compilationScopesRaw)
        {
            List<TopLevelConstruct> originalCode = new List<TopLevelConstruct>();
            foreach (CompilationScope scope in compilationScopesRaw.OrderBy(scope => scope.ScopeKey))
            {
                originalCode.AddRange(scope.GetExecutables_HACK());
            }
            TopLevelConstruct[] code = originalCode.ToArray();

            parser.VerifyNoBadImports();

            LibraryCompilationScope[] librariesInDependencyOrder = LibraryDependencyResolver.GetLibraryResolutionOrder(parser);
            List<CompilationScope> compilationScopes = new List<CompilationScope>(librariesInDependencyOrder);
            compilationScopes.Add(parser.UserCodeCompilationScope);

            using (new PerformanceSection("ResolveNames for compilation scopes"))
            {
                // Resolve raw names into the actual things they refer to based on namespaces and imports.
                foreach (CompilationScope scope in compilationScopes)
                {
                    using (new PerformanceSection("Resolve Names for: " + scope.ScopeKey))
                    {
                        NameResolver.Resolve(parser, scope);
                    }
                }
            }

            FunctionDefinition mainFunction = MainFunctionFinder.Find(parser);

            code = SimpleFirstPass.Run(parser, code);

            parser.InlinableLibraryFunctions = InlineableLibraryFunctionFinder.Find(code);

            code = DependencyBasedClassSorter.Run(code);

            LocalScopeVariableIdAllocator.Run(parser, code);
            LocaleIdAllocator.Run(parser, compilationScopes);

            return code;
        }

        // Generally this is used with the name resolver. So for example, you have a refernce to a ClassDefinition
        // instance from the resolver, but you want to turn it into a ClassReference instance.
        // TODO: put this in a method on these classes and implement an interface. The function signatures are all close enough.
        public static Expression ConvertStaticReferenceToExpression(TopLevelConstruct item, Token primaryToken, TopLevelConstruct owner)
        {
            if (item is ClassDefinition) return new ClassReference(primaryToken, (ClassDefinition)item, owner);
            if (item is EnumDefinition) return new EnumReference(primaryToken, (EnumDefinition)item, owner);
            if (item is ConstStatement) return new ConstReference(primaryToken, (ConstStatement)item, owner);
            if (item is FunctionDefinition) return new FunctionReference(primaryToken, (FunctionDefinition)item, owner);

            throw new InvalidOperationException();
        }
    }
}
