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

        // TODO: get rid of this and use compilation scope directly
        private LibraryMetadata library = null;
        public LibraryMetadata Library
        {
            get
            {
                if (this.library == null && this.TopLevelEntity != null)
                {
                    this.library = this.TopLevelEntity.Library;
                }
                return this.library;
            }
            set { this.library = value; }
        }

        private string GetName(Node thing)
        {
            if (thing == null) return "none";

            if (thing is Expression)
            {
                if (thing is Lambda) return "lambda";
                return "expr";
            }

            if (thing is Executable)
            {
                return "exec";
            }

            if (thing is TopLevelEntity)
            {
                if (thing is FunctionDefinition) return "func";
                if (thing is ClassDefinition) return "class";
                if (thing is ConstructorDefinition) return "ctor";
                if (thing is Namespace) return "namespace";
                if (thing is FieldDefinition) return "field";
                if (thing is EnumDefinition) return "enum";
                if (thing is ConstDefinition) return "const";
                if (thing is ImportStatement) return "import";
            }

            if (thing is Annotation) return "annotation";

            throw new System.Exception();
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
