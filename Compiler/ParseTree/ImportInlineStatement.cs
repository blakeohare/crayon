using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon.ParseTree
{
    class ImportInlineStatement : Executable
    {
        public string Path { get; set; }
        public string Library { get; set; }

        public ImportInlineStatement(Token importToken, Executable owner, string library, string filepath)
            : base(importToken, owner)
        {
            this.Path = filepath;
            this.Library = library;
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }

        internal override void PerformLocalIdAllocation(VariableIdAllocator varIds, VariableIdAllocPhase phase) { }

        internal override IList<Executable> Resolve(Parser parser)
        {
            return Listify(this);
        }

        internal override Executable ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
        {
            return this;
        }

        internal override Executable PastelResolve(Parser parser)
        {
            return this;
        }
    }
}
