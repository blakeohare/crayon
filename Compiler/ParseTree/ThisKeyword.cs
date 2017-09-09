using System.Collections.Generic;

namespace Crayon.ParseTree
{
    internal class ThisKeyword : Expression
    {
        internal override Expression PastelResolve(Parser parser)
        {
            throw new System.NotImplementedException();
        }

        public override bool CanAssignTo { get { return false; } }

        public ThisKeyword(Token token, TopLevelConstruct owner)
            : base(token, owner)
        {
        }

        internal override Expression Resolve(Parser parser)
        {
            return this;
        }

        internal override Expression ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
        {
            return this;
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }
        internal override void PerformLocalIdAllocation(Parser parser, VariableIdAllocator varIds, VariableIdAllocPhase phase) { }
    }
}
