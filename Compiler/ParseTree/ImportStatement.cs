using System;
using System.Collections.Generic;

namespace Crayon.ParseTree
{
    internal class ImportStatement : TopLevelConstruct
    {
        public string ImportPath { get; set; }

        public ImportStatement(Token importToken, string path, Library callingLibrary, FileScope fileScope)
            : base(importToken, null, fileScope)
        {
            this.Library = callingLibrary;
            this.ImportPath = path;
            fileScope.Imports.Add(this);
        }

        internal override void Resolve(Parser parser)
        {
            throw new Exception("Imports shouldn't exist at this point in the compilation pipeline.");
        }

        internal override void ResolveNames(Parser parser, Dictionary<string, TopLevelConstruct> lookup, string[] imports)
        {
            throw new InvalidOperationException();
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }

        internal override void PerformLocalIdAllocation(Parser parser, VariableIdAllocator varIds, VariableIdAllocPhase phase) { }
    }
}
