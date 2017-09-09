using System.Collections.Generic;

namespace Crayon.ParseTree
{
    internal class BooleanConstant : Expression, IConstantValue
    {
        internal override Expression PastelResolve(Parser parser)
        {
            return this;
        }

        public override bool CanAssignTo { get { return false; } }

        public bool Value { get; private set; }

        public override bool IsLiteral { get { return true; } }

        public BooleanConstant(Token token, bool value, TopLevelConstruct owner)
            : base(token, owner)
        {
            this.Value = value;
        }

        internal override Expression Resolve(Parser parser)
        {
            return this;
        }

        internal override Expression ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
        {
            return this;
        }

        public Expression CloneValue(Token token, TopLevelConstruct owner)
        {
            return new BooleanConstant(token, this.Value, owner);
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }
        internal override void PerformLocalIdAllocation(Parser parser, VariableIdAllocator varIds, VariableIdAllocPhase phase) { }
    }
}
