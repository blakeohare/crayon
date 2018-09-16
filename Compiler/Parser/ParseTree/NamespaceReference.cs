using System;

namespace Parser.ParseTree
{
    public class NamespaceReferenceTemplate
    {
        public Namespace OriginalNamespace { get; set; }
        public string Name { get; set; }
    }

    // This serves as an ephemeral instance of an Expression that relays the NamespaceReferenceTemplate to
    // the next parent node of the name resolver. It should never reach the byte code serialization phase.
    public class NamespaceReference : Expression
    {
        public NamespaceReferenceTemplate Template { get; private set; }
        public Namespace OriginalNamespace { get; private set; }
        public Namespace ParentNamespace { get; private set; }
        public string Name { get; private set; }

        public NamespaceReference(Token firstToken, Node owner, NamespaceReferenceTemplate nsRef)
            : base(firstToken, owner)
        {
            this.Template = nsRef;
            this.OriginalNamespace = nsRef.OriginalNamespace;
            this.Name = nsRef.Name;
        }

        public override bool CanAssignTo { get { return false; } }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase) { throw new Exception(); }

        internal override Expression Resolve(ParserContext parser)
        {
            throw new ParserException(this, "Namespace reference was not used.");
        }

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            throw new Exception(); // Generated from the ResolveNames phase.
        }

        internal override void ResolveTypes(ParserContext parser)
        {
            throw new System.NotImplementedException();
        }
    }
}
