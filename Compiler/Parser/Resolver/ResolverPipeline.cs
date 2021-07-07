using AssemblyResolver;
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
            Dictionary<string, CompilationScope> compilationScopeLookup = new Dictionary<string, CompilationScope>();
            foreach (CompilationScope scope in compilationScopesRaw.OrderBy(scope => scope.ScopeKey))
            {
                originalCode.AddRange(scope.GetTopLevelEntities());
                compilationScopeLookup[scope.Metadata.ID] = scope;
            }
            TopLevelEntity[] code = originalCode.ToArray();

            parser.VerifyNoBadImports();

            ExternalAssemblyMetadata[] assembliesInDependencyOrder = AssemblyDependencyUtil.GetAssemblyResolutionOrder(parser.ScopeManager.ImportedAssemblyScopes.Select(scope => scope.Metadata));

            List<CompilationScope> compilationScopes = new List<CompilationScope>(
                assembliesInDependencyOrder.Select(asm => compilationScopeLookup[asm.ID]));
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
                        TopLevelEntity[] topLevelEntities = scope.GetTopLevelEntities();
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

                            // This shouldn't pick anything up, but it'll fire any errors when undeclared variable-like expressions are used.
                            cnst.Expression.ResolveVariableOrigins(parser, VariableScope.NewEmptyScope(false), VariableIdAllocPhase.REGISTER_AND_ALLOC);

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
                TopLevelEntity[] everything = scope.GetTopLevelEntities();
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
            Expression output = null;
            TopLevelEntity referencedEntity = null;
            if (item is ClassDefinition)
            {
                ClassDefinition classDef = (ClassDefinition)item;
                output = new ClassReference(primaryToken, classDef, owner);
                referencedEntity = classDef;
            }
            else if (item is EnumDefinition)
            {
                EnumDefinition enumDef = (EnumDefinition)item;
                output = new EnumReference(primaryToken, enumDef, owner);
                referencedEntity = enumDef;
            }
            else if (item is ConstDefinition)
            {
                ConstDefinition constDef = (ConstDefinition)item;
                output = new ConstReference(primaryToken, constDef, owner);
                referencedEntity = constDef;
            }
            else if (item is FunctionDefinition)
            {
                FunctionDefinition funcDef = (FunctionDefinition)item;
                output = new FunctionReference(primaryToken, funcDef, owner);
                referencedEntity = funcDef;
            }
            else
            {
                throw new InvalidOperationException();
            }

            Node.EnsureAccessIsAllowed(primaryToken, owner, referencedEntity);

            return output;
        }
    }
}
