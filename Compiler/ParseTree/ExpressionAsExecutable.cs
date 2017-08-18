﻿using System;
using System.Collections.Generic;

namespace Crayon.ParseTree
{
    internal class ExpressionAsExecutable : Executable
    {
        public Expression Expression { get; private set; }

        public ExpressionAsExecutable(Expression expression, Executable owner)
            : base(expression.FirstToken, owner)
        {
            this.Expression = expression;
        }

        internal override IList<Executable> Resolve(Parser parser)
        {
            this.Expression = this.Expression.Resolve(parser);

            if (this.Expression == null)
            {
                return new Executable[0];
            }

            if (this.Expression is Increment)
            {
                Increment inc = (Increment)this.Expression;
                Assignment output = new Assignment(
                    inc.Root,
                    inc.IncrementToken,
                    inc.IsIncrement ? "+=" : "-=",
                    new IntegerConstant(inc.IncrementToken, 1, this.FunctionOrClassOwner),
                    this.FunctionOrClassOwner);
                return output.Resolve(parser);
            }

            return Listify(this);
        }

        internal override Executable ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
        {
            this.Expression = this.Expression.ResolveNames(parser, lookup, imports);
            return this;
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars)
        {
            this.Expression.GetAllVariablesReferenced(vars);
        }

        internal override void PerformLocalIdAllocation(VariableIdAllocator varIds, VariableIdAllocPhase phase)
        {
            if ((phase & VariableIdAllocPhase.ALLOC) != 0)
            {
                this.Expression.PerformLocalIdAllocation(varIds, phase);
            }
        }

        internal override Executable PastelResolve(Parser parser)
        {
            this.Expression = this.Expression.PastelResolve(parser);
            return this;
        }
    }
}
