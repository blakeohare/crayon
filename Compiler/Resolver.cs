using System;
using System.Collections.Generic;
using System.Linq;
using Crayon.ParseTree;

namespace Crayon
{
    internal class Resolver
    {
        private Parser parser;
        private Executable[] currentCode;

        public Resolver(Parser parser, IList<Executable> originalCode)
        {
            this.parser = parser;
            this.currentCode = originalCode.ToArray();
        }

        private Dictionary<string, Executable> CreateFullyQualifiedLookup(IList<Executable> code)
        {
            HashSet<string> namespaces = new HashSet<string>();

            Dictionary<string, Executable> lookup = new Dictionary<string, Executable>();
            bool mainFound = false;
            foreach (Executable item in code)
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
                    if (item is Assignment)
                    {
                        error += " Did you mean to mark this as a const expression?";
                    }
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
                lookup[ns] = new Namespace(null, ns, null);
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
                throw new Exception("No main(args) function was defined.");
            }

            return lookup;
        }

        public Executable[] ResolveTranslatedCode()
        {
            this.SimpleFirstPassResolution();
            return this.currentCode;
        }

        public Executable[] ResolveInterpretedCode()
        {
            // Resolve raw names into the actual things they refer to based on namespaces and imports.
            this.ResolveNames();

            this.SimpleFirstPassResolution();

            this.DetermineInlinableLibraryFunctions();

            this.RearrangeClassDefinitions();

            this.AllocateLocalScopeIds();

            return this.currentCode;
        }

        private void DetermineInlinableLibraryFunctions()
        {
            HashSet<FunctionDefinition> inlineCandidates = new HashSet<FunctionDefinition>();
            foreach (FunctionDefinition funcDef in this.currentCode.OfType<FunctionDefinition>())
            {
                // Look for function definitions that are in libraries that have one single line of code that's a return statement that
                // invokes a native code.
                if (funcDef.LibraryName != null && funcDef.Code.Length == 1)
                {
                    ReturnStatement returnStatement = funcDef.Code[0] as ReturnStatement;
                    if (returnStatement != null)
                    {
                        LibraryFunctionCall libraryFunctionCall = returnStatement.Expression as LibraryFunctionCall;
                        if (libraryFunctionCall != null)
                        {
                            bool allSimpleVariables = true;
                            foreach (Expression expr in libraryFunctionCall.Args)
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

        private void ResolveNames()
        {
            Dictionary<string, Executable> definitionsByFullyQualifiedNames = this.CreateFullyQualifiedLookup(this.currentCode);

            this.parser.MainFunctionHasArg = ((FunctionDefinition)definitionsByFullyQualifiedNames["~"]).ArgNames.Length == 1;

            IEnumerable<ClassDefinition> allClasses = this.currentCode.OfType<ClassDefinition>(); // TODO: change this when nested classes are done.

            foreach (ClassDefinition cd in allClasses)
            {
                cd.ResolveBaseClasses(definitionsByFullyQualifiedNames, cd.NamespacePrefixSearch);
            }

            foreach (ClassDefinition cd in allClasses)
            {
                cd.VerifyNoBaseClassLoops();
            }

            foreach (Executable item in this.currentCode)
            {
                item.ResolveNames(this.parser, definitionsByFullyQualifiedNames, item.NamespacePrefixSearch);
            }

            foreach (ClassDefinition cd in allClasses)
            {
                cd.ResolveMemberIds();
            }

            /*
			 * 0 - not resolved
			 * 1 - currently being resolved
			 * 2 - fully resolved
			 */
            foreach (Executable ex in definitionsByFullyQualifiedNames.Values
                .Where<Executable>(ex => ex is ConstStatement || ex is EnumDefinition))
            {
                parser.ConstantAndEnumResolutionState[ex] = 0;
            }
        }

        private void RearrangeClassDefinitions()
        {
            // Rearrange class definitions so that base classes always come first.

            HashSet<int> classIdsIncluded = new HashSet<int>();
            List<ClassDefinition> classDefinitions = new List<ClassDefinition>();
            List<FunctionDefinition> functionDefinitions = new List<FunctionDefinition>();
            List<Executable> output = new List<Executable>();
            foreach (Executable exec in this.currentCode)
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

        private void RearrangeClassDefinitionsHelper(ClassDefinition def, HashSet<int> idsAlreadyIncluded, List<Executable> output)
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

        // This will run for both compiled and translated code.
        private void SimpleFirstPassResolution()
        {
            List<Executable> output = new List<Executable>();
            foreach (Executable line in this.currentCode)
            {
                output.AddRange(line.Resolve(this.parser));
            }

            this.currentCode = output.ToArray();
        }

        private void AllocateLocalScopeIds()
        {
            foreach (Executable item in this.currentCode)
            {
                if (item is FunctionDefinition)
                {
                    ((FunctionDefinition)item).AllocateLocalScopeIds();
                }
                else if (item is ClassDefinition)
                {
                    ((ClassDefinition)item).AllocateLocalScopeIds();
                }
                else
                {
                    throw new System.InvalidOperationException(); // everything else in the root scope should have thrown before now.
                }
            }
        }

        // Convert anything that looks like a function call into a verified pointer to the function if possible using the
        // available namespaces.
        public static List<Executable> CreateVerifiedFunctionCalls(Parser parser, IList<Executable> original)
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

        public static Expression ConvertStaticReferenceToExpression(Executable item, Token primaryToken, Executable owner)
        {
            if (item is Namespace) return new PartialNamespaceReference(primaryToken, ((Namespace)item).Name, owner);
            if (item is ClassDefinition) return new ClassReference(primaryToken, (ClassDefinition)item, owner);
            if (item is EnumDefinition) return new EnumReference(primaryToken, (EnumDefinition)item, owner);
            if (item is ConstStatement) return new ConstReference(primaryToken, (ConstStatement)item, owner);
            if (item is FunctionDefinition) return new FunctionReference(primaryToken, (FunctionDefinition)item, owner);

            throw new System.InvalidOperationException(); // what?
        }
    }
}
