using Builder.Resolver;
using System.Collections.Generic;

namespace Builder.ParseTree
{
    internal class BooleanNot : Expression
    {
        public Expression Root { get; private set; }

        public BooleanNot(Token bang, Expression root, Node owner)
            : base(bang, owner)
        {
            this.Root = root;
        }

        internal override IEnumerable<Expression> Descendants { get { return new Expression[] { this.Root }; } }

        internal override Expression Resolve(ParserContext parser)
        {
            this.Root = this.Root.Resolve(parser);

            if (this.Root is BooleanConstant)
            {
                return new BooleanConstant(this.FirstToken, !((BooleanConstant)this.Root).Value, this.Owner);
            }

            return this;
        }

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            this.Root = this.Root.ResolveEntityNames(parser);
            return this;
        }

        internal override Expression ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            this.Root = this.Root.ResolveTypes(parser, typeResolver);
            TypeContext tc = parser.TypeContext;
            if (this.Root.ResolvedType == tc.ANY || this.Root.ResolvedType == tc.BOOLEAN)
            {
                this.ResolvedType = tc.BOOLEAN;
                return this;
            }
            throw new ParserException(this.FirstToken, "Cannot apply ! to an expression of this type.");
        }

        internal override void ResolveVariableOrigins(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase)
        {
            if ((phase & VariableIdAllocPhase.ALLOC) != 0)
            {
                this.Root.ResolveVariableOrigins(parser, varIds, phase);
            }
        }
    }
}
