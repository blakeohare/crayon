using System.Collections.Generic;

namespace Crayon.ParseTree
{
    internal class IntegerConstant : Expression, IConstantValue
    {
        internal override Expression PastelResolve(Parser parser)
        {
            return this;
        }

        public override bool CanAssignTo { get { return false; } }

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
                    if (c < '0' && c > '9') throw new ParserException(token, "Invalid integer constant.");
                    number = number * 10 + (c - '0');
                }
            }
            return number;
        }

        public IntegerConstant(Token token, int value, TopLevelConstruct owner)
            : base(token, owner)
        {
            this.Value = value;
        }

        internal override Expression Resolve(Parser parser)
        {
            // doesn't get simpler than this.
            return this;
        }

        internal override Expression ResolveNames(Parser parser)
        {
            return this;
        }

        public Expression CloneValue(Token token, TopLevelConstruct owner)
        {
            return new IntegerConstant(token, this.Value, owner);
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }
        internal override void PerformLocalIdAllocation(Parser parser, VariableIdAllocator varIds, VariableIdAllocPhase phase) { }
    }
}
