using Builder.Resolver;
using System;
using System.Collections.Generic;

namespace Builder.ParseTree
{
    internal class ClassReferenceLiteral : Expression
    {
        public ClassDefinition ClassDefinition { get; set; }

        internal override IEnumerable<Expression> Descendants { get { return Expression.NO_DESCENDANTS; } }

        public ClassReferenceLiteral(Token firstToken, ClassDefinition cd, Node owner)
            : base(firstToken, owner)
        {
            this.ClassDefinition = cd;
        }

        internal override void ResolveVariableOrigins(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase) { }

        internal override Expression Resolve(ParserContext parser) { return this; }

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            // ClassReferenceLiteral is created in the Resolve pass, so this is never called.
            throw new InvalidOperationException();
        }

        internal override Expression ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            this.ResolvedType = ResolvedType.GetClassRefType(parser.TypeContext, this.ClassDefinition);
            return this;
        }
    }
}
