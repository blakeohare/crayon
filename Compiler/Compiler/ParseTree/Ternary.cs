using Parser.Resolver;
using System.Collections.Generic;

namespace Parser.ParseTree
{
    internal class Ternary : Expression
    {
        public Expression Condition { get; private set; }
        public Expression TrueValue { get; private set; }
        public Expression FalseValue { get; private set; }

        public Ternary(Expression condition, Expression trueValue, Expression falseValue, Node owner)
            : base(condition.FirstToken, owner)
        {
            this.Condition = condition;
            this.TrueValue = trueValue;
            this.FalseValue = falseValue;
        }

        internal override IEnumerable<Expression> Descendants { get { return new Expression[] { this.Condition, this.TrueValue, this.FalseValue }; } }

        internal override Expression Resolve(ParserContext parser)
        {
            this.Condition = this.Condition.Resolve(parser);
            this.TrueValue = this.TrueValue.Resolve(parser);
            this.FalseValue = this.FalseValue.Resolve(parser);

            BooleanConstant bc = this.Condition as BooleanConstant;
            if (bc != null)
            {
                return bc.Value ? this.TrueValue : this.FalseValue;
            }

            return this;
        }

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            this.Condition = this.Condition.ResolveEntityNames(parser);
            this.TrueValue = this.TrueValue.ResolveEntityNames(parser);
            this.FalseValue = this.FalseValue.ResolveEntityNames(parser);
            return this;
        }

        internal override Expression ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            this.Condition = this.Condition.ResolveTypes(parser, typeResolver);
            this.TrueValue = this.TrueValue.ResolveTypes(parser, typeResolver);
            this.FalseValue = this.FalseValue.ResolveTypes(parser, typeResolver);
            if (!this.Condition.ResolvedType.CanAssignToA(parser.TypeContext.BOOLEAN))
            {
                throw new ParserException(this.Condition, "Ternary expression must use a boolean condition.");
            }
            this.ResolvedType = typeResolver.FindCommonAncestor(this.TrueValue.ResolvedType, this.FalseValue.ResolvedType);
            return this;
        }

        internal override void ResolveVariableOrigins(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase)
        {
            this.Condition.ResolveVariableOrigins(parser, varIds, phase);
            this.TrueValue.ResolveVariableOrigins(parser, varIds, phase);
            this.FalseValue.ResolveVariableOrigins(parser, varIds, phase);
        }
    }
}
