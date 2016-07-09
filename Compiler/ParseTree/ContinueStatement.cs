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
