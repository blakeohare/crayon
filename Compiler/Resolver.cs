using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Crayon.ParseTree;

namespace Crayon
{
    internal class Resolver
    {
        private Parser parser;
        private TopLevelConstruct[] currentCode;

        public Resolver(Parser parser, ICollection<CompilationScope> compilationScopes)
        {
            List<TopLevelConstruct> originalCode = new List<TopLevelConstruct>();
            foreach (CompilationScope scope in compilationScopes
                .OrderBy(scope => scope.Library == null ? "" : scope.Library.Metadata.Name))
            {
                originalCode.AddRange(scope.GetExecutables_HACK());
            }
            this.parser = parser;
            this.currentCode = originalCode.ToArray();
        }

        private Dictionary<string, TopLevelConstruct> CreateFullyQualifiedLookup(IList<Executable> code)
        {
            using (new PerformanceSection(""))
            {
                HashSet<string> namespaces = new HashSet<string>();

                Dictionary<string, TopLevelConstruct> lookup = new Dictionary<string, TopLevelConstruct>();
                bool mainFound = false;
                foreach (TopLevelConstruct item in code)
                {
                    string ns;
                    string memberName;
                    if (item is FunctionDefinition)
                    {
                        FunctionDefinition fd = (FunctionDefinition)item;
                        ns = fd.Namespace;
                        memberName = fd.NameToken.Value;
                        if (memberName == "main")
                        {
                            if (mainFound)
                            {
                                throw new ParserException(item.FirstToken, "Multiple main methods found.");
                            }
                            mainFound = true;
                            lookup["~"] = item;
                        }
                    }
                    else if (item is ClassDefinition)
                    {
                        ClassDefinition cd = (ClassDefinition)item;
                        ns = cd.Namespace;
                        memberName = cd.NameToken.Value;

                        // TODO: nested classes, constants, and enums.
                    }
                    else if (item is EnumDefinition)
                    {
                        EnumDefinition ed = (EnumDefinition)item;
                        ns = ed.Namespace;
                        memberName = ed.Name;
                    }
                    else if (item is ConstStatement)
                    {
                        ConstStatement cs = (ConstStatement)item;
                        ns = cs.Namespace;
                        memberName = cs.Name;
                    }
                    else
                    {
                        string error = "This sort of expression cannot exist outside of function or field definitions.";
                        throw new ParserException(item.FirstToken, error);
                    }

                    if (ns.Length > 0)
                    {
                        string accumulator = "";
                        foreach (string nsPart in ns.Split('.'))
                        {
                            if (accumulator.Length > 0) accumulator += ".";
                            accumulator += nsPart;
                            namespaces.Add(accumulator);
                        }
                    }

                    string fullyQualifiedName = (ns.Length > 0 ? (ns + ".") : "") + memberName;

                    if (lookup.ContainsKey(fullyQualifiedName))
                    {
                        // TODO: token information from two locations
                        throw new ParserException(item.FirstToken, "Two items have identical fully-qualified names: '" + fullyQualifiedName + "'");
                    }
                    lookup[fullyQualifiedName] = item;
                }

                foreach (string key in lookup.Keys)
                {
                    if (namespaces.Contains(key))
                    {
                        throw new ParserException(lookup[key].FirstToken, "This name collides with a namespace definition.");
                    }
                }

                // Go through and fill in all the partially qualified namespace names.
                foreach (string ns in namespaces)
                {
                    Namespace nsInstance = new Namespace(null, ns, null, null);
                    string possibleLibraryName = ns.Split('.')[0];

                    TODO.EnglishLocaleAssumed();
                    Library library = this.parser.LibraryManager.GetLibraryFromName(possibleLibraryName);
                    if (library != null)
                    {
                        // TODO: once you get rid of this line, make the Library setter protected
                        nsInstance.Library = library;
                    }
                    lookup[ns] = nsInstance;
                }

                if (lookup.ContainsKey("~"))
                {
                    FunctionDefinition mainFunc = (FunctionDefinition)lookup["~"];
                    if (mainFunc.ArgNames.Length > 1)
                    {
                        throw new ParserException(mainFunc.FirstToken, "The main function must accept 0 or 1 arguments.");
                    }
                }
                else
                {
                    throw new InvalidOperationException("No main(args) function was defined.");
                }

                return lookup;
            }
        }

        public TopLevelConstruct[] ResolveTranslatedCode()
        {
            this.SimpleFirstPassResolution();
            return this.currentCode;
        }

        public TopLevelConstruct[] ResolveInterpretedCode()
        {
            this.parser.VerifyNoBadImports();

            Dictionary<string, TopLevelConstruct> definitionsByFullyQualifiedNames = this.CreateFullyQualifiedLookup(this.currentCode);

            Library[] librariesInDependencyOrder = LibraryDependencyResolver.GetLibraryResolutionOrder(this.parser);

            // Populate lookups of executables based on library.
            Dictionary<Library, Dictionary<string, Executable>> definitionsByLibrary = new Dictionary<Library, Dictionary<string, Executable>>();
            Dictionary<string, Executable> nonLibraryCode = new Dictionary<string, Executable>();
            foreach (string exKey in definitionsByFullyQualifiedNames.Keys)
            {
                Executable ex = definitionsByFullyQualifiedNames[exKey];
                if (ex.Library == null)
                {
                    nonLibraryCode[exKey] = ex;
                }
                else
                {
                    Library library = ex.Library;
                    Dictionary<string, Executable> lookup;
                    if (!definitionsByLibrary.TryGetValue(library, out lookup))
                    {
                        lookup = new Dictionary<string, Executable>();
                        definitionsByLibrary[library] = lookup;
                    }
                    lookup[exKey] = ex;
                }
            }

            using (new PerformanceSection("ResolveNames for compilation segments"))
            {
                Dictionary<string, Executable> alreadyResolvedDependencies;
                // Resolve raw names into the actual things they refer to based on namespaces and imports.
                foreach (Library library in librariesInDependencyOrder)
                {
                    // First create a lookup of JUST the libraries that are available to this library.
                    alreadyResolvedDependencies = Common.Util.MergeDictionaries<string, Executable>(
                        library.LibraryDependencies.Select(lib => definitionsByLibrary[lib]).ToArray());

                    // Resolve definitions based on what's available.
                    this.ResolveNames(library, alreadyResolvedDependencies, definitionsByLibrary[library]);
                }
                alreadyResolvedDependencies = Common.Util.MergeDictionaries<string, Executable>(
                    this.parser.LibraryManager.LibrariesUsed.Select(lib => definitionsByLibrary[lib]).ToArray());
                nonLibraryCode.Remove("~");
                this.ResolveNames(null, alreadyResolvedDependencies, nonLibraryCode);
            }

            // Determine if the main function uses args.
            FunctionDefinition mainFunction = (FunctionDefinition)definitionsByFullyQualifiedNames["~"];
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
                foreach (FunctionDefinition funcDef in this.currentCode.OfType<FunctionDefinition>())
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
                                    if (!(expr is Variable))
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

        private void ResolveNames(
            Library nullableLibrary,
            Dictionary<string, Executable> alreadyResolved,
            Dictionary<string, Executable> currentLibraryDefinitions)
        {
            using (new PerformanceSection("ResolveNames"))
            {
                List<ClassDefinition> classes = new List<ClassDefinition>();

                // Concatenate compilation items on top of everything that's already been resolved to create a lookup of everything that is available for this library.
                Dictionary<string, Executable> allKnownDefinitions = new Dictionary<string, Executable>(alreadyResolved);
                foreach (string executableKey in currentLibraryDefinitions.Keys)
                {
                    if (allKnownDefinitions.ContainsKey(executableKey))
                    {
                        throw new ParserException(
                            currentLibraryDefinitions[executableKey].FirstToken,
                            "Two conflicting definitions of '" + executableKey + "'");
                    }
                    Executable ex = currentLibraryDefinitions[executableKey];
                    if (ex is ClassDefinition)
                    {
                        classes.Add((ClassDefinition)ex);
                    }
                    allKnownDefinitions[executableKey] = ex;
                }

                foreach (ClassDefinition cd in classes)
                {
                    if (cd.BaseClassDeclarations.Length > 0)
                    {
                        cd.ResolveBaseClasses(allKnownDefinitions, cd.LocalNamespace, cd.NamespacePrefixSearch);
                    }
                }

                foreach (ClassDefinition cd in classes)
                {
                    cd.VerifyNoBaseClassLoops();
                }

                foreach (string itemKey in currentLibraryDefinitions.Keys.OrderBy(key => key))
                {
                    Executable item = currentLibraryDefinitions[itemKey];
                    if (!(item is Namespace))
                    {
                        item.ResolveNames(this.parser, allKnownDefinitions, item.NamespacePrefixSearch);
                    }
                }

                foreach (ClassDefinition cd in classes)
                {
                    cd.ResolveMemberIds();
                }

                foreach (Executable ex in currentLibraryDefinitions.Values.Where(ex => ex is ConstStatement || ex is EnumDefinition))
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
                List<Executable> enumsAndConstants = new List<Executable>();
                List<Executable> everythingElse = new List<Executable>();
                foreach (Executable ex in this.currentCode)
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
                    output.AddRange(ex.ResolveTopLevel(this.parser));
                }

                this.currentCode = output.ToArray();
            }
        }

        private void AllocateLocalScopeIds()
        {
            using (new PerformanceSection("AllocateLocalScopeIds"))
            {
                foreach (Executable item in this.currentCode)
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

        // Convert anything that looks like a function call into a verified pointer to the function if possible using the
        // available namespaces.
        public static List<Executable> CreateVerifiedFunctionCalls(Parser parser, IList<Executable> original)
        {
            using (new PerformanceSection("CreateVerifiedFunctionCalls"))
            {
                // First create a fully-qualified lookup of all functions and classes.
                Dictionary<string, Executable> functionsAndClasses = new Dictionary<string, Executable>();
                foreach (Executable exec in original)
                {
                    if (exec is FunctionDefinition)
                    {
                        FunctionDefinition fd = (FunctionDefinition)exec;
                        string key = fd.Namespace + ":" + fd.NameToken.Value;
                        functionsAndClasses[key] = fd;
                    }
                    else if (exec is ClassDefinition)
                    {
                        ClassDefinition cd = (ClassDefinition)exec;
                        string key = cd.Namespace + ":" + cd.NameToken.Value;
                        functionsAndClasses[key] = cd;
                    }
                }

                List<Executable> output = new List<Executable>();

                return output;
            }
        }

        // Generally this is used with the name resolver. So for example, you have a refernce to a ClassDefinition
        // instance from the resolver, but you want to turn it into a ClassReference instance.
        public static Expression ConvertStaticReferenceToExpression(Executable item, Token primaryToken, TopLevelConstruct owner)
        {
            if (item is Namespace) return new PartialNamespaceReference(primaryToken, ((Namespace)item).Name, owner);
            if (item is ClassDefinition) return new ClassReference(primaryToken, (ClassDefinition)item, owner);
            if (item is EnumDefinition) return new EnumReference(primaryToken, (EnumDefinition)item, owner);
            if (item is ConstStatement) return new ConstReference(primaryToken, (ConstStatement)item, owner);
            if (item is FunctionDefinition) return new FunctionReference(primaryToken, (FunctionDefinition)item, owner);

            throw new InvalidOperationException();
        }
    }
}
