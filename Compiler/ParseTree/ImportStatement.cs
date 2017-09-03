using System;
using System.Collections.Generic;

namespace Crayon.ParseTree
{
    internal class ImportStatement : Executable
    {
        public string ImportPath { get; set; }

        public ImportStatement(Token importToken, string path)
            : base(importToken, null)
        {
            this.ImportPath = path;
        }

        internal override IList<Executable> Resolve(Parser parser)
        {
            throw new Exception("Imports shouldn't exist at this point in the compilation pipeline.");
        }

        internal override Executable ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
        {
            throw new InvalidOperationException();
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }

        internal override void PerformLocalIdAllocation(Parser parser, VariableIdAllocator varIds, VariableIdAllocPhase phase) { }

        internal override Executable PastelResolve(Parser parser)
        {
            throw new System.NotImplementedException();
        }
    }
}
