using System;
using System.Collections.Generic;

namespace Crayon.ParseTree
{
    // These are ephemeral references to a portion of a namespace and ideally occur only as part of
    // a DotStep chain that resolves into something concrete during the name resolution phase.
    // Otherwise, throw an error during the Resolve phase.
    public class PartialNamespaceReference : Expression
    {
        internal override Expression PastelResolve(ParserContext parser)
        {
            throw new NotImplementedException();
        }

        public override bool CanAssignTo { get { return false; } }

        public string Name { get; set; }

        public PartialNamespaceReference(Token token, string name, TopLevelConstruct owner)
            : base(token, owner)
        {
            this.Name = name;
        }

        internal override Expression Resolve(ParserContext parser)
        {
            throw new ParserException(this.FirstToken, "Dangling reference to a namespace.");
        }

        internal override Expression ResolveNames(ParserContext parser)
        {
            throw new InvalidOperationException(); // created during the name resolution phase.
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableIdAllocator varIds, VariableIdAllocPhase phase) { }
        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }
    }
}
