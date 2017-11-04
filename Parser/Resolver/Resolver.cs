using Common;
using Parser.ParseTree;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Parser.Resolver
{
    internal class ResolverPipeline
    {
        private ParserContext parser;
        private TopLevelConstruct[] currentCode;

        public ResolverPipeline(ParserContext parser, ICollection<CompilationScope> compilationScopes)
        {
            List<TopLevelConstruct> originalCode = new List<TopLevelConstruct>();
            foreach (CompilationScope scope in compilationScopes.OrderBy(scope => scope.ScopeKey))
            {
                originalCode.AddRange(scope.GetExecutables_HACK());
            }
            this.parser = parser;
            this.currentCode = originalCode.ToArray();
        }

        private FunctionDefinition FindMain(UserCodeCompilationScope userCodeScope)
        {
            FunctionDefinition[] mainFunctions = userCodeScope.GetTopLevelConstructs()
                .OfType<FunctionDefinition>()
                .Where(fd => fd.NameToken.Value == this.parser.Keywords.MAIN_FUNCTION)
                .ToArray();
            if (mainFunctions.Length == 1) return mainFunctions[0];
            if (mainFunctions.Length == 0) throw new InvalidOperationException("No main(args) function was defined.");
            throw new ParserException(mainFunctions[0].FirstToken, "Multiple main methods found.");
        }

        public TopLevelConstruct[] ResolveTranslatedCode()
        {
            this.SimpleFirstPassResolution();
            return this.currentCode;
        }

        public TopLevelConstruct[] ResolveInterpretedCode()
        {
            this.parser.VerifyNoBadImports();

            LibraryCompilationScope[] librariesInDependencyOrder = LibraryDependencyResolver.GetLibraryResolutionOrder(this.parser);
            List<CompilationScope> compilationScopes = new List<CompilationScope>(librariesInDependencyOrder);
            compilationScopes.Add(this.parser.UserCodeCompilationScope);

            using (new PerformanceSection("ResolveNames for compilation segments"))
            {
                // Resolve raw names into the actual things they refer to based on namespaces and imports.
                foreach (CompilationScope scope in compilationScopes)
                {
                    Dictionary<string, TopLevelConstruct> scopeLookup = new Dictionary<string, TopLevelConstruct>();
                    Dictionary<string, TopLevelConstruct> depsLookup = new Dictionary<string, TopLevelConstruct>();
                    Dictionary<string, NamespaceReferenceTemplate> depsNamespaceLookup = new Dictionary<string, NamespaceReferenceTemplate>();

                    // First create a lookup of JUST the libraries that are available to this library.
                    // This is localized to the locales that are used by that library.
                    foreach (LocalizedLibraryView depLocView in scope.Dependencies)
                    {
                        depLocView.LibraryScope.FlattenFullyQualifiedLookupsIntoGlobalLookup(depsLookup, depLocView.Locale);
                        Util.MergeDictionaryInto(depLocView.LibraryScope.GetFlattenedNamespaceLookup(depLocView.Locale), depsNamespaceLookup);
                    }

                    scope.FlattenFullyQualifiedLookupsIntoGlobalLookup(scopeLookup, scope.Locale);
                    Dictionary<string, NamespaceReferenceTemplate> scopeNamespaceLookup = scope.GetFlattenedNamespaceLookup(scope.Locale);
                    TopLevelConstruct[] toResolve = scope.GetTopLevelConstructs();

                    this.ResolveNames(toResolve, scopeLookup, depsLookup, scopeNamespaceLookup, depsNamespaceLookup);
                }
            }

            // Determine if the main function uses args.
            FunctionDefinition mainFunction = this.FindMain(this.parser.UserCodeCompilationScope);
            if (mainFunction == null)
            {
                throw new InvalidOperationException("No main(args) function was defined.");
            }
            this.parser.MainFunctionHasArg = mainFunction.ArgNames.Length == 1;

            this.SimpleFirstPassResolution();

            this.DetermineInlinableLibraryFunctions();

            this.RearrangeClassDefinitions();

            this.AllocateLocalScopeIds();

            return this.currentCode;
        }

        private void DetermineInlinableLibraryFunctions()
        {
            using (new PerformanceSection("DetermineInlinableLibraryFunctions"))
            {
                HashSet<FunctionDefinition> inlineCandidates = new HashSet<FunctionDefinition>();

                List<FunctionDefinition> allFlattenedFunctions = new List<FunctionDefinition>(this.currentCode.OfType<FunctionDefinition>());
                foreach (ClassDefinition cd in this.currentCode.OfType<ClassDefinition>())
                {
                    allFlattenedFunctions.AddRange(cd.Methods.Where(fd => fd.IsStaticMethod));
                }

                foreach (FunctionDefinition funcDef in allFlattenedFunctions)
                {
                    // Look for function definitions that are in libraries that have one single line of code that's a return statement that
                    // invokes a native code.
                    if (funcDef.Library != null && funcDef.Code.Length == 1)
                    {
                        ReturnStatement returnStatement = funcDef.Code[0] as ReturnStatement;
                        if (returnStatement != null)
                        {
                            Expression[] argsFromLibOrCoreFunction = null;
                            if (returnStatement.Expression is LibraryFunctionCall)
                            {
                                argsFromLibOrCoreFunction = ((LibraryFunctionCall)returnStatement.Expression).Args;
                            }
                            else if (returnStatement.Expression is CoreFunctionInvocation)
                            {
                                argsFromLibOrCoreFunction = ((CoreFunctionInvocation)returnStatement.Expression).Args;
                            }

                            if (argsFromLibOrCoreFunction != null)
                            {
                                bool allSimpleVariables = true;
                                foreach (Expression expr in argsFromLibOrCoreFunction)
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

                this.parser.InlinableLibraryFunctions = inlineCandidates;
            }
        }

        // Note that the lookup will also contain the toResolve entries plus the entities from the dependencies.
        private void ResolveNames(
            TopLevelConstruct[] toResolve,
            Dictionary<string, TopLevelConstruct> scopeLookup,
            Dictionary<string, TopLevelConstruct> depsLookup,
            Dictionary<string, NamespaceReferenceTemplate> scopeNamespaceLookup,
            Dictionary<string, NamespaceReferenceTemplate> depsNamespaceLookup)
        {
            using (new PerformanceSection("ResolveNames"))
            {
                foreach (FileScope file in new HashSet<FileScope>(toResolve.Select(tlc => tlc.FileScope)))
                {
                    file.FileScopeEntityLookup.InitializeLookups(depsLookup, scopeLookup, depsNamespaceLookup, scopeNamespaceLookup);
                }

                List<ClassDefinition> classes = new List<ClassDefinition>(toResolve.OfType<ClassDefinition>());

                foreach (ClassDefinition cd in classes)
                {
                    if (cd.BaseClassDeclarations.Length > 0)
                    {
                        cd.ResolveBaseClasses();
                    }
                }

                foreach (ClassDefinition cd in classes)
                {
                    cd.VerifyNoBaseClassLoops();
                }

                foreach (TopLevelConstruct item in toResolve)
                {
                    if (!(item is Namespace))
                    {
                        item.ResolveNames(this.parser);
                    }
                }

                foreach (ClassDefinition cd in classes)
                {
                    cd.ResolveMemberIds();
                }

                foreach (TopLevelConstruct ex in toResolve.Where(ex => ex is ConstStatement || ex is EnumDefinition))
                {
                    parser.ConstantAndEnumResolutionState[ex] = ConstantResolutionState.NOT_RESOLVED;
                }
            }
        }

        private void RearrangeClassDefinitions()
        {
            using (new PerformanceSection("RearrangeClassDefinitions"))
            {
                // Rearrange class definitions so that base classes always come first.

                HashSet<int> classIdsIncluded = new HashSet<int>();
                List<ClassDefinition> classDefinitions = new List<ClassDefinition>();
                List<FunctionDefinition> functionDefinitions = new List<FunctionDefinition>();
                List<TopLevelConstruct> output = new List<TopLevelConstruct>();
                foreach (TopLevelConstruct exec in this.currentCode)
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
                        throw new ParserException(exec.FirstToken, "Unexpected item.");
                    }
                }

                output.AddRange(functionDefinitions);

                foreach (ClassDefinition cd in classDefinitions)
                {
                    this.RearrangeClassDefinitionsHelper(cd, classIdsIncluded, output);
                }

                this.currentCode = output.ToArray();
            }
        }

        private void RearrangeClassDefinitionsHelper(ClassDefinition def, HashSet<int> idsAlreadyIncluded, List<TopLevelConstruct> output)
        {
            if (!idsAlreadyIncluded.Contains(def.ClassID))
            {
                if (def.BaseClass != null)
                {
                    this.RearrangeClassDefinitionsHelper(def.BaseClass, idsAlreadyIncluded, output);
                }

                output.Add(def);
                idsAlreadyIncluded.Add(def.ClassID);
            }
        }

        private void SimpleFirstPassResolution()
        {
            using (new PerformanceSection("SimpleFirstPassResolution"))
            {
                List<TopLevelConstruct> enumsAndConstants = new List<TopLevelConstruct>();
                List<TopLevelConstruct> everythingElse = new List<TopLevelConstruct>();
                foreach (TopLevelConstruct ex in this.currentCode)
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
                    ex.Resolve(this.parser);
                }

                this.currentCode = everythingElse.ToArray();
            }
        }

        private void AllocateLocalScopeIds()
        {
            using (new PerformanceSection("AllocateLocalScopeIds"))
            {
                foreach (TopLevelConstruct item in this.currentCode)
                {
                    if (item is FunctionDefinition)
                    {
                        ((FunctionDefinition)item).AllocateLocalScopeIds(this.parser);
                    }
                    else if (item is ClassDefinition)
                    {
                        ((ClassDefinition)item).AllocateLocalScopeIds(this.parser);
                    }
                    else
                    {
                        throw new InvalidOperationException(); // everything else in the root scope should have thrown before now.
                    }
                }
            }
        }

        // Generally this is used with the name resolver. So for example, you have a refernce to a ClassDefinition
        // instance from the resolver, but you want to turn it into a ClassReference instance.
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
