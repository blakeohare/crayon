﻿using System.Collections.Generic;

namespace Crayon.ParseTree
{
    internal class NullCoalescer : Expression
    {
        internal override Expression PastelResolve(Parser parser)
        {
            throw new System.NotImplementedException();
        }

        public override bool CanAssignTo { get { return false; } }

        public Expression PrimaryExpression { get; set; }
        public Expression SecondaryExpression { get; set; }

        public NullCoalescer(Expression primaryExpression, Expression secondaryExpression, Executable owner)
            : base(primaryExpression.FirstToken, owner)
        {
            this.PrimaryExpression = primaryExpression;
            this.SecondaryExpression = secondaryExpression;
        }

        internal override Expression Resolve(Parser parser)
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

        internal override Expression ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
        {
            this.PrimaryExpression = this.PrimaryExpression.ResolveNames(parser, lookup, imports);
            this.SecondaryExpression = this.SecondaryExpression.ResolveNames(parser, lookup, imports);
            return this;
        }

        internal override void PerformLocalIdAllocation(VariableIdAllocator varIds, VariableIdAllocPhase phase)
        {
            if ((phase & VariableIdAllocPhase.ALLOC) != 0)
            {
                this.PrimaryExpression.PerformLocalIdAllocation(varIds, phase);
                this.SecondaryExpression.PerformLocalIdAllocation(varIds, phase);
            }
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars)
        {
            throw new System.NotImplementedException();
        }
    }
}
