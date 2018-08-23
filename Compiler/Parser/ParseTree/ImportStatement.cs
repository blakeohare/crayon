using Localization;
using System;
using System.Collections.Generic;

namespace Parser.ParseTree
{
    public class ImportStatement : TopLevelConstruct
    {
        public string ImportPath { get; set; }

        public ImportStatement(Token importToken, string path, LibraryMetadata callingLibrary, FileScope fileScope)
            : base(importToken, null, fileScope)
        {
            this.Library = callingLibrary;
            this.ImportPath = path;
            fileScope.Imports.Add(this);
        }

        public override string GetFullyQualifiedLocalizedName(Locale locale)
        {
            throw new Exception();
        }

        internal override void Resolve(ParserContext parser)
        {
            throw new Exception("Imports shouldn't exist at this point in the compilation pipeline.");
        }

        internal override void ResolveEntityNames(ParserContext parser)
        {
            throw new InvalidOperationException();
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableIdAllocator varIds, VariableIdAllocPhase phase) { }
    }
}
