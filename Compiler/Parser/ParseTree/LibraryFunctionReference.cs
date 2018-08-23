using System;
using System.Collections.Generic;

namespace Parser.ParseTree
{
    public class LibraryFunctionReference : Expression
    {
        public override bool CanAssignTo { get { return false; } }

        public string Name { get; private set; }

        public LibraryFunctionReference(Token token, string name, TopLevelConstruct owner)
            : base(token, owner)
        {
            this.Name = name;
        }

        internal override Expression Resolve(ParserContext parser)
        {
            throw new ParserException(this.FirstToken, "Library functions cannot be passed around as references. They can only be invoked.");
        }

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            throw new InvalidOperationException(); // Created during resolve name phase.
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableIdAllocator varIds, VariableIdAllocPhase phase) { }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars)
        {
            throw new NotImplementedException();
        }
    }
}
