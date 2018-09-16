using Common;

namespace Parser.ParseTree
{
    public class FloatConstant : Expression, IConstantValue
    {
        public override bool IsInlineCandidate { get { return true; } }

        public override bool CanAssignTo { get { return false; } }

        public double Value { get; private set; }

        public override bool IsLiteral { get { return true; } }

        public FloatConstant(Token startValue, double value, Node owner)
            : base(startValue, owner)
        {
            this.Value = value;
        }

        public static double ParseValue(Token firstToken, string fullValue)
        {
            double value;
            if (!Util.ParseDouble(fullValue, out value))
            {
                throw new ParserException(firstToken, "Invalid float literal.");
            }
            return value;
        }

        internal override Expression Resolve(ParserContext parser)
        {
            return this;
        }

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            return this;
        }

        internal override void ResolveTypes(ParserContext parser)
        {
            throw new System.NotImplementedException();
        }

        public Expression CloneValue(Token token, Node owner)
        {
            return new FloatConstant(token, this.Value, owner);
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase) { }
    }
}
