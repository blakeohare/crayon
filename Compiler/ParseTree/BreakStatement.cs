using System.Collections.Generic;

namespace Crayon.ParseTree
{
    internal class BreakStatement : Executable
    {
        public BreakStatement(Token breakToken, Executable owner)
            : base(breakToken, owner)
        { }

        internal override IList<Executable> Resolve(Parser parser)
        {
            return Listify(this);
        }

        public override bool IsTerminator { get { return true; } }
        internal override void CalculateLocalIdPass(VariableIdAllocator varIds) { }
        internal override void SetLocalIdPass(VariableIdAllocator varIds) { }

        internal override Executable ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
        {
            return this;
        }

        internal override void GenerateGlobalNameIdManifest(VariableIdAllocator varIds)
        {
            // no assignments
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }
    }
}
