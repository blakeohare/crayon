using Parser.Resolver;
using System;
using System.Collections.Generic;

namespace Parser.ParseTree
{
    public class EnumReference : Expression
    {
        public EnumDefinition EnumDefinition { get; set; }

        public EnumReference(Token token, EnumDefinition enumDefinition, Node owner)
            : base(token, owner)
        {
            this.EnumDefinition = enumDefinition;
        }

        public override bool CanAssignTo { get { return false; } }

        internal override IEnumerable<Expression> Descendants { get { return Expression.NO_DESCENDANTS; } }

        internal override Expression Resolve(ParserContext parser)
        {
            return this;
        }

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            throw new NotImplementedException();
        }

        internal override Expression ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            throw new System.NotImplementedException();
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase) { }
    }
}
