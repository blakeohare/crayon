using System.Collections.Generic;

namespace Crayon.ParseTree
{
    public abstract class Node
    {
        internal Node(Token firstToken, TopLevelConstruct owner)
        {
            this.FirstToken = firstToken;
            this.Owner = owner;
        }

        public Token FirstToken { get; private set; }

        // This is a misnomer. This can be any top-level object such as a function, class, const, or enum that can wrap
        // other executables or expressions.
        public TopLevelConstruct Owner { get; private set; }

        internal void BatchExecutableNameResolver(Parser parser, Dictionary<string, Executable> lookup, string[] imports, Executable[] executables)
        {
            for (int i = 0; i < executables.Length; ++i)
            {
                executables[i] = executables[i].ResolveNames(parser, lookup, imports);
            }
        }

        internal void BatchExpressionNameResolver(Parser parser, Dictionary<string, Executable> lookup, string[] imports, Expression[] expressions)
        {
            for (int i = 0; i < expressions.Length; ++i)
            {
                if (expressions[i] != null)
                {
                    expressions[i] = expressions[i].ResolveNames(parser, lookup, imports);
                }
            }
        }

        internal abstract void GetAllVariableNames(Dictionary<string, bool> lookup);

        internal abstract void PerformLocalIdAllocation(Parser parser, VariableIdAllocator varIds, VariableIdAllocPhase phase);

        /*
            Resolution order:
            - First check if the name exists as a fully qualified name. This should ALWAYS take full priority as there is no other way to reference it, if overridden in a namespace.
            - Then check for the fully qualified name with the local namespace prefixed to it
            - Walk through the imports and check to see if the fully qualified name exists with any of those as a prefix
            - Then walk through the local namespace, popping off the last namespace component and using that as a prefix for fully qualified names.

         */
        public static Executable DoNameLookup(
            Dictionary<string, Executable> lookup,
            string[] imports,
            string[] localNamespaces,
            string name)
        {
            Executable output;

            // If it exists as a direct fully-qualified name, then stop.
            if (lookup.TryGetValue(name, out output))
            {
                return output;
            }

            // If there is no local namespace, then you already did the fully qualified lookup which is redundant.
            if (localNamespaces.Length > 0)
            {
                string path = localNamespaces[0] + "." + name;
                if (lookup.TryGetValue(path, out output))
                {
                    return output;
                }
            }

            // check each import as a fully qualified name
            foreach (string import in imports)
            {
                string path = import + "." + name;
                if (lookup.TryGetValue(path, out output))
                {
                    return output;
                }
            }

            // walk up the current namespace, popping off the end, to check for fully qualified names.
            for (int i = 1; i < localNamespaces.Length; ++i)
            {
                string path = localNamespaces[i] + "." + name;
                if (lookup.TryGetValue(path, out output))
                {
                    return output;
                }
            }

            // nope. not found.
            return null;
        }

        internal static ClassDefinition DoClassLookup(Token nameToken, Dictionary<string, Executable> lookup, string[] imports, string[] localNamespace, string name)
        {
            return DoClassLookup(nameToken, lookup, imports, localNamespace, name, false);
        }

        internal static ClassDefinition DoClassLookup(Token nameToken, Dictionary<string, Executable> lookup, string[] imports, string[] localNamespace, string name, bool failSilently)
        {
            Executable ex = DoNameLookup(lookup, imports, localNamespace, name);
            if (ex == null)
            {
                if (failSilently)
                {
                    return null;
                }

                string message = "No class named '" + name + "' was found.";
                if (name.Contains("."))
                {
                    message += " Did you forget to import a library?";
                }
                throw new ParserException(nameToken, message);
            }

            if (ex is ClassDefinition)
            {
                return (ClassDefinition)ex;
            }

            // Still throw an exception if the found item is not a class. This is used by code to check if
            // something is a valid variable name or a class name. Colliding with something else is bad.
            throw new ParserException(nameToken, "This is not a class.");
        }
    }
}
