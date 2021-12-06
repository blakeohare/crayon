using Builder.Resolver;
using System.Collections.Generic;

namespace Builder.ParseTree
{
    internal class IntegerConstant : Expression, IConstantValue
    {
        public override bool IsInlineCandidate { get { return true; } }

        internal override IEnumerable<Expression> Descendants { get { return Expression.NO_DESCENDANTS; } }

        public int Value { get; private set; }

        public override bool IsLiteral { get { return true; } }

        private static readonly Dictionary<char, int> DIGIT_VALUES = new Dictionary<char, int>()
        {
            { '0', 0 },
            { '1', 1 },
            { '2', 2 },
            { '3', 3 },
            { '4', 4 },
            { '5', 5 },
            { '6', 6 },
            { '7', 7 },
            { '8', 8 },
            { '9', 9 },
            { 'a', 10 },
            { 'b', 11 },
            { 'c', 12 },
            { 'd', 13 },
            { 'e', 14 },
            { 'f', 15 },
            { 'A', 10 },
            { 'B', 11 },
            { 'C', 12 },
            { 'D', 13 },
            { 'E', 14 },
            { 'F', 15 },
        };

        public static int ParseIntConstant(Token token, string value)
        {
            bool isHex = value.StartsWith("0x");
            int number = 0;
            if (isHex)
            {
                value = value.Substring(2).ToLowerInvariant();
                if (value.Length == 0) throw new ParserException(token, "Invalid hex constant.");
                foreach (char c in value)
                {
                    if (!DIGIT_VALUES.ContainsKey(c)) throw new ParserException(token, "Invalid hex constant.");
                    number = number * 16 + DIGIT_VALUES[c];
                }
            }
            else
            {
                foreach (char c in value)
                {
                    if (c < '0' || c > '9') throw new ParserException(token, "Invalid integer constant.");
                    number = number * 10 + (c - '0');
                }
            }
            return number;
        }

        public IntegerConstant(Token token, int value, Node owner)
            : base(token, owner)
        {
            this.ResolvedType = TypeContext.HACK_REF.INTEGER;
            this.Value = value;
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
            return new IntegerConstant(token, this.Value, owner);
        }

        internal override void ResolveVariableOrigins(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase) { }
    }
}
