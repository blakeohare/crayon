using Builder.Resolver;
using System.Collections.Generic;

namespace Builder.ParseTree
{
    internal class FloatConstant : Expression, IConstantValue
    {
        public override bool IsInlineCandidate { get { return true; } }

        internal override IEnumerable<Expression> Descendants { get { return Expression.NO_DESCENDANTS; } }

        public double Value { get; private set; }

        public override bool IsLiteral { get { return true; } }

        public FloatConstant(Token startValue, double value, Node owner)
            : base(startValue, owner)
        {
            this.ResolvedType = TypeContext.HACK_REF.FLOAT;
            this.Value = value;
        }

        // Parses the value with strict format rules (optional int, dot, optional int)
        // Using the built-in float parser is subject to locale rules.
        public static double ParseValue(Token firstToken, string fullValue)
        {
            string[] parts = fullValue.Split('.');
            bool isValid = true;
            if (parts.Length == 1) parts = new string[] { parts[0], "0" };
            else if (parts.Length != 2) isValid = false;
            else if (parts[0].Length == 0)
            {
                parts[0] = "0";
            }

            long intPortion = 0;
            long decPortion = 0;
            long denominator = 1;
            if (isValid)
            {
                string left = parts[0];
                string right = parts[1];

                for (int i = 0; i < left.Length; ++i)
                {
                    char c = left[i];
                    int d = c - '0';
                    if (d < 0 || d > 9) isValid = false;
                    intPortion = intPortion * 10 + (c - '0');
                }

                for (int i = 0; i < right.Length; ++i)
                {
                    char c = right[i];
                    int d = c - '0';
                    if (d < 0 || d > 9) isValid = false;
                    decPortion = decPortion * 10 + (c - '0');
                    denominator *= 10;
                }
            }

            if (!isValid) throw new ParserException(firstToken, "Invalid float literal.");

            return intPortion + (1.0 * decPortion / denominator);
        }

        internal override Expression Resolve(ParserContext parser)
        {
            return this;
        }

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            return this;
        }

        internal override Expression ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            return this;
        }

        public Expression CloneValue(Token token, Node owner)
        {
            return new FloatConstant(token, this.Value, owner);
        }

        internal override void ResolveVariableOrigins(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase) { }
    }
}
