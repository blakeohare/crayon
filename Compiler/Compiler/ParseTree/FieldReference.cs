using Parser.Resolver;
using System.Collections.Generic;

namespace Parser.ParseTree
{
    internal class FieldReference : Expression
    {
        public override bool IsInlineCandidate
        {
            get
            {
                return this.Field.Modifiers.HasStatic;
            }

        }

        public override bool CanAssignTo { get { return true; } }

        internal override IEnumerable<Expression> Descendants { get { return Expression.NO_DESCENDANTS; } }

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
            throw new ParserException(this, "This should not happen.");
        }

        internal override Expression ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            this.ResolvedType = this.Field.ResolvedFieldType;
            return this;
        }

        internal override void ResolveVariableOrigins(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase) { }
    }
}
