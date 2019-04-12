using AssemblyResolver;
using System.Collections.Generic;

namespace Parser.ParseTree
{
    public abstract class Node
    {
        private int autoGenIdCounter = 0;

        internal Node(Token firstToken, Node owner)
        {
            this.FirstToken = firstToken;
            this.Owner = owner;
        }

        public AssemblyMetadata Assembly
        {
            get { return this.CompilationScope.Metadata; }
        }

        public Token FirstToken { get; private set; }

        // This is a misnomer. This can be any top-level object such as a function, class, const, or enum that can wrap
        // other executables or expressions.
        public Node Owner { get; private set; }
        public ClassDefinition ClassOwner
        {
            get
            {
                Node walker = this.Owner;
                while (walker != null && !(walker is ClassDefinition))
                {
                    walker = walker.Owner;
                }
                return walker as ClassDefinition;
            }
        }

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

        internal static void EnsureAccessIsAllowed(Token throwToken, Node callerLocation, TopLevelEntity referencedEntity)
        {
            if (!IsAccessAllowed(callerLocation, referencedEntity))
            {
                // TODO: better wording: "The field/function 'foo' is not visible from the class 'FooClass' due to its access scope."
                // TODO: even better: "...it is marked as protected but does not inherit from 'AbstractFooClass'" etc.
                throw new ParserException(throwToken, "This class member is not visible from here due to its access scope.");
            }
        }

        private static ClassDefinition GetWrappingClassOfNodeIfNotAlreadyAClass(Node node)
        {
            while (node != null && !(node is ClassDefinition))
            {
                node = node.Owner;
            }
            if (node == null) return null;
            return (ClassDefinition)node;
        }

        internal static bool IsAccessAllowed(Node callerLocation, TopLevelEntity referencedEntity)
        {
            AccessModifierType invokedAccessType = referencedEntity.Modifiers.AccessModifierType;

            if (invokedAccessType == AccessModifierType.PUBLIC)
            {
                return true;
            }

            bool sameScope = referencedEntity.CompilationScope == callerLocation.CompilationScope;

            if (invokedAccessType == AccessModifierType.INTERNAL) return sameScope;
            if (invokedAccessType == AccessModifierType.INTERNAL_PROTECTED)
            {
                if (!sameScope) return false;
            }

            ClassDefinition memberClass = GetWrappingClassOfNodeIfNotAlreadyAClass(referencedEntity);
            ClassDefinition callerClass = GetWrappingClassOfNodeIfNotAlreadyAClass(callerLocation);
            bool sameClass = memberClass == callerClass;

            if (sameClass || invokedAccessType == AccessModifierType.PRIVATE)
            {
                return sameClass;
            }

            // at this point, we are only left with PROTECTED and
            // INTERNAL_PROTECTED (where the INTERNAL part was already verified) and we
            // know that the calling site does not occur in the member's class.

            ClassDefinition classWalker = callerClass;
            while (classWalker != null)
            {
                classWalker = classWalker.BaseClass;
                if (classWalker == memberClass) return true;
            }
            return false;
        }

        internal string GetNextAutoGenVarName()
        {
            return "$gen_" + this.autoGenIdCounter++;
        }
    }
}
