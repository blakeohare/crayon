using System.Collections.Generic;

namespace Parser.ParseTree
{
    public class NullCoalescer : Expression
    {
        internal override Expression PastelResolve(ParserContext parser)
        {
            throw new System.NotImplementedException();
        }

        public override bool CanAssignTo { get { return false; } }

        public Expression PrimaryExpression { get; set; }
        public Expression SecondaryExpression { get; set; }

        public NullCoalescer(Expression primaryExpression, Expression secondaryExpression, TopLevelConstruct owner)
            : base(primaryExpression.FirstToken, owner)
        {
            this.PrimaryExpression = primaryExpression;
            this.SecondaryExpression = secondaryExpression;
        }

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

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableIdAllocator varIds, VariableIdAllocPhase phase)
        {
            if ((phase & VariableIdAllocPhase.ALLOC) != 0)
            {
                this.PrimaryExpression.PerformLocalIdAllocation(parser, varIds, phase);
                this.SecondaryExpression.PerformLocalIdAllocation(parser, varIds, phase);
            }
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars)
        {
            throw new System.NotImplementedException();
        }
    }
}
