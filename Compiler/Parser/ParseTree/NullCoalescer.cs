using Parser.Resolver;
using System.Collections.Generic;

namespace Parser.ParseTree
{
    public class NullCoalescer : Expression
    {
        public Expression PrimaryExpression { get; set; }
        public Expression SecondaryExpression { get; set; }

        public NullCoalescer(Expression primaryExpression, Expression secondaryExpression, Node owner)
            : base(primaryExpression.FirstToken, owner)
        {
            this.PrimaryExpression = primaryExpression;
            this.SecondaryExpression = secondaryExpression;
        }

        internal override IEnumerable<Expression> Descendants { get { return new Expression[] { this.PrimaryExpression, this.SecondaryExpression }; } }

        internal override Expression Resolve(ParserContext parser)
        {
            this.PrimaryExpression = this.PrimaryExpression.Resolve(parser);
            this.SecondaryExpression = this.SecondaryExpression.Resolve(parser);

            if (this.PrimaryExpression is NullConstant)
            {
                return this.SecondaryExpression;
            }

            if (this.PrimaryExpression is IntegerConstant ||
                this.PrimaryExpression is BooleanConstant ||
                this.PrimaryExpression is StringConstant ||
                this.PrimaryExpression is ListDefinition ||
                this.PrimaryExpression is DictionaryDefinition)
            {
                return this.PrimaryExpression;
            }

            return this;
        }

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            this.PrimaryExpression = this.PrimaryExpression.ResolveEntityNames(parser);
            this.SecondaryExpression = this.SecondaryExpression.ResolveEntityNames(parser);
            return this;
        }

        internal override Expression ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            this.PrimaryExpression = this.PrimaryExpression.ResolveTypes(parser, typeResolver);
            this.SecondaryExpression = this.SecondaryExpression.ResolveTypes(parser, typeResolver);
            if (this.PrimaryExpression.ResolvedType == ResolvedType.NULL)
            {
                return this.SecondaryExpression;
            }
            if (this.PrimaryExpression is IConstantValue)
            {
                return this.PrimaryExpression;
            }
            this.ResolvedType = typeResolver.FindCommonAncestor(this.PrimaryExpression.ResolvedType, this.SecondaryExpression.ResolvedType);
            return this;
        }

        internal override void ResolveVariableOrigins(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase)
        {
            if ((phase & VariableIdAllocPhase.ALLOC) != 0)
            {
                this.PrimaryExpression.ResolveVariableOrigins(parser, varIds, phase);
                this.SecondaryExpression.ResolveVariableOrigins(parser, varIds, phase);
            }
        }
    }
}
