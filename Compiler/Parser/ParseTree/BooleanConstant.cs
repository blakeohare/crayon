using System.Collections.Generic;

namespace Parser.ParseTree
{
    public class BooleanConstant : Expression, IConstantValue
    {
        internal override Expression PastelResolve(ParserContext parser)
        {
            return this;
        }

        public override bool IsInlineCandidate {  get { return true; } }

        public override bool CanAssignTo { get { return false; } }

        public bool Value { get; private set; }

        public override bool IsLiteral { get { return true; } }

        public BooleanConstant(Token token, bool value, TopLevelConstruct owner)
            : base(token, owner)
        {
            this.Value = value;
        }

        internal override Expression Resolve(ParserContext parser)
        {
            return this;
        }

        internal override Expression ResolveNames(ParserContext parser)
        {
            return this;
        }

        public Expression CloneValue(Token token, TopLevelConstruct owner)
        {
            return new BooleanConstant(token, this.Value, owner);
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }
        internal override void PerformLocalIdAllocation(ParserContext parser, VariableIdAllocator varIds, VariableIdAllocPhase phase) { }
    }
}
