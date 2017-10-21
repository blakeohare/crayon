using System.Collections.Generic;

namespace Crayon.ParseTree
{
    internal class NullConstant : Expression, IConstantValue
    {
        internal override Expression PastelResolve(Parser parser)
        {
            return this;
        }

        public override bool IsInlineCandidate { get { return true; } }

        public override bool CanAssignTo { get { return false; } }

        public NullConstant(Token token, TopLevelConstruct owner)
            : base(token, owner)
        { }

        public override bool IsLiteral { get { return true; } }

        internal override Expression Resolve(Parser parser)
        {
            return this;
        }

        internal override Expression ResolveNames(Parser parser)
        {
            return this;
        }

        public Expression CloneValue(Token token, TopLevelConstruct owner)
        {
            return new NullConstant(token, owner);
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }
        internal override void PerformLocalIdAllocation(Parser parser, VariableIdAllocator varIds, VariableIdAllocPhase phase) { }
    }
}
