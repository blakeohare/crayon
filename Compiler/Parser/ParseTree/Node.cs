using System.Collections.Generic;

namespace Parser.ParseTree
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
        public CompilationScope CompilationScope {  get { return this.Owner.FileScope.CompilationScope; } }
        public Localization.Locale Locale { get { return this.CompilationScope.Locale; } }

        internal void BatchTopLevelConstructNameResolver(ParserContext parser, ICollection<TopLevelConstruct> constructs)
        {
            foreach (TopLevelConstruct tlc in constructs)
            {
                tlc.ResolveEntityNames(parser);
            }
        }

        internal void BatchExecutableEntityNameResolver(ParserContext parser, Executable[] executables)
        {
            for (int i = 0; i < executables.Length; ++i)
            {
                executables[i] = executables[i].ResolveEntityNames(parser);
            }
        }

        internal void BatchExpressionEntityNameResolver(ParserContext parser, Expression[] expressions)
        {
            for (int i = 0; i < expressions.Length; ++i)
            {
                if (expressions[i] != null)
                {
                    expressions[i] = expressions[i].ResolveEntityNames(parser);
                }
            }
        }

        internal static ClassDefinition DoClassLookup(TopLevelConstruct currentContainer, Token nameToken, string name)
        {
            return DoClassLookup(currentContainer, nameToken, name, false);
        }

        internal static ClassDefinition DoClassLookup(TopLevelConstruct currentContainer, Token nameToken, string name, bool failSilently)
        {
            TopLevelConstruct ex = currentContainer.FileScope.FileScopeEntityLookup.DoEntityLookup(name, currentContainer);
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
