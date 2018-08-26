using System;

namespace Parser.ParseTree
{
    public class FieldReference : Expression
    {
        public override bool IsInlineCandidate
        {
            get
            {
                return this.Field.IsStaticField;
            }

        }

        public override bool CanAssignTo { get { return true; } }

        public FieldDefinition Field { get; set; }

        public FieldReference(Token token, FieldDefinition field, Node owner)
            : base(token, owner)
        {
            this.Field = field;
        }

        internal override Expression Resolve(ParserContext parser)
        {
            return this;
        }

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            throw new InvalidOperationException(); // created in the resolve name phase.
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase) { }
    }
}
