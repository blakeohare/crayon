using System;
using System.Collections.Generic;

namespace Parser.ParseTree
{
    public class ClassReference : Expression
    {
        public override bool CanAssignTo { get { return false; } }

        public ClassDefinition ClassDefinition { get; private set; }

        public ClassReference(Token token, ClassDefinition clazz, TopLevelConstruct owner)
            : base(token, owner)
        {
            this.ClassDefinition = clazz;
        }

        internal override Expression Resolve(ParserContext parser)
        {
            // normal usages should be optimized out by now.
            throw new ParserException(this.FirstToken, "Unexpected class reference.");
        }

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            throw new InvalidOperationException(); // Created during the resolve names phase.
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableIdAllocator varIds, VariableIdAllocPhase phase) { }
    }
}
