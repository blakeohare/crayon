using System.Collections.Generic;

namespace Parser.ParseTree
{
    public abstract class Node
    {
        internal Node(Token firstToken, Node owner)
        {
            this.FirstToken = firstToken;
            this.Owner = owner;
        }

        public LibraryMetadata Library
        {
            get
            {
                if (this.CompilationScope is LibraryCompilationScope)
                {
                    return ((LibraryCompilationScope)this.CompilationScope).Library;
                }
                return null;
            }
        }

        public Token FirstToken { get; private set; }

        // This is a misnomer. This can be any top-level object such as a function, class, const, or enum that can wrap
        // other executables or expressions.
        public Node Owner { get; private set; }
        private TopLevelEntity topLevelEntity = null;
        public TopLevelEntity TopLevelEntity
        {
            get
            {
                if (this.topLevelEntity == null && this.Owner != null)
                {
                    this.topLevelEntity = (this.Owner as TopLevelEntity) ?? this.Owner.TopLevelEntity;
                }
                return this.topLevelEntity;
            }
        }
        public CompilationScope CompilationScope { get { return this.FileScope.CompilationScope; } }

        protected FileScope fileScopeOverride = null; // Top level items that have no Owner will have this set.
        public FileScope FileScope { get { return this.fileScopeOverride ?? this.Owner.FileScope; } }
        public Localization.Locale Locale { get { return this.CompilationScope.Locale; } }

        internal void BatchTopLevelConstructNameResolver(ParserContext parser, ICollection<TopLevelEntity> constructs)
        {
            foreach (TopLevelEntity tlc in constructs)
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
    }
}
