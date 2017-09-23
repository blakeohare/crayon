using System.Collections.Generic;

namespace Crayon.ParseTree
{
    internal class ContinueStatement : Executable
    {
        public ContinueStatement(Token continueToken, TopLevelConstruct owner)
            : base(continueToken, owner)
        { }

        internal override IList<Executable> Resolve(Parser parser)
        {
            return Listify(this);
        }

        internal override Executable ResolveNames(Parser parser)
        {
            return this;
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }
        internal override void PerformLocalIdAllocation(Parser parser, VariableIdAllocator varIds, VariableIdAllocPhase phase) { }

        internal override Executable PastelResolve(Parser parser)
        {
            throw new System.NotImplementedException();
        }
    }
}
