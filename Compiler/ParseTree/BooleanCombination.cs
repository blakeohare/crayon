using System;
using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
    internal class BooleanCombination : Expression
    {
        internal override Expression PastelResolve(Parser parser)
        {
            for (int i = 0; i < this.Expressions.Length; ++i)
            {
                this.Expressions[i] = this.Expressions[i].PastelResolve(parser);
            }
            return this;
        }

        public override bool CanAssignTo { get { return false; } }

        public Expression[] Expressions { get; private set; }
        public Token[] Ops { get; private set; }

        public BooleanCombination(IList<Expression> expressions, IList<Token> ops, TopLevelConstruct owner)
            : base(expressions[0].FirstToken, owner)
        {
            this.Expressions = expressions.ToArray();
            this.Ops = ops.ToArray();
        }

        internal override Expression Resolve(Parser parser)
        {
            for (int i = 0; i < this.Expressions.Length; ++i)
            {
                this.Expressions[i] = this.Expressions[i].Resolve(parser);
            }

            if (this.Expressions[0] is BooleanConstant)
            {
                List<Expression> expressions = new List<Expression>(this.Expressions);
                List<Token> ops = new List<Token>(this.Ops);
                while (ops.Count > 0 && expressions[0] is BooleanConstant)
                {
                    bool boolValue = ((BooleanConstant)expressions[0]).Value;
                    bool isAnd = ops[0].Value == "&&";
                    if (isAnd)
                    {
                        if (boolValue)
                        {
                            expressions.RemoveAt(0);
                            ops.RemoveAt(0);
                        }
                        else
                        {
                            return new BooleanConstant(this.FirstToken, false, this.Owner);
                        }
                    }
                    else
                    {
                        if (boolValue)
                        {
                            return new BooleanConstant(this.FirstToken, true, this.Owner);
                        }
                        else
                        {
                            expressions.RemoveAt(0);
                            ops.RemoveAt(0);
                        }
                    }
                }

                if (expressions.Count == 1)
                {
                    return expressions[0];
                }
            }

            return this;
        }

        internal override Expression ResolveNames(Parser parser)
        {
            this.BatchExpressionNameResolver(parser, this.Expressions);
            return this;
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars)
        {
            foreach (Expression expr in this.Expressions)
            {
                expr.GetAllVariablesReferenced(vars);
            }
        }

        internal override void PerformLocalIdAllocation(Parser parser, VariableIdAllocator varIds, VariableIdAllocPhase phase)
        {
            if ((phase & VariableIdAllocPhase.ALLOC) != 0)
            {
                foreach (Expression ex in this.Expressions)
                {
                    ex.PerformLocalIdAllocation(parser, varIds, phase);
                }
            }
        }
    }
}
