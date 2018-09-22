using Common;
using Parser.ParseTree;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Parser.Resolver
{
    internal static class ResolverPipeline
    {
        public static TopLevelEntity[] Resolve(ParserContext parser, ICollection<CompilationScope> compilationScopesRaw)
        {
            List<TopLevelEntity> originalCode = new List<TopLevelEntity>();
            foreach (CompilationScope scope in compilationScopesRaw.OrderBy(scope => scope.ScopeKey))
            {
                originalCode.AddRange(scope.GetExecutables_HACK());
            }
            TopLevelEntity[] code = originalCode.ToArray();

            parser.VerifyNoBadImports();

            CompilationScope[] assembliesInDependencyOrder = AssemblyDependencyResolver.GetAssemblyResolutionOrder(parser);
            List<CompilationScope> compilationScopes = new List<CompilationScope>(assembliesInDependencyOrder);
            compilationScopes.Add(parser.RootScope);

            using (new PerformanceSection("ResolveNames for compilation scopes"))
            {
                // Resolve raw names into the actual things they refer to based on namespaces and imports.
                foreach (CompilationScope scope in compilationScopes)
                {
                    using (new PerformanceSection("Resolve Names for: " + scope.ScopeKey))
                    {
                        EntityNameResolver.Resolve(parser, scope);
                    }
                }
            }

            LocalScopeVariableIdAllocator.Run(parser, code.Where(tle => !(tle is ConstDefinition || tle is EnumDefinition)));

            using (new PerformanceSection("Resolve Types"))
            {
                foreach (CompilationScope scope in compilationScopes)
                {
                    using (new PerformanceSection("Resolve types for: " + scope.ScopeKey))
                    {
                        TopLevelEntity[] topLevelEntities = scope.GetTopLevelConstructs();
                        ConstDefinition[] consts = topLevelEntities.OfType<ConstDefinition>().ToArray();
                        EnumDefinition[] enums = topLevelEntities.OfType<EnumDefinition>().ToArray();
                        ClassDefinition[] classes = topLevelEntities.OfType<ClassDefinition>().ToArray();
                        FunctionDefinition[] functions = topLevelEntities.OfType<FunctionDefinition>().ToArray();

                        // Constants are provided in dependency order. Type resolution running in this order
                        // will not encounter anything unknown. Const dependency loops are found by .SortConstants()
                        // Note that this sorting is for the purposes of type resolution. All enums are automatically
                        // known to be integers, so there's possibly still some dependency loops there, which are
                        // caught during the consolidation in the Resolve() phase.

                        TopLevelEntity[] topLevelEntitiesWithoutConstants = new TopLevelEntity[0]
                            .Concat(enums)
                            .Concat(functions)
                            .Concat(classes)
                            .ToArray();

                        // The type of the overall constant is dependent on the expression's type when there are
                        // no type declarations (i.e. Crayon). Because signature types need to be resolved before
                        // the expression itself, these are sorted in dependency order. Enum values, even though
                        // a const expression can depend on them, are always an integer, so they don't need to be
                        // type-resolved at this time.
                        ConstDefinition[] dependencySortedConstants = new ConstantDependencySorter(consts).SortConstants();
                        foreach (ConstDefinition cnst in dependencySortedConstants)
                        {
                            TypeResolver typeResolver = new TypeResolver(cnst);
                            cnst.ResolveTypes(parser, typeResolver);
                            cnst.ValidateConstTypeSignature();
                        }

                        foreach (TopLevelEntity tle in topLevelEntitiesWithoutConstants)
                        {
                            TypeResolver typeResolver = new TypeResolver(tle);
                            tle.ResolveSignatureTypes(parser, typeResolver);
                        }

                        foreach (TopLevelEntity tle in topLevelEntitiesWithoutConstants.Where(t => !(t is EnumDefinition)))
                        {
                            tle.EnsureModifierAndTypeSignatureConsistency();
                        }

                        foreach (TopLevelEntity tle in topLevelEntitiesWithoutConstants)
                        {
                            TypeResolver typeResolver = new TypeResolver(tle);
                            tle.ResolveTypes(parser, typeResolver);
                        }
                    }
                }
            }

            SpecialFunctionFinder.Run(parser);

            List<TopLevelEntity> newCode = new List<TopLevelEntity>();
            foreach (CompilationScope scope in compilationScopes)
            {
                TopLevelEntity[] everything = scope.GetTopLevelConstructs();
                TopLevelEntity[] constsAndEnums = new TopLevelEntity[0]
                    .Concat(everything.OfType<ConstDefinition>())
                    .Concat(everything.OfType<EnumDefinition>())
                    .ToArray();
                TopLevelEntity[] codeContainers = new TopLevelEntity[0]
                    .Concat(everything.OfType<FunctionDefinition>())
                    .Concat(everything.OfType<ClassDefinition>())
                    .ToArray();
                foreach (TopLevelEntity tle in everything.Concat(codeContainers))
                {
                    tle.Resolve(parser);
                }
                newCode.AddRange(DependencyBasedClassSorter.Run(codeContainers));
            }

            code = newCode.ToArray();

            parser.InlinableLibraryFunctions = InlineableLibraryFunctionFinder.Find(code);

            return code;
        }

        // Generally this is used with the name resolver. So for example, you have a refernce to a ClassDefinition
        // instance from the resolver, but you want to turn it into a ClassReference instance.
        // TODO: put this in a method on these classes and implement an interface. The function signatures are all close enough.
        public static Expression ConvertStaticReferenceToExpression(TopLevelEntity item, Token primaryToken, Node owner)
        {
            if (item is ClassDefinition) return new ClassReference(primaryToken, (ClassDefinition)item, owner);
            if (item is EnumDefinition) return new EnumReference(primaryToken, (EnumDefinition)item, owner);
            if (item is ConstDefinition) return new ConstReference(primaryToken, (ConstDefinition)item, owner);
            if (item is FunctionDefinition) return new FunctionReference(primaryToken, (FunctionDefinition)item, owner);

            throw new InvalidOperationException();
        }
    }
}
