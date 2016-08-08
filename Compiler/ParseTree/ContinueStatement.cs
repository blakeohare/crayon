using System.Collections.Generic;

namespace Crayon.ParseTree
{
    internal class ContinueStatement : Executable
    {
        public ContinueStatement(Token continueToken, Executable owner)
            : base(continueToken, owner)
        { }

        internal override IList<Executable> Resolve(Parser parser)
        {
            return Listify(this);
        }

        internal override Executable ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
        {
            return this;
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }
        internal override void PerformLocalIdAllocation(VariableIdAllocator varIds, VariableIdAllocPhase phase) { }
    }
}
