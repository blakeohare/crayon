using Parser.Resolver;
using System;
using System.Collections.Generic;

namespace Parser.ParseTree
{
    internal class NamespaceReferenceTemplate
    {
        public string Name { get; set; }

        public Namespace OriginalNamespace { get; set; }
        // If the OriginalNamespace has other things after the Name that this NRT is associated with
        // (for example, this NRT may be for "MyLibrary" but the namespace reference is for a "MyLibrary.Internal"),
        // this number ensures which level this namespace reference is associated with. In the given example,
        // that namespace has 2 levels, but the value here will be 1.
        public int OriginalNamespaceDepthClipping { get; set; }

        private string fullyQualifiedNameCache = null;
        public string GetFullyQualifiedName()
        {
            if (fullyQualifiedNameCache != null)
            {
                return fullyQualifiedNameCache;
            }

            string value = this.OriginalNamespace.FullyQualifiedDefaultName;
            if (this.OriginalNamespaceDepthClipping == 1 && !value.Contains("."))
            {
                return value;
            }
            string[] parts = value.Split('.');
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(parts[0]);
            for (int i = 1; i < this.OriginalNamespaceDepthClipping; ++i)
            {
                sb.Append('.');
                sb.Append(parts[i]);
            }

            fullyQualifiedNameCache = sb.ToString();

            return fullyQualifiedNameCache;
        }
    }

    // This serves as an ephemeral instance of an Expression that relays the NamespaceReferenceTemplate to
    // the next parent node of the name resolver. It should never reach the byte code serialization phase.
    internal class NamespaceReference : Expression
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

        internal override IEnumerable<Expression> Descendants { get { return Expression.NO_DESCENDANTS; } }

        internal override void ResolveVariableOrigins(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase) { throw new Exception(); }

        internal override Expression Resolve(ParserContext parser)
        {
            throw new ParserException(this, "Namespace reference was not used.");
        }

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            throw new Exception(); // Generated from the ResolveNames phase.
        }

        internal override Expression ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            throw new NotImplementedException();
        }
    }
}
