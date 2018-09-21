using Parser.Resolver;
using System.Collections.Generic;
using System.Linq;

namespace Parser.ParseTree
{
    public class BooleanCombination : Expression
    {
        public Expression[] Expressions { get; private set; }
        public Token[] Ops { get; private set; }

        public BooleanCombination(IList<Expression> expressions, IList<Token> ops, Node owner)
            : base(expressions[0].FirstToken, owner)
        {
            this.Expressions = expressions.ToArray();
            this.Ops = ops.ToArray();
        }

        internal override IEnumerable<Expression> Descendants { get { return this.Expressions; } }

        internal override Expression Resolve(ParserContext parser)
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

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            this.BatchExpressionEntityNameResolver(parser, this.Expressions);
            return this;
        }

        internal override Expression ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            for (int i = 0; i < this.Expressions.Length; ++i)
            {
                Expression expr = this.Expressions[i];
                expr = expr.ResolveTypes(parser, typeResolver);
                ResolvedType rType = expr.ResolvedType;
                if (rType != ResolvedType.BOOLEAN && rType !=   ResolvedType.ANY)
                {
                    throw new ParserException(expr, "Only a boolean expression can be used here.");
                }
                this.Expressions[i] = expr;
            }
            this.ResolvedType = ResolvedType.BOOLEAN;
            return this;
        }

        internal override void ResolveVariableOrigins(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase)
        {
            if ((phase & VariableIdAllocPhase.ALLOC) != 0)
            {
                foreach (Expression ex in this.Expressions)
                {
                    ex.ResolveVariableOrigins(parser, varIds, phase);
                }
            }
        }
    }
}
