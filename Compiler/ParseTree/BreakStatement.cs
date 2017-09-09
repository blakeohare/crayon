using System;
using System.Collections.Generic;

namespace Crayon.ParseTree
{
    internal class BreakStatement : Executable
    {
        public BreakStatement(Token breakToken, TopLevelConstruct owner)
            : base(breakToken, owner)
        { }

        internal override IList<Executable> Resolve(Parser parser)
        {
            return Listify(this);
        }

        public override bool IsTerminator { get { return true; } }

        internal override Executable ResolveNames(Parser parser, Dictionary<string, TopLevelConstruct> lookup, string[] imports)
        {
            return this;
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }

        internal override void PerformLocalIdAllocation(Parser parser, VariableIdAllocator varIds, VariableIdAllocPhase phase) { }

        internal override Executable PastelResolve(Parser parser)
        {
            return this;
        }
    }
}
