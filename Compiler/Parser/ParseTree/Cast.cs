using Parser.Resolver;
using System.Collections.Generic;

namespace Parser.ParseTree
{
    public class Cast : Expression
    {
        public AType Type { get; private set; }
        public Expression Expression { get; private set; }
        internal override IEnumerable<Expression> Descendants { get { return new Expression[] { this.Expression }; } }

        public Cast(Token firstToken, AType type, Expression expression, Node owner)
            : base(firstToken, owner)
        {
            this.Type = type;
            this.Expression = expression;
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase)
        {
            this.Expression.PerformLocalIdAllocation(parser, varIds, phase);
        }

        internal override Expression Resolve(ParserContext parser)
        {
            this.Expression = this.Expression.Resolve(parser);
            return this;
        }

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            this.Expression = this.Expression.ResolveEntityNames(parser);
            return this;
        }

        internal override Expression ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            this.ResolvedType = typeResolver.ResolveType(this.Type);
            this.Expression = this.Expression.ResolveTypes(parser, typeResolver);
            if (this.ResolvedType == this.Expression.ResolvedType)
            {
                // TODO: warn about extraneous cast.
                return this.Expression;
            }

            if (this.Expression.ResolvedType.CanAssignToA(this.ResolvedType))
            {
                return this;
            }

            if (this.ResolvedType.CanAssignToA(this.Expression.ResolvedType))
            {
                return this;
            }

            throw new ParserException(this.FirstToken, "Invalid cast.");
        }
    }
}
