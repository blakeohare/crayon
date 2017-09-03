using System;
using System.Collections.Generic;

namespace Crayon.ParseTree
{
    // These are ephemeral references to a portion of a namespace and ideally occur only as part of
    // a DotStep chain that resolves into something concrete during the name resolution phase.
    // Otherwise, throw an error during the Resolve phase.
    internal class PartialNamespaceReference : Expression
    {
        internal override Expression PastelResolve(Parser parser)
        {
            throw new NotImplementedException();
        }

        public override bool CanAssignTo { get { return false; } }

        public string Name { get; set; }

        public PartialNamespaceReference(Token token, string name, Executable owner)
            : base(token, owner)
        {
            this.Name = name;
        }

        internal override Expression Resolve(Parser parser)
        {
            throw new ParserException(this.FirstToken, "Dangling reference to a namespace.");
        }

        internal override Expression ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
        {
            throw new InvalidOperationException(); // created during the name resolution phase.
        }

        internal override void PerformLocalIdAllocation(Parser parser, VariableIdAllocator varIds, VariableIdAllocPhase phase) { }
        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }
    }
}
